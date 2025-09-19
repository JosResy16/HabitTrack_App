using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Configuration;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Auth;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Aplication.Tests.UseCases.User
{
    class RegisterUserTest
    {
        private Mock<IUserRepository> _userRepositoryMock = null!;
        private Mock<ITokenGenerator> _tokenGeneratorMock = null!;
        private AuthService _authService = null!;

        [SetUp]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenGeneratorMock = new Mock<ITokenGenerator>();
            var options = Options.Create(new AppSettings());

            _authService = new AuthService(options, _userRepositoryMock.Object, _tokenGeneratorMock.Object);
            
        }
        
        [Test]
        public async Task LoginAsync_WithValidCredential_ReturnsToken()
        {
            var fakeToken = "fake-jwt";

            var user = new HabitTracker.Domain.Entities.User
            {
                UserName = "admin",
                PasswordHashed = new PasswordHasher<HabitTracker.Domain.Entities.User>().HashPassword(null!, "1234")
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);
            _tokenGeneratorMock.Setup(r => r.GenerateToken(It.IsAny<HabitTracker.Domain.Entities.User>())).Returns(fakeToken);

            var request = new UserDTO
            {
                UserName = "admin",
                Password = "1234"
            };

            //Act
            var result = await _authService.LoginAsync(request);

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(fakeToken, Is.EqualTo(result.AccessToken));
        }

        [Test]
        public void LoginAsync_WithInvalidPassword_ThrowsUnauthorized()
        {
            var user = new HabitTracker.Domain.Entities.User
            {
                UserName = "jose",
                PasswordHashed = new PasswordHasher<HabitTracker.Domain.Entities.User>().HashPassword(null!, "correct-password")
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("jose")).ReturnsAsync(user);

            var request = new UserDTO { UserName = "jose", Password = "wrong-password" };

            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }
    }
}
