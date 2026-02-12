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
            var habit = new HabitEntity(userId, "title", null, null, null);
            var habitDto = new UpdateHabitDTO { Title = "new title"};

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(r => r.GetByTitleAsync(userId, It.IsAny<string>()))
                .ReturnsAsync((HabitEntity?)null);

            _habitLogRepositoryMock.Setup(r => r.AddLogAsync(habitId, ActionType.Updated))
                .ReturnsAsync(Result.Success());
            _habitRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value?.Title, Is.EqualTo("new title"));

            _habitRepositoryMock.Verify(r => r.GetByTitleAsync(userId, It.IsAny<string>()), Times.Once);
            _habitLogRepositoryMock.Verify(r => r.AddLogAsync(habitId, ActionType.Updated), Times.Once);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateHabit_WhenHabitDoesNotExist_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habitDto = new UpdateHabitDTO { Title = "new title" };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
            _habitRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((HabitEntity?)null);

            var result = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));

            _habitRepositoryMock.Verify(r => r.GetByTitleAsync(userId, It.IsAny<string>()), Times.Never);
            _habitLogRepositoryMock.Verify(r => r.AddLogAsync(habitId, ActionType.Updated), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateHabit_WhenUserIsNotTheOwner_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var anotherUser = Guid.NewGuid();
            var habit = new HabitEntity(anotherUser, "title", null, null, null);
            var habitDto = new UpdateHabitDTO { Title = "new title" };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);

            var response = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(response.ErrorMessage, Is.EqualTo("Not authorized"));

            _habitRepositoryMock.Verify(r => r.GetByTitleAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateHabit_WhenExistAnotherHabitWithSameTitle_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            var habit = new HabitEntity(userId, "old title", null, null, null);
            var habitDto = new UpdateHabitDTO { Title = "title" };
            var existingHabit = new HabitEntity(Guid.NewGuid(), "title", null, null, null);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitRepositoryMock.Setup(r => r.GetByTitleAsync(userId, habitDto.Title)).ReturnsAsync(existingHabit);
            
            var response = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.ErrorMessage, Is.EqualTo("Already exists an habit with the same Title"));

            _habitLogRepositoryMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateHabit_WhenTitleDoesNotChange_DoesNotCheckForDuplicateTitle()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity(userId, "same title", null, null, null);
            var habitDto = new UpdateHabitDTO { Title = "same title" };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var result = await _habitService.UpdateHabitAsync(habitId, habitDto);

            Assert.That(result.IsSuccess, Is.True);

            _habitRepositoryMock.Verify(
                r => r.GetByTitleAsync(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
