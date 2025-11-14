using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain.Entities;
using Moq;
using System.Threading.Tasks;

namespace Application.Tests.UseCases.HabitsUseCases.Queries
{
    internal class GetHabitsQueryTests
    {
        private Mock<IHabitRepository> _habitRepositoryMock;
        private Mock<IUserContextService> _userContextServiceMock;
        private Mock<IHabitLogService> _habitLogServiceMock;
        private HabitQueryService _habitQueryService;

        [SetUp]
        public void SetUp()
        {
            _habitRepositoryMock = new Mock<IHabitRepository>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _habitLogServiceMock = new Mock<IHabitLogService>();
            _habitQueryService = new HabitQueryService(_habitRepositoryMock.Object, _userContextServiceMock.Object, _habitLogServiceMock.Object);
        }

        #region GetHabitByIdAsync

        [Test]
        public async Task GetHabitByIdAsync_WithValidHabitId_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity {Id = habitId, Title = "Test", UserId = userId };

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var result = await _habitQueryService.GetHabitByIdAsync(habitId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value?.Id, Is.EqualTo(habitId));
        }

        [Test]
        public async Task GetHabitByIdAsync_WhenHabitDoesNotExist_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync((HabitEntity?) null);

            var result = await _habitQueryService.GetHabitByIdAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));
        }

        [Test]
        public async Task GetHAbitById_WhenUserIsNotTheOwner_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "Test title" , UserId = otherUserId};

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var result = await _habitQueryService.GetHabitByIdAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Not authorize"));
        }
        #endregion
    }
}
