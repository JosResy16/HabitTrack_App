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
        public async Task UndoHabitCompletion_WithValidData_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity(userId, "title", null, null, null);
            habit.MarkHabitAsDone();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);
            _habitLogRepositoryMock.Setup(l => l.AddLogAsync(habitId, ActionType.Undone)).ReturnsAsync(Result.Success());
            _habitRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(habit.IsCompleted, Is.False);

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(habitId, ActionType.Undone), Times.Once);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UndoHabitCompletion_WhenHabitDoesNotExist_ReturnFailure()
        {
            var userId = Guid.NewGuid();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((HabitEntity?)null);

            var result = await _habitService.UndoHabitCompletion(Guid.NewGuid());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task UndoHabitCompletion_WhenUserIsNotOwner_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity(otherUserId, "title", null, null, null);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Not authorized"));

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task UndoHabitCompletion_WhenHabitIsNotMarkedAsCompleted_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity(userId, "title", null, null, null);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);

            var result = await _habitService.UndoHabitCompletion(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit is not marked as completed"));

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
