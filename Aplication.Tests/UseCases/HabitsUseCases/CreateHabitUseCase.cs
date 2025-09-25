using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain.Entities;
using Moq;
using NUnit.Framework.Constraints;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Aplication.Tests.UseCases.Habits
{
    internal class CreateHabitUseCase
    {
        private Mock<IHabitRepository> _habitRepositoryMock = null;
        private Mock<IUserContextService> _userContextServiceMock = null;
        private HabitServices _habitService = null;

        [SetUp]
        public void SetUp()
        {
            _habitRepositoryMock = new Mock<IHabitRepository>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _habitService = new HabitServices(_habitRepositoryMock.Object, _userContextServiceMock.Object);
        }

        [Test]
        public async Task CreateHabitAsync_ShouldCreateHabitWithCorrectUserData()
        {
            var userId = Guid.NewGuid();
            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            var title = "Wake up early";
            var description = "Wake up at 6 am every day";
            var habitDto = new HabitDTO { Title = title, Description = description };

            HabitEntity? habitSaved = null;

            _habitRepositoryMock.Setup(x => x.AddAsync(It.IsAny<HabitEntity>()))
                .Callback<HabitEntity>(h => habitSaved = h)
                .Returns(Task.CompletedTask);

            var result = await _habitService.AddNewHabitAsync(habitDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(userId, result.UserId);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(description, result.Description);
            Assert.IsNotNull(habitSaved);

            _habitRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HabitEntity>()), Times.Once);
        }

        [Test]
        public async Task CreateHabitASync_WithInvalidData_ShouldThrowValidationException()
        {
            var userId = Guid.NewGuid();
            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);

            string title = "";
            string description = "";
            var habitDto = new HabitDTO {Title = title, Description = description };

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _habitService.AddNewHabitAsync(habitDto)
            );

            Assert.That(ex.Message, Is.EqualTo("Title is required."));
        }
    }
}
