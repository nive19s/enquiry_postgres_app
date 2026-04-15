using System.ComponentModel.DataAnnotations;

namespace Enquiry.Data.DTOs
{
    // Login Request
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;
    }

    // Login Response
    public class LoginResponse
    {
        public bool success { get; set; }
        public string token { get; set; } = string.Empty;
        public UserDto? user { get; set; }
        public DateTime expiresAt { get; set; }
        public string message { get; set; } = string.Empty;
    }

    // User DTO (what we send to frontend)
    public class UserDto
    {
        public int userId { get; set; }
        public string email { get; set; } = string.Empty;
        public string fullName { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
    }

    // Register Request
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string password { get; set; } = string.Empty;

        [Required]
        public string fullName { get; set; } = string.Empty;
    }

    // Generic API Response
    public class ApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public object? data { get; set; }
    }

    // Update Profile Request
    public class UpdateUserDto
    {
        [Required]
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
    }
}