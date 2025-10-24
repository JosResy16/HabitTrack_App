

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
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
        private HabitQueryService _habitQueryService = null;

        [SetUp]
        public void SetUp()
        {
            _habitRepositoryMock = new Mock<IHabitRepository>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _habitQueryService = new HabitQueryService(_habitRepositoryMock.Object, _userContextServiceMock.Object);
        }

        [Test]
        public async Task GetHabitByIdAsync_WithValidHabitId_ReturnsHabit()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity {Id = habitId, Title = "Read", UserId = userId };

            _habitRepositoryMock.Setup( r => r.GetByIdAsync(habitId))
                .ReturnsAsync((HabitEntity h) => Result<HabitEntity>.Success(h));

            _userContextServiceMock.Setup(u => u.GetCurrentUserId())
                .Returns(userId);

            var result = await _habitQueryService.GetHabitByIdAsync(habitId);

            Assert.IsNotNull(result);
            Assert.That(result.Value.Id, Is.EqualTo(habitId));
            Assert.That(result.Value.UserId, Is.EqualTo(userId));
            Assert.That(result.Value.Title, Is.EqualTo("Read"));
        }

        [Test]
        public async Task GetHabitByIdAsync_WhenHabitDoesNotExist_ReturnsFailureResult()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId))
                .ReturnsAsync(Result<HabitEntity>.Failure("Habit not found"));

            _userContextServiceMock.Setup(u => u.GetCurrentUserId())
                .Returns(userId);

            var result = await _habitQueryService.GetHabitByIdAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("habit not found"));
        }
    }
}
