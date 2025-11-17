using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;
using System.Threading.Tasks;

namespace Application.Tests.UseCases.HabitsUseCases.Queries
{
    internal class GetHabitsQueryTests
    {
        private Mock<IHabitRepository> _habitRepositoryMock;
        private Mock<IUserContextService> _userContextServiceMock;
        private Mock<IHabitLogService> _habitLogServiceMock;
        private HabitQueryService _habitQueryService;

        [SetUp]
        public void SetUp()
        {
            _habitRepositoryMock = new Mock<IHabitRepository>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _habitLogServiceMock = new Mock<IHabitLogService>();
            _habitQueryService = new HabitQueryService(_habitRepositoryMock.Object, _userContextServiceMock.Object, _habitLogServiceMock.Object);
        }

        #region GetHabitByIdAsync

        [Test]
        public async Task GetHabitByIdAsync_WithValidHabitId_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity {Id = habitId, Title = "Test", UserId = userId };

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var result = await _habitQueryService.GetHabitByIdAsync(habitId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value?.Id, Is.EqualTo(habitId));
        }

        [Test]
        public async Task GetHabitByIdAsync_WhenHabitDoesNotExist_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync((HabitEntity?) null);

            var result = await _habitQueryService.GetHabitByIdAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));
        }

        [Test]
        public async Task GetHAbitById_WhenUserIsNotTheOwner_ReturnFailure()
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "Test title" , UserId = otherUserId};

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var result = await _habitQueryService.GetHabitByIdAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Not authorize"));
        }
        #endregion

        #region GetUserHabitsAsync
        [Test]
        public async Task GetUserHabits_WhenHabitsExist_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            Priority? priority = null;
            var habits = new List<HabitEntity> 
            { 
                new HabitEntity { Id = Guid.NewGuid(), Title = "A" },
                new HabitEntity {Id= Guid.NewGuid(), Title = "B"}
            };

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsAsync(userId, priority)).ReturnsAsync(habits);

            var response = await _habitQueryService.GetUserHabitsAsync(priority);

            Assert.That(response.IsSuccess, Is.True, "service failed. Expected succes but was response.Failure");
            Assert.That(response.Value.Count(), Is.EqualTo(habits.Count));
            Assert.That(response.Value, Is.EquivalentTo(habits));

            _habitRepositoryMock.Verify(r => r.GetHabitsAsync(userId, priority), Times.Once);
        }

        [Test]
        public async Task GetEmptyList_WhenHabitsNotExist_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            Priority? priority = null;

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsAsync(userId, priority)).ReturnsAsync(new List<HabitEntity>());

            var response = await _habitQueryService.GetUserHabitsAsync(priority);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value, Is.Empty);

            _habitRepositoryMock.Verify(r => r.GetHabitsAsync(userId, priority), Times.Once);
        }

        [Test]
        public async Task GetHabitsFiltered_WhenPassPriorityAsParameter_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            Priority? priority = Priority.none;
            var habits = new List<HabitEntity>
            {
                new HabitEntity { Id = Guid.NewGuid(), Title = "A", Priority = priority },
                new HabitEntity {Id = Guid.NewGuid(), Title = "B", Priority = priority }
            };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsAsync(userId, priority)).ReturnsAsync(habits);

            var response = await _habitQueryService.GetUserHabitsAsync(priority);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value, Is.All.Matches<HabitEntity>(h => h.Priority == priority));
            Assert.That(response.Value.Count(), Is.EqualTo(habits.Count));

            _habitRepositoryMock.Verify(r => r.GetHabitsAsync(userId, priority), Times.Once);
        }
        #endregion

        #region GetHabitsHistory
        [Test]
        public async Task GetHabitHistory_WhitValidHabitId_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, Title = "A", UserId = userId };
            var logs = new List<HabitLog>
            {
                new HabitLog(habitId, DateTime.UtcNow.AddDays(-1), ActionType.Completed),
                new HabitLog(habitId, DateTime.UtcNow, ActionType.Undone)
            };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitLogServiceMock.Setup(r => r.GetLogsByHabitAsync(habitId))
                .ReturnsAsync(Result<IEnumerable<HabitLog>>.Success(logs));

            var response = await _habitQueryService.GetHabitHistoryAsync(habitId);

            Assert.That(response.Value.First().Date, Is.GreaterThan(response.Value.Last().Date));
            Assert.That(response.Value.All(h => h.HabitId == habitId), Is.True);
            Assert.That(response.Value.All(h => h.HabitTitle == habit.Title), Is.True);

            _habitLogServiceMock.Verify(r => r.GetLogsByHabitAsync(habitId), Times.Once);
        }

        [Test]
        public async Task GetHabitHistory_WhenHabitNotFound_ReturnsFailure()
        {
            var habitId = Guid.NewGuid();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync((HabitEntity?)null);

            var response = await _habitQueryService.GetHabitHistoryAsync(habitId);

            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.ErrorMessage, Is.EqualTo("Habit not found"));
        }

        [Test]
        public async Task GetHabitHistory_WhenUserIsNotOwner_ReturnFailure()
        {
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity { Id = habitId, UserId = Guid.NewGuid() };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);

            var response = await _habitQueryService.GetHabitHistoryAsync(habitId);

            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.ErrorMessage, Is.EqualTo("Not authorized"));
        }

        [Test]
        public async Task GetHabitHistory_WhenHasNoLogs_ReturnsEmptyList()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            var habit = new HabitEntity { Id = habitId, UserId = userId };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitLogServiceMock
                .Setup(r => r.GetLogsByHabitAsync(habitId))
                .ReturnsAsync(Result<IEnumerable<HabitLog>>.Success(new List<HabitLog>()));

            var response = await _habitQueryService.GetHabitHistoryAsync(habitId);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value.Count(), Is.EqualTo(0));
        }


        #endregion

        #region GetHabitsByCategory
        [Test]
        public async Task GetHabitsByCategory_WhitValidData_ReturnSuccessWihtHabits()
        {
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var habits = new List<HabitEntity>
            {
                new HabitEntity {Id = Guid.NewGuid(), Title = "A" , UserId = userId, CategoryId = categoryId},
                new HabitEntity {Id = Guid.NewGuid(),Title = "B" , UserId = userId, CategoryId = categoryId}
            };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsByCategoryIdAsync(categoryId, userId)).ReturnsAsync(habits);

            var response = await _habitQueryService.GetHabitsByCategoryAsync(categoryId);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value, Is.Not.Null);
            Assert.That(response.Value.Count(), Is.EqualTo(2));

            _habitRepositoryMock.Verify(r => r.GetHabitsByCategoryIdAsync(categoryId, userId), Times.Once);
        } 

        [Test]
        public async Task GetHabitsByCategory_WithNoHabits_ReturnsSuccessWithEmptyList()
        {
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsByCategoryIdAsync(categoryId, userId))
                                .ReturnsAsync(new List<HabitEntity>());

            var result = await _habitQueryService.GetHabitsByCategoryAsync(categoryId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(0));

            _habitRepositoryMock.Verify(r => r.GetHabitsByCategoryIdAsync(categoryId, userId),Times.Once);
        }

        #endregion

    }
}
