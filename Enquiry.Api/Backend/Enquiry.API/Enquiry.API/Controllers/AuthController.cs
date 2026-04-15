using Enquiry.Data.DTOs;
using Enquiry.Data.Model;
using Enquiry.Data.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;


namespace Enquiry.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("allowCors")]
    public class AuthController : ControllerBase
    {        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    success = false,
                    message = "Invalid request data",
                    data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _authService.RegisterAsync(request);

            if (!result.success)
            {
                return BadRequest(result);
            }

            return StatusCode(201, result);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponse
                {
                    success = false,
                    message = "Invalid request data"
                });
            }

            var result = await _authService.LoginAsync(request);

            if (!result.success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        // POST: api/Auth/google-login
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponse
                {
                    success = false,
                    message = "Invalid request data"
                });
            }

            var result = await _authService.GoogleLoginAsync(request);

            if (!result.success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        // POST: api/Auth/microsoft-login
        [HttpPost("microsoft-login")]
        public async Task<IActionResult> MicrosoftLogin([FromBody] MicrosoftLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponse
                {
                    success = false,
                    message = "Invalid request data"
                });
            }

            var result = await _authService.MicrosoftLoginAsync(request);

            if (!result.success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }


        // POST: api/Auth/update-profile
        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    success = false,
                    message = "Invalid request data"
                });
            }
            var result = await _authService.UpdateProfileAsync(request);
            if (!result.success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}