using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Auth;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Aplication.Tests.UseCases.User
{
    class AuthServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock = null!;
        private Mock<ITokenGenerator> _tokenGeneratorMock = null!;
        private AuthService _authService = null!;

        private readonly static string _username = "user";
        private readonly static string _email = "test@email.com";
        private readonly static string _password = "password";

        [SetUp]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenGeneratorMock = new Mock<ITokenGenerator>();

            _authService = new AuthService(_userRepositoryMock.Object, _tokenGeneratorMock.Object);
        }

        #region Login

        [Test]
        public async Task LoginAsync_WithValidCredential_ReturnsToken()
        {
            var token = "test-jwt";
            var request = new LoginRequest
            {
                Email = _email,
                Password = _password
            };

            var user = new UserEntity(_username, _email);
            user.SetPassword(new PasswordHasher<UserEntity>().HashPassword(user, _password));

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);
            _tokenGeneratorMock.Setup(r => r.GenerateToken(It.IsAny<UserEntity>())).Returns(token);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _authService.LoginAsync(request);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.AccessToken, Is.EqualTo("test-jwt"));
            Assert.That(result.Value.RefreshToken, Is.Not.Null.And.Not.Empty);

            _userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _tokenGeneratorMock.Verify(t => t.GenerateToken(It.IsAny<UserEntity>()), Times.Once);
        }

        [Test]
        public async Task LoginAsync_WithInvalidPassword_ReturnsFailure()
        {
            var user = new UserEntity(_username, _email);
            user.SetPassword(new PasswordHasher<UserEntity>().HashPassword(user, _password));

            var request = new LoginRequest
            {
                Email = user.Email,
                Password = "wrongPassword"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);

            var response = await _authService.LoginAsync(request);

            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.ErrorMessage, Is.EqualTo("Invalid email or password"));
        }

        [Test]
        public async Task LoginAsync_WhenUserDoesNotExist_ReturnsFailure()
        {
            var request = new LoginRequest
            {
                Email = _email,
                Password = _password
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email))
                .ReturnsAsync((UserEntity?)null);

            var result = await _authService.LoginAsync(request);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Invalid email or password"));
        }


        #endregion

        #region RegisterUser

        [Test]
        public async Task RegisterUser_WithValidData_ReturnsSuccess()
        {
            var request = new RegisterRequest
            {
                Email = _email,
                UserName = _username,
                Password = _password
            };

            var user = new UserEntity(request.UserName, request.Email);
            user.SetPassword(new PasswordHasher<UserEntity>().HashPassword(user, request.Password));

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((UserEntity?)null);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _authService.RegisterAsync(request);

            _userRepositoryMock.Verify(r => r.AddUserAsync(It.IsAny<UserEntity>()), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.That(result.IsSuccess, Is.True);
        }


        [Test]
        public async Task RegisterUser_WhenAlreadyExistsAnAccount_WithSameEmail_ReturnsFailure()
        {
            var request = new RegisterRequest
            {
                UserName = _username,
                Email = _email,
                Password = _password
            };
            var registeredUser = new UserEntity("AnohterUserName", _email);

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(_email)).ReturnsAsync(registeredUser);

            var result = await _authService.RegisterAsync(request);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("User already exists with this email"));
        }

        #endregion

        #region RefreshToken

        [Test]
        public async Task RefreshTokenAsync_WithValidRefreshToken_ReturnsNewTokens()
        {
            var oldRefreshToken = "old-refresh-token";
            var newAccessToken = "new-access-token";

            var request = new RefreshTokenRequestDTO { RefreshToken = oldRefreshToken };
            var user = new UserEntity(_username, _email);
            user.UpdateRefreshToken(
                oldRefreshToken,
                DateTime.UtcNow.AddDays(1)
            );

            _userRepositoryMock.Setup(r => r.GetByRefreshTokenAsync(request.RefreshToken)).ReturnsAsync(user);
            _tokenGeneratorMock.Setup(r => r.GenerateToken(It.IsAny<UserEntity>())).Returns(newAccessToken);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _authService.RefreshTokenAsync(request);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.AccessToken, Is.EqualTo(newAccessToken));
            Assert.That(result.Value.RefreshToken, Is.Not.Null.And.Not.Empty);

            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task RefreshTokenAsync_WithInvalidRefreshToken_ReturnsFailure()
        {
            var request = new RefreshTokenRequestDTO
            {
                RefreshToken = "invalid-token"
            };

            _userRepositoryMock
                .Setup(r => r.GetByRefreshTokenAsync(request.RefreshToken))
                .ReturnsAsync((UserEntity?)null);

            var result = await _authService.RefreshTokenAsync(request);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Invalid refresh token"));

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Test]
        public async Task RefreshTokenAsync_WithExpiredRefreshToken_ReturnsFailure()
        {
            var token = "expired-token";
            var user = new UserEntity(_username, _email);

            user.UpdateRefreshToken(token, DateTime.UtcNow.AddMinutes(-1));

            _userRepositoryMock
                .Setup(r => r.GetByRefreshTokenAsync(token))
                .ReturnsAsync(user);

            var result = await _authService.RefreshTokenAsync(
                new RefreshTokenRequestDTO { RefreshToken = token });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Invalid refresh token"));
        }
        #endregion

        #region Logout

        [Test]
        public async Task LogoutAsync_WhenUserDoesNotExist_ReturnsFailure()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(r => r.GetById(userId))
                .ReturnsAsync((UserEntity?)null);

            var result = await _authService.LogoutAsync(userId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found"));

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Test]
        public async Task LogoutAsync_WithValidUser_InvalidatesRefreshToken()
        {
            var user = new UserEntity(_username, _email);

            user.UpdateRefreshToken(
                "valid-refresh-token",
                DateTime.UtcNow.AddDays(5)
            );

            _userRepositoryMock
                .Setup(r => r.GetById(user.Id))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _authService.LogoutAsync(user.Id);

            Assert.That(result.IsSuccess, Is.True);

            Assert.That(user.RefreshToken, Is.Null);
            Assert.That(user.RefreshTokenExpiryTime, Is.LessThanOrEqualTo(DateTime.UtcNow));

            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        #endregion
    }
}
