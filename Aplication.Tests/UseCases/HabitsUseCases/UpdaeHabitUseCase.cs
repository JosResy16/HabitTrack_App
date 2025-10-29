

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain.Entities;
using Moq;

namespace Aplication.Tests.UseCases.HabitsUseCases
{
    internal class UpdaeHabitUseCase
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
        public async Task UpdateHabit_WithValidData_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity{ Id = habitId, Title = "Habit test", UserId = userId};
            var habitDto = new HabitDTO { Title = "Habit updated successfuly"};

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<HabitEntity>())).ReturnsAsync(true);

            var response = await _habitsService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value?.Title, Is.EqualTo("Habit updated successfuly"));

            _habitRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<HabitEntity>()), Times.Once);
        }

        [Test]
        public async Task UpdateHabit_WithInvaidData_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = userId };
            var habitDto = new HabitDTO { Title = "Habit updated successfuly" };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(habit)).ReturnsAsync(false);

            var response = await _habitsService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Value?.Title, Is.EqualTo("Habit updated successfuly"));
        }
    }
}
