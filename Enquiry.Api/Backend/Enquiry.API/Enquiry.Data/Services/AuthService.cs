using Enquiry.Data.DTOs;
using Enquiry.Data.Model;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Npgsql;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Enquiry.Data.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString; // Store the connection string 
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger; // Logging

        public AuthService(IOptions<DatabaseSettings> dbSettings, IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger)
        {
            _connectionString = dbSettings.Value.ConnectionString;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        // Register new user
        public async Task<ApiResponse> RegisterAsync(RegisterRequest request)
        {
            // LogDebug: Method Entry
            _logger.LogDebug("[{Class}] {Method}: Entering with Email: {Email}, FullName: {FullName}",
                nameof(AuthService), nameof(RegisterAsync), request.email, request.fullName);
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // LogDebug: Before DB Call
                    _logger.LogDebug("[{Class}] {Method}: Executing PostgreSQL function register_user for Email: {Email}",
                        nameof(AuthService), nameof(RegisterAsync), request.email);

                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT register_user(@p_fullname, @p_email, @p_password, @p_role, @p_createddate)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_fullname", request.fullName);
                        cmd.Parameters.AddWithValue("p_email", request.email);

                        // Hash password BEFORE sending to DB

                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.password);
                        cmd.Parameters.AddWithValue("p_password", hashedPassword);
                        cmd.Parameters.AddWithValue("p_role", "User");
                        cmd.Parameters.AddWithValue("p_createddate", DateTime.UtcNow);

                        // gets the first column of the first row
                        object result = await cmd.ExecuteScalarAsync();
                        int newUserId = Convert.ToInt32(result);
                        if (newUserId == -1)
                        {
                            // LogWarning: Business rule issue (Expected failure)
                            _logger.LogWarning("[{Class}] {Method}: Registration failed - Email {Email} is already registered",
                                nameof(AuthService), nameof(RegisterAsync), request.email);

                            return new ApiResponse { success = false, message = "User with this email already exists" };
                        }
                        // LogInformation: Successful operation
                        _logger.LogInformation("[{Class}] {Method}: User {Email} registered successfully with ID: {UserId}",
                            nameof(AuthService), nameof(RegisterAsync), request.email, newUserId);

                        return new ApiResponse { success = true, message = "User registered successfully", data = newUserId };
                    }
                }
            }
            catch (Exception ex)
            {
                // LogError: Handled exception with full exception object
                _logger.LogError(ex, "[{Class}] {Method}: Unexpected error during registration for Email: {Email}",
                    nameof(AuthService), nameof(RegisterAsync), request.email);
                return new ApiResponse { success = false, message = "Registration failed due to an internal error." };
            }
        }

        // Login user
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // LogDebug: Method Entry
            _logger.LogDebug("[{Class}] {Method}: Login attempt started for Email: {Email}",
                nameof(AuthService), nameof(LoginAsync), request.email);

            // Find user by email
            try
            {
                Users user = null;
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM login_user(@p_email)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_email", request.email);
                        using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read()) // If we found a user
                            {
                                user = new Users
                                {
                                    userId = Convert.ToInt32(reader["userid"]),
                                    fullName = reader["fullname"].ToString(),
                                    email = reader["email"].ToString(),
                                    password = reader["password"].ToString(), // Hashed password
                                    role = reader["role"].ToString(),
                                    ProfileImage = reader["profileimage"] != DBNull.Value ? reader["profileimage"].ToString() : null
                                };
                            }
                        }
                    }
                }
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.password, user.password))
                {
                    // LogWarning: Business rule (Incorrect credentials)
                    _logger.LogWarning("[{Class}] {Method}: Failed login attempt for Email: {Email}",
                        nameof(AuthService), nameof(LoginAsync), request.email);

                    return new LoginResponse { success = false, message = "Invalid email or password" };
                }

                // LogInformation: Success
                _logger.LogInformation("[{Class}] {Method}: User {Email} (ID: {UserId}) logged in successfully",
                    nameof(AuthService), nameof(LoginAsync), user.email, user.userId);

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

                return new LoginResponse
                {
                    success = true,
                    token = token,
                    user = new UserDto
                    {
                        userId = user.userId,
                        email = user.email,
                        fullName = user.fullName,
                        ProfileImage = user.ProfileImage,
                        role = user.role
                    },
                    expiresAt = expiresAt,
                    message = "Login successful"
                };
            }
            catch (Exception ex) 
            {
                // LogError with Identifiers
                _logger.LogError(ex, "[{Class}] {Method}: Error during login for Email: {Email}",
                    nameof(AuthService), nameof(LoginAsync), request.email);
                return new LoginResponse { success = false, message = "Login failed due to an internal error." };
            }
        }


        public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request)
        {
            _logger.LogDebug("[{Class}] {Method}: Google login attempt started", 
                nameof(AuthService), nameof(GoogleLoginAsync));

            try
            {
                // Verify Google Token
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { "262492784884-o61fo0dg5q6uu1gjcunkqgv44j5nug0e.apps.googleusercontent.com" }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

                _logger.LogDebug("[{Class}] {Method}: Google token validated for Email: {Email}", 
                    nameof(AuthService), nameof(GoogleLoginAsync), payload.Email);

                // Call Stored Procedure
                Users user = null;
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM google_login(@p_email, @p_fullname, @p_googleid, @p_profileimage)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_email", payload.Email);
                        cmd.Parameters.AddWithValue("p_fullname", payload.Name);
                        cmd.Parameters.AddWithValue("p_googleid", payload.Subject);
                        cmd.Parameters.AddWithValue("p_profileimage", payload.Picture);
                        using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                user = new Users
                                {
                                    userId = Convert.ToInt32(reader["userid"]),
                                    fullName = reader["fullname"].ToString(),
                                    email = reader["email"].ToString(),
                                    role = reader["role"].ToString(),
                                    ProfileImage = reader["profileimage"] != DBNull.Value ? reader["profileimage"].ToString() : null
                                };
                            }
                        }
                    }
                }

                _logger.LogInformation("[{Class}] {Method}: Google user {Email} logged in successfully", 
                    nameof(AuthService), nameof(GoogleLoginAsync), user.email);

                // Generate Token
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
                return new LoginResponse
                {
                    success = true,
                    token = token,
                    user = new UserDto { userId = user.userId, email = user.email, fullName = user.fullName, role = user.role, ProfileImage = user.ProfileImage },
                    expiresAt = expiresAt,
                    message = "Google login successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Class}] {Method}: Google login failed", 
                    nameof(AuthService), nameof(GoogleLoginAsync));
                return new LoginResponse { success = false, message = "Google login failed" };
            }
        }


        public async Task<LoginResponse> MicrosoftLoginAsync(MicrosoftLoginRequest request)
        {
            _logger.LogDebug("[{Class}] {Method}: Microsoft login attempt started", 
                nameof(AuthService), nameof(MicrosoftLoginAsync));

            try
            {
                // Validate Token
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(request.Token);
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username" || c.Type == "email")?.Value;
                var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                var oid = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(oid))
                {
                    _logger.LogWarning("[{Class}] {Method}: Invalid Microsoft token payload", 
                        nameof(AuthService), nameof(MicrosoftLoginAsync));
                    return new LoginResponse { success = false, message = "Invalid Microsoft Token" };
                }

                _logger.LogDebug("[{Class}] {Method}: Microsoft token validated for Email: {Email}", 
                    nameof(AuthService), nameof(MicrosoftLoginAsync), email);

                // Call Stored Procedure
                Users user = null;
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM microsoft_login(@p_email, @p_fullname, @p_microsoftid)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_email", email);
                        cmd.Parameters.AddWithValue("p_fullname", name ?? "Microsoft User");
                        cmd.Parameters.AddWithValue("p_microsoftid", oid);
                        using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                user = new Users
                                {
                                    userId = Convert.ToInt32(reader["userid"]),
                                    fullName = reader["fullname"].ToString(),
                                    email = reader["email"].ToString(),
                                    role = reader["role"].ToString(),
                                    ProfileImage = reader["profileimage"] != DBNull.Value ? reader["profileimage"].ToString() : null
                                };
                            }
                        }
                    }
                }

                _logger.LogInformation("[{Class}] {Method}: Microsoft user {Email} logged in successfully", 
                    nameof(AuthService), nameof(MicrosoftLoginAsync), user.email);

                // Generate Token
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
                return new LoginResponse
                {
                    success = true,
                    token = token,
                    user = new UserDto { userId = user.userId, email = user.email, fullName = user.fullName, role = user.role, ProfileImage = user.ProfileImage },
                    expiresAt = expiresAt,
                    message = "Microsoft Login Successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Class}] {Method}: Microsoft login failed", 
                    nameof(AuthService), nameof(MicrosoftLoginAsync));
                return new LoginResponse { success = false, message = "Microsoft login failed" };
            }
        }

        //Update Profile
        public async Task<ApiResponse> UpdateProfileAsync(UpdateUserDto request)
        {
            _logger.LogDebug("[{Class}] {Method}: Profile update attempt for UserId: {UserId}", 
                nameof(AuthService), nameof(UpdateProfileAsync), request.UserId);

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM update_profile(@p_userid, @p_fullname, @p_profileimage)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_userid", request.UserId);
                        cmd.Parameters.AddWithValue("p_fullname", request.FullName);

                        // Handle optional profile image
                        if (string.IsNullOrEmpty(request.ProfileImage))
                            cmd.Parameters.AddWithValue("p_profileimage", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("p_profileimage", request.ProfileImage);

                        using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                // Map the updated user data
                                var updatedUser = new Users
                                {
                                    userId = Convert.ToInt32(reader["userid"]),
                                    fullName = reader["fullname"].ToString(),
                                    email = reader["email"].ToString(),
                                    role = reader["role"].ToString(),
                                    ProfileImage = reader["profileimage"] != DBNull.Value ? reader["profileimage"].ToString() : null
                                };

                                _logger.LogInformation("[{Class}] {Method}: Successfully updated profile for UserId: {UserId}", 
                                    nameof(AuthService), nameof(UpdateProfileAsync), request.UserId);

                                return new ApiResponse
                                {
                                    success = true,
                                    message = "Profile updated successfully",
                                    data = updatedUser
                                };
                            }
                        }
                    }
                }

                _logger.LogWarning("[{Class}] {Method}: Profile update failed - User not found or result empty for UserId: {UserId}", 
                    nameof(AuthService), nameof(UpdateProfileAsync), request.UserId);

                return new ApiResponse { success = false, message = "User not found or update failed" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Class}] {Method}: Error occurred during profile update for UserId: {UserId}", 
                    nameof(AuthService), nameof(UpdateProfileAsync), request.UserId);

                return new ApiResponse { success = false, message = "An error occurred while updating the profile." };
            }
        }

        // Generate JWT Token
        private string GenerateJwtToken(Users user)
        {
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Role, user.role),
        new Claim("fullName", user.fullName),
        new Claim("userId", user.userId.ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}