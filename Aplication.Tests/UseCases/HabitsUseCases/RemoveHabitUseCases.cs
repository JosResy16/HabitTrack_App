

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain.Entities;
using Moq;
using System.Threading.Tasks;

namespace Aplication.Tests.UseCases.HabitsUseCases
{
    internal class RemoveHabitUseCases
    {
        private Mock<IHabitRepository> _habitRepositoryMock;
        private Mock<IUserContextService> _userContextServiceMock;
        private Mock<IHabitLogRepository> _habitLogRepositoryMock;
        private HabitServices _habitsService;

        [SetUp]
        public void SetUp()
        {
            _habitRepositoryMock = new Mock<IHabitRepository>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _habitLogRepositoryMock = new Mock<IHabitLogRepository>();
            _habitsService = new HabitServices(_habitRepositoryMock.Object, _userContextServiceMock.Object, _habitLogRepositoryMock.Object);
        }

        [Test]
        public async Task RemoveHabitAsync_WithValidData_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "read", UserId = userId};

            _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.DeleteAsync(habitId))
                .ReturnsAsync(true);

            var result = await _habitsService.RemoveHabitAsync(habit);

            Assert.IsNotNull(result);
            Assert.That(result.IsSuccess);
        }

        [Test]
        public async Task RemoveHabitAsync_WithInvalidData_ReturnFailed()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "read", UserId = userId };

            _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.DeleteAsync(habitId))
                .ReturnsAsync(false);

            var result = await _habitsService.RemoveHabitAsync(habit);

            Assert.IsNotNull(result);
            Assert.That(result.IsSuccess, Is.EqualTo(false));
            Assert.That(result.ErrorMessage, Is.EqualTo("Couldn´t delet this habit"));
        }

    }
}
