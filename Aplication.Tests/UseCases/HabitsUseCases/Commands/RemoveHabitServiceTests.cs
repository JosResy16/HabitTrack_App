using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;
using System.Net;
using System.Threading.Tasks;

namespace Application.Tests.UseCases.HabitsUseCases.Commands
{
    internal class RemoveHabitServiceTests
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
        public async Task RemoveHabit_WithValidData_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "read", UserId = userId};

            _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(r => r.DeleteAsync(habitId))
                .ReturnsAsync(true);

            var result = await _habitService.RemoveHabitAsync(habitId);

            Assert.That(result.IsSuccess, Is.True);
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(habitId, ActionType.Removed), Times.Once);
        }

        [Test]
        public async Task RemoveHabit_WhenHabitDoesNotExist_ReturnFailed()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "read", UserId = userId };

            _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId))
                .ReturnsAsync((HabitEntity?)null);

            var result = await _habitService.RemoveHabitAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));

            _habitRepositoryMock.Verify(r => r.DeleteAsync(habitId), Times.Never);
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }

        [Test]
        public async Task RemoveHabit_WhenUserIsNotOwner_ReturnFailure()
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

            _habitRepositoryMock.Verify(r => r.DeleteAsync(habitId), Times.Never);
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }

        [Test]
        public async Task RemoveHabit_WhenRemovedFails_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "read", UserId = userId };

            _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId))
                .ReturnsAsync(habit);
            _habitRepositoryMock.Setup(r => r.DeleteAsync(habitId)).ReturnsAsync(false);

            var result = await _habitService.RemoveHabitAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Couldn't delete this habit"));

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }

    }
}
