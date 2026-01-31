using HabitTracker.Application.DTOs;

namespace HabitTrack_UI.Services.Api
{
    public class AuthApiClient
    {
        private readonly ApiClient _apiClient;

        public AuthApiClient(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Task<TokenResponseDTO> Login(LoginRequest request)
        {
            return _apiClient.PostAsync<TokenResponseDTO>("api/auth/login", request);
        }

        public Task Register(RegisterRequest request)
        {
             return _apiClient.PostAsync<object>("api/auth/register", request);
        }

        public Task<TokenResponseDTO> RefreshToken(RefreshTokenRequestDTO request)
        {
            return _apiClient.PostAsync<TokenResponseDTO>("api/auth/refresh-token", request);
        }

        public async Task Logout()
        {
            await _apiClient.PutAsync("api/auth/logout", null);
        }

        public Task<UserResponseDTO> Me()
        {
            return _apiClient.GetAsync<UserResponseDTO>("api/auth/me");
        }
    }
}
