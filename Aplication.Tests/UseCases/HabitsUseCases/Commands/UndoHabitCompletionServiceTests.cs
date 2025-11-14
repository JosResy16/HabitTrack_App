using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;

namespace Application.Tests.UseCases.HabitsUseCases.Commands
{
    internal class UndoHabitCompletionServiceTests
    {
        private Mock<IHabitRepository> _habitRepositoryMock;
        private Mock<IUserContextService> _userContextServiceMock;
        private Mock<IHabitLogService> _habitLogRepositoryMock;
        private HabitServices _habitService;

        [SetUp]
        public void SetUp()
        {
            _habitRepositoryMock = new Mock<IHabitRepository>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _habitLogRepositoryMock = new Mock<IHabitLogService>();
            _habitService = new HabitServices(_habitRepositoryMock.Object, _userContextServiceMock.Object, _habitLogRepositoryMock.Object);
        }

        [Test]
        public async Task UndoHabitCompetion_WithValidData_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = userId };
            habit.MarkHabitAsDone();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(habit)).ReturnsAsync(true);
            _habitLogRepositoryMock.Setup(l => l.AddLogAsync(habitId, ActionType.Undone)).ReturnsAsync(Result.Success());

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.True);
            _habitRepositoryMock.Verify(x => x.UpdateAsync(It.Is<HabitEntity>(h => !h.IsCompleted)), Times.Once);
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Once);
        }

        [Test]
        public async Task UndoHabitCompetion_WhenHabitDoesNotExist_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync((HabitEntity?)null);

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));

            _habitRepositoryMock.Verify(x => x.UpdateAsync(It.Is<HabitEntity>(h => h.IsCompleted)), Times.Never);
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
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

            _habitRepositoryMock.Verify(x => x.UpdateAsync(It.Is<HabitEntity>(h => h.IsCompleted)), Times.Never);
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }

        [Test]
        public async Task UndoHabitCompetion_WhenUpdateFails_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, UserId = userId};
            habit.MarkHabitAsDone();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(habit)).ReturnsAsync(false);

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Could not mark as undo this habit"));

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }
    }
}
