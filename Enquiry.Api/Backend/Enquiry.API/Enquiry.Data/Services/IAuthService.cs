using Enquiry.Data.DTOs;
using Enquiry.Data.Model;

namespace Enquiry.Data.Services
{
    public interface IAuthService
    {
        Task<ApiResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request);
        Task<LoginResponse> MicrosoftLoginAsync(MicrosoftLoginRequest request);
        Task<ApiResponse> UpdateProfileAsync(UpdateUserDto request);
    }
}