using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;

namespace Application.Tests.UseCases.HabitsUseCases.Commands
{
    internal class UpdateHabitServiceTests
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
        public async Task UpdateHabit_WithValidData_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity{ Id = habitId, Title = "Habit test", UserId = userId};
            var habitDto = new CreateHabitDTO { Title = "Habit updated successfuly"};

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<HabitEntity>())).ReturnsAsync(true);

            var response = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value?.Title, Is.EqualTo("Habit updated successfuly"));

            _habitRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<HabitEntity>()), Times.Once);
        }

        [Test]
        public async Task UpdateHabit_WhenHabitDoesNotExist_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = userId };
            var habitDto = new CreateHabitDTO { Title = "Habit updated successfuly" };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(habit)).ReturnsAsync(false);

            var response = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(response.ErrorMessage, Is.EqualTo("Habit not found"));
        }

        [Test]
        public async Task UpdateHabit_WhenUserIsNotTheOwner_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var anotherUser = new Guid();
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = anotherUser };
            var habitDto = new CreateHabitDTO { Title = "Habit test" };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(x => x.UpdateAsync(habit)).ReturnsAsync(true);

            var response = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(response.ErrorMessage, Is.EqualTo("Not authorize"));
            _habitRepositoryMock.Verify(r => r.GetByTitleAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }

        [Test]
        public async Task UpdateHabit_WhenExistAnotherHabitWithSameTitle_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var existingHabitId = Guid.NewGuid();

            var habit = new HabitEntity { Id = habitId, Title = "Old title", UserId = userId };
            var habitDto = new CreateHabitDTO { Title = "New title" };
            var existingHabit = new HabitEntity { Id = existingHabitId, Title = "New Title", UserId = userId };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(r => r.GetByTitleAsync(userId, habitDto.Title)).ReturnsAsync(existingHabit);
            
            var response = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.ErrorMessage, Is.EqualTo("Already exists an habit with the same Title"));
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<HabitEntity>()), Times.Never);
        }

        [Test]
        public async Task UpdateHabit_WhenUpdatingHabitFails_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "Habit test", UserId = userId };
            var habitDto = new CreateHabitDTO { Title = "Habit test" };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(r => r.GetByTitleAsync(userId, habitDto.Title)).ReturnsAsync((HabitEntity?)null);

            var response = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(response.ErrorMessage, Is.EqualTo("Could not update this habit"));
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }
    }
}
