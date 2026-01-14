using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
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
            var habit = new HabitEntity(userId, "title", null, null, null);

            _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);
            _habitLogRepositoryMock.Setup(r => r.AddLogAsync(habit.Id, ActionType.Removed))
                .ReturnsAsync(Result.Success());
            _habitRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _habitService.RemoveHabitAsync(habit.Id);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(habit.IsDeleted, Is.True);

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(habit.Id, ActionType.Removed), Times.Once);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task RemoveHabit_WhenHabitDoesNotExist_ReturnFailed()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((HabitEntity?)null);

            var result = await _habitService.RemoveHabitAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task RemoveHabit_WhenUserIsNotOwner_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity(otherUserId, "title", null, null, null);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);

            var result = await _habitService.RemoveHabitAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Not authorized"));

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task RemoveHabit_WhenHabitIsAlreadyDeleted_ReturnsSuccessWithoutSideEffects()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity(userId, "title", null, null, null);

            habit.SoftDelete();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var result = await _habitService.RemoveHabitAsync(habitId);

            Assert.That(result.IsSuccess, Is.True);

            _habitLogRepositoryMock.Verify(
                l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()),
                Times.Never);

            _habitRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }
    }
}
