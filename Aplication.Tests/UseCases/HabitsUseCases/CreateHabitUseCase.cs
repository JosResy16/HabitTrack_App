using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain.Entities;
using Moq;
using NUnit.Framework.Constraints;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Aplication.Tests.UseCases.Habits
{
    internal class CreateHabitUseCase
    {
        private Mock<IHabitRepository> _habitRepositoryMock;
        private Mock<IUserContextService> _userContextServiceMock;
        private Mock<IHabitLogRepository> _habitLogRepository;
        private HabitServices _habitService;

        [SetUp]
        public void SetUp()
        {
            _habitRepositoryMock = new Mock<IHabitRepository>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _habitLogRepository = new Mock<IHabitLogRepository>();
            _habitService = new HabitServices(_habitRepositoryMock.Object, _userContextServiceMock.Object, _habitLogRepository.Object);
        }

        [Test]
        public async Task CreateHabitAsync_ShouldCreateHabitWithCorrectUserData()
        {
            var userId = Guid.NewGuid();
            var title = "Wake up early";
            var description = "Wake up at 6 am every day";
            var habit = new HabitEntity { Title = title, Description = description , UserId = userId};
            var habitDto = new HabitDTO { Title = title, Description = description };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.AddAsync(habit))
                .ReturnsAsync(true);

            var result = await _habitService.AddNewHabitAsync(habitDto);

            Assert.IsTrue(result.IsSuccess, $"Expected operation to succeed but got error: {result.ErrorMessage}");
            Assert.IsNotNull(result.Value, "Expected a HabitEntity value when operation succeeds");

            Assert.AreEqual(userId, result.Value.UserId);
            Assert.AreEqual(title, result.Value.Title);
            Assert.AreEqual(description, result.Value.Description);

            _habitRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HabitEntity>()), Times.Once);
        }

        [Test]
        public async Task CreateHabitASync_WithInvalidData_ShouldThrowValidationException()
        {
            var userId = Guid.NewGuid();
            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            string title = "";
            string description = "";
            var habitDto = new HabitDTO {Title = title, Description = description };

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _habitService.AddNewHabitAsync(habitDto)
            );

            Assert.That(ex.Message, Is.EqualTo("Title is required."));
        }
    }
}
