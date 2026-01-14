using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;

namespace Application.Tests.UseCases.HabitsUseCases.Queries
{
    internal class GetHabitsQueryTests
    {
        private Mock<IHabitRepository> _habitRepositoryMock;
        private Mock<IUserContextService> _userContextServiceMock;
        private Mock<IHabitLogRepository> _habitLogRepositoryMock;
        private HabitQueryService _habitQueryService;

        [SetUp]
        public void SetUp()
        {
            _habitRepositoryMock = new Mock<IHabitRepository>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _habitLogRepositoryMock = new Mock<IHabitLogRepository>();
            _habitQueryService = new HabitQueryService(_habitRepositoryMock.Object, _userContextServiceMock.Object, _habitLogRepositoryMock.Object);
        }

        #region GetById

        [Test]
        public async Task GetHabitByIdAsync_WithValidHabitId_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habit = new HabitEntity(userId, "title", null, null, null);

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);

            var result = await _habitQueryService.GetHabitByIdAsync(habit.Id);

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task GetHabitByIdAsync_WhenHabitDoesNotExist_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((HabitEntity?) null);

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
            var habit = new HabitEntity(otherUserId, "title", null, null, null);

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);

            var result = await _habitQueryService.GetHabitByIdAsync(habitId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Not authorized"));
        }
        #endregion

        #region GetHabitsAsync
        [Test]
        public async Task GetUserHabits_WhenHabitsExist_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            Priority? priority = null;
            var habits = new List<HabitEntity>
            {
                new HabitEntity(userId, "title", null, null, null),
                new HabitEntity(userId, "Another title", null, null, null)
            };

            _userContextServiceMock.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsAsync(It.IsAny<Guid>(), It.IsAny<Priority?>()))
                .ReturnsAsync(habits);

            var response = await _habitQueryService.GetUserHabitsAsync(priority);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value!.Any(h => h.Title == "title"), Is.True);
            Assert.That(response.Value!.Any(h => h.Title == "Another title"), Is.True);
        }

        [Test]
        public async Task GetEmptyList_WhenHabitsNotExist_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            Priority? priority = null;

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsAsync(It.IsAny<Guid>(), priority))
                .ReturnsAsync(new List<HabitEntity>());

            var response = await _habitQueryService.GetUserHabitsAsync(priority);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value, Is.Empty);

            _habitRepositoryMock.Verify(r => r.GetHabitsAsync(userId, priority), Times.Once);
        }

        [Test]
        public async Task GetHabitsFiltered_WhenPassPriorityAsParameter_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            Priority? priority = Priority.High;
            var habits = new List<HabitEntity>
            {
                new HabitEntity(userId, "title", null, null, null) { Priority = priority },
                new HabitEntity(userId, "Another title", null, null, null) { Priority = priority }
            };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsAsync(It.IsAny<Guid>(), priority))
                .ReturnsAsync(habits);

            var response = await _habitQueryService.GetUserHabitsAsync(priority);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value, Is.All.Matches<HabitResponseDTO>(h => h.Priority == priority));
            Assert.That(response.Value.Count(), Is.EqualTo(habits.Count));

            _habitRepositoryMock.Verify(r => r.GetHabitsAsync(userId, priority), Times.Once);
        }
        #endregion

        #region GetHabitHistory
        [Test]
        public async Task GetHabitHistory_WhitValidHabitId_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var habit = new HabitEntity(userId, "title", null, null, null);
            var day = new DateOnly(2026, 1, 10);
            var logs = new List<HabitLog>
            {
                new HabitLog(habitId, day, ActionType.Completed, new DateTime(2026, 1, 10, 20, 0, 0, DateTimeKind.Utc)),
                new HabitLog(habitId, day.AddDays(-1), ActionType.Undone, new DateTime(2026, 1, 9, 20, 0, 0, DateTimeKind.Utc))
            };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(habit);
            _habitLogRepositoryMock.Setup(r => r.GetLogsByHabitIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(logs);

            var response = await _habitQueryService.GetHabitHistoryAsync(habitId);

            Assert.That(response.Value!.First().Date, Is.GreaterThan(response.Value!.Last().Date));
            Assert.That(response.Value!.All(h => h.HabitId == habitId), Is.True);
            Assert.That(response.Value!.All(h => h.HabitTitle == habit.Title), Is.True);

            _habitLogRepositoryMock.Verify(r => r.GetLogsByHabitIdAsync(userId, habitId), Times.Once);
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
            var habit = new HabitEntity(Guid.NewGuid(), "title", null, null, null);

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

            var habit = new HabitEntity(userId, "title", null, null, null);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetByIdAsync(habitId)).ReturnsAsync(habit);
            _habitLogRepositoryMock.Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
                .ReturnsAsync(new List<HabitLog>());

            var response = await _habitQueryService.GetHabitHistoryAsync(habitId);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value!.Count(), Is.EqualTo(0));
        }


        #endregion

        #region GetByCategory
        [Test]
        public async Task GetHabitsByCategory_WhitValidData_ReturnSuccessWihtHabits()
        {
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var habits = new List<HabitEntity>
            {
                new HabitEntity(Guid.NewGuid(), "title", null, null, null) { CategoryId = categoryId },
                new HabitEntity(Guid.NewGuid(), "another title", null, null, null) { CategoryId = categoryId }
            };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsByCategoryIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(habits);

            var response = await _habitQueryService.GetHabitsByCategoryAsync(categoryId);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value, Is.Not.Null);
            Assert.That(response.Value!.All(h => h.CategoryId == categoryId), Is.True);
            Assert.That(response.Value.Count(), Is.EqualTo(2));

            _habitRepositoryMock.Verify(r => r.GetHabitsByCategoryIdAsync(categoryId, userId), Times.Once);
        } 

        [Test]
        public async Task GetHabitsByCategory_WithNoHabits_ReturnsSuccessWithEmptyList()
        {
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsByCategoryIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                                .ReturnsAsync(new List<HabitEntity>());

            var result = await _habitQueryService.GetHabitsByCategoryAsync(categoryId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(0));

            _habitRepositoryMock.Verify(r => r.GetHabitsByCategoryIdAsync(categoryId, userId),Times.Once);
        }

        #endregion

        #region GetTodayHabits
        [Test]
        public async Task GetTodayHabits_WhenHabitsExist_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var day = new DateOnly(2026, 1, 10); ;
            var habits = new List<HabitEntity>
            {
                new HabitEntity(userId, "title", null, null, null)
            };

            var logs = new List<HabitLog>
            {
                new HabitLog (habits[0].Id, day, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc))
            };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsAsync(userId, null)).ReturnsAsync(habits);
            _habitLogRepositoryMock.Setup(r => r.GetLogsByDateAsync(userId, day))
                .ReturnsAsync(logs);

            var result = await _habitQueryService.GetTodayHabitsAsync(day);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Count(), Is.EqualTo(1));
            Assert.That(result.Value!.First().IsCompletedToday, Is.True);
        }

        [Test]
        public async Task GetTodayHabits_WhenHabitsDoesNotExist_ReturnSuccessWithEmptyList()
        {
            var userId = Guid.NewGuid();
            var day = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsAsync(userId, null)).ReturnsAsync(new List<HabitEntity>());
            _habitLogRepositoryMock.Setup(r => r.GetLogsByDateAsync(userId, It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<HabitLog>());

            var result = await _habitQueryService.GetTodayHabitsAsync(day);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        }

        [Test]
        public async Task GetTodayHabits_WhenNoLogsForToday_ReturnsHabitsMarkedAsNotCompleted()
        {
            var userId = Guid.NewGuid();
            var day = DateOnly.FromDateTime(DateTime.UtcNow);

            var habits = new List<HabitEntity>
            {
                new HabitEntity(userId, "title", null, null, null)
            };

            _userContextServiceMock
                .Setup(x => x.GetCurrentUserId())
                .Returns(userId);

            _habitRepositoryMock
                .Setup(r => r.GetHabitsAsync(userId, null))
                .ReturnsAsync(habits);

            _habitLogRepositoryMock
                .Setup(r => r.GetLogsByDateAsync(userId, day))
                .ReturnsAsync(Enumerable.Empty<HabitLog>());

            var result = await _habitQueryService.GetTodayHabitsAsync(day);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Count, Is.EqualTo(1));

            var dto = result.Value!.First();
            Assert.That(dto.IsCompletedToday, Is.False);
        }

        [Test]
        public async Task GetTodayHabits_WhenLogIsCompleted_ReturnsHabitAsCompleted()
        {
            var userId = Guid.NewGuid();
            var day = new DateOnly(2026, 1, 10);

            var habit = new HabitEntity(userId, "title", null, null, null);

            var logs = new List<HabitLog>
            {
                new HabitLog(habit.Id, day, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc))
            };

            _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
            _habitRepositoryMock.Setup(r => r.GetHabitsAsync(userId, null))
                .ReturnsAsync(new List<HabitEntity> { habit });

            _habitLogRepositoryMock.Setup(r => r.GetLogsByDateAsync(userId, day))
                .ReturnsAsync(logs);

            var result = await _habitQueryService.GetTodayHabitsAsync(day);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.First().IsCompletedToday, Is.True);
        }

        #endregion

        #region GetHabitsBetweenDatesAsync

        [Test]
        public async Task GetHabitsBetweenDates_WhitValidDataAndHabitsExist_ReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var start = new DateOnly(2026, 1, 10);
            var end = new DateOnly(2026, 1, 12);
            var habitId = Guid.NewGuid();

            var habit = new HabitEntity(userId, "title", null, null, null);

            var logs = new List<HabitLog>
            {
                new HabitLog(habitId, start, ActionType.Undone, new DateTime(2026, 1, 10, 20, 0, 0, DateTimeKind.Utc)) { Habit = habit },
                new HabitLog(habitId, end, ActionType.Completed, new DateTime(2026, 1, 12, 20, 0, 0, DateTimeKind.Utc)) { Habit = habit }
            };

            _userContextServiceMock
                .Setup(x => x.GetCurrentUserId())
                .Returns(userId);

            _habitLogRepositoryMock
                .Setup(r => r.GetLogsBetweenDatesAsync(userId, start, end))
                .ReturnsAsync(logs);

            var response = await _habitQueryService.GetHabitsBetweenDatesAsync(start, end);

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Value!.Count, Is.EqualTo(2));

            var ordered = response.Value!.ToList();

            Assert.That(ordered[0].ActionType, Is.EqualTo(ActionType.Undone));
            Assert.That(ordered[1].ActionType, Is.EqualTo(ActionType.Completed));
        }

        [Test]
        public async Task GetHabitsBetweenDates_WhenHabitsLogsDoesNotExist_ReturnFailure()
        {
            var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));
            var end = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var userId = Guid.NewGuid();

            _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(userId);
            _habitLogRepositoryMock.Setup(x => x.GetLogsBetweenDatesAsync(userId, start, end))
                .ReturnsAsync(Enumerable.Empty<HabitLog>());

            var response = await _habitQueryService.GetHabitsBetweenDatesAsync(start, end);

            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.ErrorMessage, Is.EqualTo("No files found in the given range"));

            _habitLogRepositoryMock.Verify(x => x.GetLogsBetweenDatesAsync(userId, start, end), Times.Once);
        }

        [Test]
        public async Task GetHabitsBetweenDates_WhenDatesAreInvalid_ReturnFailure()
        {
            var start = DateOnly.FromDateTime(DateTime.UtcNow);
            var end = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));

            var response = await _habitQueryService.GetHabitsBetweenDatesAsync(start, end);

            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.ErrorMessage, Is.EqualTo("Start date cannot be greater than end date."));

            _userContextServiceMock.Verify(x => x.GetCurrentUserId(), Times.Never);
            _habitLogRepositoryMock.Verify(
                x => x.GetLogsBetweenDatesAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()),
                Times.Never);
        }

        #endregion

        #region GetHabitsByActionType
        [Test]
        public async Task GetHabitsByActionType_ReturnSuccess_WhenValidData()
        {
            var actionType = ActionType.Completed;
            var userId = Guid.NewGuid();
            var day = new DateOnly(2026, 1, 10);

            var habit = new HabitEntity(userId, "title", null, null, null);

            var logs = new List<HabitLog>
            {
                new HabitLog(habit.Id, day, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc))
                {
                    Habit = habit
                }
            };

            _userContextServiceMock
                .Setup(x => x.GetCurrentUserId())
                .Returns(userId);

            _habitLogRepositoryMock
                .Setup(r => r.GetLogsByActionTypeAsync(userId, actionType, day))
                .ReturnsAsync(logs);

            var result = await _habitQueryService.GetHabitsByActionTypeAsync(actionType, day);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Count, Is.EqualTo(1));

            var dto = result.Value!.First();

            Assert.That(dto.Title, Is.EqualTo("title"));
            Assert.That(dto.IsCompletedToday, Is.True);
        }

        [Test]
        public async Task GetHabitsByActionType_WhenNoLogsExist_ReturnsEmptyList()
        {
            var userId = Guid.NewGuid();
            var actionType = ActionType.Completed;
            var day = DateOnly.FromDateTime(DateTime.UtcNow);

            _userContextServiceMock
                .Setup(x => x.GetCurrentUserId())
                .Returns(userId);

            _habitLogRepositoryMock
                .Setup(r => r.GetLogsByActionTypeAsync(userId, actionType, day))
                .ReturnsAsync(Enumerable.Empty<HabitLog>());

            var result = await _habitQueryService.GetHabitsByActionTypeAsync(actionType, day);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        }
        #endregion
    }
}
