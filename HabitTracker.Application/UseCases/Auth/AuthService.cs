using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;

namespace HabitTracker.Application.UseCases.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenGenerator _jwtGenerator;

        public AuthService(IUserRepository userRepository, ITokenGenerator jwtGenerator)
        {
            _userRepository = userRepository;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<Result<UserDTO>> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                return Result<UserDTO>.Failure("User already exists with this email");

            var user = new UserEntity(request.UserName, request.Email);

            var passwordHash = new PasswordHasher<UserEntity>()
                .HashPassword(user, request.Password);

            user.SetPassword(passwordHash);

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return Result<UserDTO>.Success(MapToDto(user));
        }

        public async Task<Result<TokenResponseDTO>> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.VerifyPassword(request.Password))
                return Result<TokenResponseDTO>.Failure("Invalid email or password");

            var tokenResponse = await CreateTokenResponseAsync(user);
            return Result<TokenResponseDTO>.Success(tokenResponse);
        }

        public async Task<Result<TokenResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

            if (user == null || !user.IsValidRefreshToken(request.RefreshToken))
                return Result<TokenResponseDTO>.Failure("Invalid refresh token");

            var tokenResponse = await CreateTokenResponseAsync(user);

            return Result<TokenResponseDTO>.Success(tokenResponse);
        }

        public async Task<Result> LogoutAsync(Guid userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null)
                return Result.Failure("User not found");

            user.UpdateRefreshToken(null, DateTime.UtcNow);
            await _userRepository.SaveChangesAsync();

            return Result.Success();
        }

        private async Task<TokenResponseDTO> CreateTokenResponseAsync(UserEntity user)
        {
            var refreshToken = GenerateRefreshToken();

            user.UpdateRefreshToken(
                refreshToken,
                DateTime.UtcNow.AddDays(7)
            );

            await _userRepository.SaveChangesAsync();

            return new TokenResponseDTO
            {
                AccessToken = _jwtGenerator.GenerateToken(user),
                RefreshToken = refreshToken
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private static UserDTO MapToDto(UserEntity user)
        {
            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName
            };
        }
    }
}
