using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;
using NuGet.Frameworks;

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
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = userId };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<HabitEntity>())).ReturnsAsync(true);
            _habitLogServiceMock.Setup(l => l.GetLogForHabitAndDayAsync(habitId, It.IsAny<DateTime>())).ReturnsAsync(Result<HabitLog?>.Success(null));
            _habitLogServiceMock.Setup(l => l.AddLogAsync(habitId, ActionType.Completed)).ReturnsAsync(Result.Success());

            var result = await _habitService.MarkHabitAsDone(habitId);

            Assert.That(result.IsSuccess, Is.True);
            _habitRepositoryMock.Verify(x => x.UpdateAsync(It.Is<HabitEntity>(h => h.IsCompleted)), Times.Once);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(habitId, ActionType.Completed), Times.Once);
        }

        [Test]
        public async Task MarkHabitAsDone_WhenHabitDoesNotExist_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync((HabitEntity?)null);

            var result = await _habitService.MarkHabitAsDone(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));

            _habitLogServiceMock.Verify(l => l.GetLogForHabitAndDayAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<HabitEntity>()), Times.Never);
        }

        [Test]
        public async Task MarkHabitAsDone_WhenUserIsNotOwner_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = otherUserId };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var result = await _habitService.MarkHabitAsDone(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Not authorized"));

            _habitLogServiceMock.Verify(l => l.GetLogForHabitAndDayAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<HabitEntity>()), Times.Never);
        }

        [Test]
        public async Task MarkHabitAsDone_WhenAlreadyWasMarkedToday_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var today = DateTime.UtcNow.Date;
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = userId };
            var existingLog = new HabitLog(habitId, today, ActionType.Completed);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitLogServiceMock.Setup(l => l.GetLogForHabitAndDayAsync(habitId, It.Is<DateTime>(d => d.Date == today)))
                .ReturnsAsync(Result<HabitLog?>.Success(existingLog));

            var result = await _habitService.MarkHabitAsDone(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Already marked this habit as done today."));

            _habitRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<HabitEntity>()), Times.Never);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }

        [Test]
        public async Task MarkHabitAsDone_WhenUpdateFails_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var today = DateTime.UtcNow.Date;
            var habit = new HabitEntity { Id = habitId, UserId = userId };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitLogServiceMock.Setup(l => l.GetLogForHabitAndDayAsync(habitId, It.Is<DateTime>(d => d.Date == today)))
                .ReturnsAsync(Result<HabitLog?>.Success(null));
            _habitRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<HabitEntity>())).ReturnsAsync(false);

            var result = await _habitService.MarkHabitAsDone(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Could not mark as done"));

            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }
    }
}
