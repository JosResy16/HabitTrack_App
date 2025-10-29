

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain.Entities;
using Moq;

namespace Aplication.Tests.UseCases.HabitsUseCases
{
    internal class UndoHabitCompletionUseCase
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
        public async Task UndoHabitCompetion_WithValidData_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = userId };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<HabitEntity>())).ReturnsAsync(true);

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.True);
            _habitRepositoryMock.Verify(x => x.UpdateAsync(It.Is<HabitEntity>(h => h.IsCompleted)), Times.Once);
        }

        [Test]
        public async Task UndoHabitCompetion_WithInvalidData_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync((HabitEntity?)null);

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));
        }

        [Test]
        public async Task UndoHabitCompetion_WhenUserIsNotOwner_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = otherUserId };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Not authorized"));
        }

        [Test]
        public async Task UndoHabitCompetion_WhenUpdateFails_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, UserId = userId };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<HabitEntity>())).ReturnsAsync(false);

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Could not mark as done"));
        }
    }
}
