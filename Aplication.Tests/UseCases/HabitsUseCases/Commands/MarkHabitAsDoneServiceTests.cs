using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;

namespace Application.Tests.UseCases.HabitsUseCases.Commands
{
    internal class MarkHabitAsDoneServiceTests
    {
        private Mock<IHabitRepository> _habitRepositoryMock;
        private Mock<IUserContextService> _userContextServiceMock;
        private Mock<IHabitLogService> _habitLogServiceMock;
        private HabitServices _habitService;

        [SetUp]
        public void SetUp()
        {
            _habitRepositoryMock = new Mock<IHabitRepository>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _habitLogServiceMock = new Mock<IHabitLogService>();
            _habitService = new HabitServices(_habitRepositoryMock.Object, _userContextServiceMock.Object, _habitLogServiceMock.Object);
        }

        [Test]
        public async Task MarkHabitAsDone_WithValidData_ReturnsSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity(userId, "title", null, null, null);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);

            _habitLogServiceMock.Setup(l => l.AddLogAsync(habitId, ActionType.Completed))
                .ReturnsAsync(Result.Success());
            _habitRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _habitService.MarkHabitAsDone(habitId);

            Assert.That(result.IsSuccess, Is.True);

            _habitLogServiceMock.Verify(l => l.AddLogAsync(habitId, ActionType.Completed), Times.Once);
            _habitRepositoryMock.Verify(l => l.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task MarkHabitAsDone_WhenHabitDoesNotExist_ReturnsFailure()
        {
            var habitId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync((HabitEntity?)null);

            var result = await _habitService.MarkHabitAsDone(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));

            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task MarkHabitAsDone_WhenHabitDoesNotBelongToUser_ReturnsFailure()
        {
            var habitId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var habit = new HabitEntity(ownerId, "title", null, null, null);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(otherUserId));
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var result = await _habitService.MarkHabitAsDone(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Not authorized"));

            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
