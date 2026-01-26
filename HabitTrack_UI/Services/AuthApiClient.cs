using HabitTracker.Application.DTOs;
using System.Net.Http.Json;

namespace HabitTrack_UI.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _http;
        public AuthApiClient(HttpClient http)
        {
            _http = http;
            Console.WriteLine(_http.BaseAddress);
        }

        public async Task<TokenResponseDTO> Login(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", request);

            await EnsureSuccess(response);

            return (await response.Content.ReadFromJsonAsync<TokenResponseDTO>())!;
        }

        public async Task Register(RegisterRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", request);

            await EnsureSuccess(response);
        }

        public async Task<TokenResponseDTO> RefreshToken(RefreshTokenRequestDTO request)
        {
            var response = await _http.PostAsJsonAsync("api/auth/refresh-token", request);

            await EnsureSuccess(response);

            return (await response.Content.ReadFromJsonAsync<TokenResponseDTO>())!;
        }

        public async Task Logout()
        {
            var response = await _http.PutAsync("api/auth/logout", content: null);
            await EnsureSuccess(response);
        }

        public async Task<UserResponseDTO> Me()
        {
            var response = await _http.GetAsync("api/auth/me");
            await EnsureSuccess(response);

            return (await response.Content.ReadFromJsonAsync<UserResponseDTO>())!;
        }

        private static async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception(content);
            }
        }
    }
}
