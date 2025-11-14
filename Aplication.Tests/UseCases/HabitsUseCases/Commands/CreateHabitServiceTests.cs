using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;
using NUnit.Framework.Constraints;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
        public async Task CreateHabitAsync_WithCorrectData_ShouldCreateHabit()
        {
            var userId = Guid.NewGuid();
            var title = "Wake up early";
            var description = "Wake up at 6 am every day";
            var habit = new HabitEntity { Title = title, Description = description , UserId = userId};
            var habitDto = new CreateHabitDTO { Title = title, Description = description };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(x => x.AddAsync(It.Is<HabitEntity>
                (h => h.Title == title && h.Description == description && h.UserId == userId)))
                .ReturnsAsync(true);

            var result = await _habitService.AddNewHabitAsync(habitDto);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value?.Title, Is.EqualTo(title));
            Assert.That(result.Value?.Description, Is.EqualTo(description));

            _habitRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HabitEntity>()), Times.Once);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(result.Value.Id, ActionType.Created), Times.Once);
        }

        [Test]
        public async Task CreateHabitAsync_WhenTitleIsEmpty_ShouldReturnFailure()
        { 
            var userId = Guid.NewGuid();
            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            string title = "";
            string description = "";
            var habitDto = new CreateHabitDTO {Title = title, Description = description };

            var result = await _habitService.AddNewHabitAsync(habitDto);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Title can not be empty"));

            _habitRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HabitEntity>()), Times.Never);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }

        [Test]
        public async Task CreateHabitAsync_WhenTitleExceedsMaxLength_ShouldReturnFailure()
        {
            var userId = Guid.NewGuid();
            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            string title = new string('x', 101);
            string description = "";
            var habitDto = new CreateHabitDTO { Title = title, Description = description };

            var result = await _habitService.AddNewHabitAsync(habitDto);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Title can not exceed 100 characters."));

            _habitRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HabitEntity>()), Times.Never);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }

        [Test]
        public async Task CreateHabitAsync_WhenHabitWithSameTitleExists_ShouldReturnFailure()
        {
            var userId = Guid.NewGuid();
            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var title = "Same Title";
            var habitDto = new CreateHabitDTO { Title = title, Description = "" };
            var existingHabit = new HabitEntity { Id = new Guid(), Title = title, Description = "" };

            _habitRepositoryMock.Setup(r => r.GetByTitleAsync(userId, habitDto.Title)).ReturnsAsync(existingHabit);

            var result = await _habitService.AddNewHabitAsync(habitDto);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Already exists an habit with the same Title"));

            _habitRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HabitEntity>()), Times.Never);
            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }

        [Test]
        public async Task CreateHabitAsync_WhenRepositoryFails_ShouldReturnFailure()
        {
            var userId = Guid.NewGuid();
            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var title = "Test title";
            var habitDto = new CreateHabitDTO { Title = title, Description = "" };

            _habitRepositoryMock.Setup(r => r.GetByTitleAsync(userId, habitDto.Title)).ReturnsAsync((HabitEntity?)null);
            _habitRepositoryMock.Setup(r => r.AddAsync(It.IsAny<HabitEntity>())).ReturnsAsync(false);

            var result = await _habitService.AddNewHabitAsync(habitDto);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Could not create habit"));

            _habitLogServiceMock.Verify(l => l.AddLogAsync(It.IsAny<Guid>(), It.IsAny<ActionType>()), Times.Never);
        }
    }
}
