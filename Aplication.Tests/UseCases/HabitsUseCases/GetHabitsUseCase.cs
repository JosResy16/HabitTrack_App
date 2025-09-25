

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain.Entities;
using Moq;
using System.Threading.Tasks;

namespace Aplication.Tests.UseCases.HabitsUseCases
{
    internal class GetHabitsUseCase
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
        public async Task GetHabitByIdAsync_WithValidHabitId_ReturnsHabit()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity {Id = habitId, Title = "Read", UserId = userId };

            _habitRepositoryMock.Setup( r => r.GetByIdAsync(habitId))
                .ReturnsAsync(habit);

            _userContextServiceMock.Setup(u => u.GetCurrentUserId())
                .Returns(userId);

            var result = await _habitService.GetHabitByIdAsync(habitId);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(habitId));
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Title, Is.EqualTo("Read"));
        }

        [Test]
        public void GetHabitByIdAsync_WhenHabitDoesNotExist_ThrowsException()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId))
                .ReturnsAsync((HabitEntity?)null);

            _userContextServiceMock.Setup(u => u.GetCurrentUserId())
                .Returns(userId);

            var ex = Assert.ThrowsAsync<Exception>( async () => 
                await _habitService.GetHabitByIdAsync(habitId));

            Assert.That(ex.Message, Is.EqualTo("habit not found"));
        }
    }
}
