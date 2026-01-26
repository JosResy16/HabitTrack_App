using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;

namespace Application.Tests.UseCases.HabitsUseCases.Commands
{
    internal class CreateHabitServiceTests
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
        public async Task AddNewHabitAsync_WithValidData_ShouldReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var title = "title";
            var description = "description";
            var habitDto = new CreateHabitDTO { Title = title, Description = description, RepeatCount = 1, RepeatInterval = 1};

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
            _habitRepositoryMock.Setup(r => r.GetByTitleAsync(userId, title)).ReturnsAsync((HabitEntity?)null);
            _habitRepositoryMock.Setup(r => r.AddAsync(It.IsAny<HabitEntity>())).Returns(Task.CompletedTask);
            _habitRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _habitLogServiceMock.Setup(l => l.AddLogAsync(It.IsAny<Guid>(), ActionType.Created)).ReturnsAsync(Result.Success());

            var result = await _habitService.AddNewHabitAsync(habitDto);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Title, Is.EqualTo(title));
            Assert.That(result.Value.Description, Is.EqualTo(description));

            _habitRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HabitEntity>()), Times.Once);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), ActionType.Created), Times.Once);
        }

        [Test]
        public async Task AddNewHabitAsync_WhenHabitWithSameTitleExists_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var title = "Same Title";
            var habitDto = new CreateHabitDTO { Title = title};
            var existingHabit = new HabitEntity(userId, title, null, null, null);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

            _habitRepositoryMock.Setup(r => r.GetByTitleAsync(userId, habitDto.Title)).ReturnsAsync(existingHabit);

            var result = await _habitService.AddNewHabitAsync(habitDto);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Already exists an habit with the same Title"));

            _habitRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HabitEntity>()), Times.Never);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
            _habitRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
