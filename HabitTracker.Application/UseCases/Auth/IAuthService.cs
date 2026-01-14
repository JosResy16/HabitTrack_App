using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;

namespace HabitTracker.Application.UseCases.Auth
{
    public interface IAuthService
    {
        Task<Result<UserDTO>> RegisterAsync(RegisterRequest request);
        Task<Result<TokenResponseDTO>> LoginAsync(LoginRequest request);
        Task<Result<TokenResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request);
        Task<Result> LogoutAsync(Guid userId);
    }
}
