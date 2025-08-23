using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using HabitTracker.Application.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using HabitTracker.Application.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace HabitTracker.Application.UseCases.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppSettings _appSettings;
        private readonly IUserRepository _userRepository;
        private readonly ITokenGenerator _jwtGenerator;
        public AuthService(IOptions<AppSettings> appSettings, IUserRepository userRepository, ITokenGenerator jwtGenerator)
        {
            _appSettings = appSettings.Value;
            _userRepository = userRepository;
            _jwtGenerator = jwtGenerator;
        }

        // register a new user
        public async Task<User?> RegisterAsync(UserDTO request)
        {
            var existUser = await _userRepository.GetByUsernameAsync(request.UserName);
            if (existUser != null)
                throw new ArgumentException("User already exist with this username");

            var user = new User();
            var hashPassword = new PasswordHasher<User>()
               .HashPassword(user, request.Password);

            user.UserName = request.UserName;
            user.PasswordHashed = hashPassword;

            await _userRepository.AddUserAsync(user);

            return user;
        }

        // log in method
        public async Task<TokenResponseDTO?> LoginAsync(UserDTO request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.UserName);
            if (user == null)
            {
                throw new UnauthorizedAccessException("username or password invalid");
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHashed, request.Password)
                == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("username or password invalid");
            }

            return await CreateTokenResponse(user); ;
        }

        //Generates refresh token
        public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
                return null;

            return await CreateTokenResponse(user);
        }

        // Create token response
        private async Task<TokenResponseDTO> CreateTokenResponse(User user)
        {
            return new TokenResponseDTO
            {
                AccessToken = _jwtGenerator.GenerateToken(user),
                RefreshToken = GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _userRepository.GetById(userId);
            if (user is null || user.RefreshToken != refreshToken
                || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            return refreshToken;
        }

    }
}
