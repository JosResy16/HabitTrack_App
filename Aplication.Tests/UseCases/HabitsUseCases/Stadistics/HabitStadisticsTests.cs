using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;


namespace Application.Tests.UseCases.HabitsUseCases.Stadistics;

internal class HabitStadisticsTests
{
    private Mock<IHabitLogRepository> _habitLogRepositoryMock;
    private Mock<IUserContextService> _userContextServiceMock;
    private Mock<IUserDataTimeService> _userDataTimeServiceMock;
    private Mock<IHabitRepository> _habitRepositoryMock;
    private IHabitStatisticsService _habitStadisticsService;

    [SetUp]
    public void Setup()
    {
        _habitLogRepositoryMock = new Mock<IHabitLogRepository>();
        _userContextServiceMock = new Mock<IUserContextService>();
        _userDataTimeServiceMock = new Mock<IUserDataTimeService>();
        _habitRepositoryMock = new Mock<IHabitRepository>();


        _habitStadisticsService = new HabitStatisticsService(
            _habitLogRepositoryMock.Object,
            _userContextServiceMock.Object,
            _habitRepositoryMock.Object,
            _userDataTimeServiceMock.Object
        );
    }

    #region GetCompletitionRateasync

    [Test]
    public async Task CompletionRate_WhenLastLogIsCompleted_CountsDay()
    {
        var day = new DateOnly(2026, 1, 10);
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, day, ActionType.Undone, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, day, ActionType.Completed, new DateTime(2026, 1, 10, 18, 0, 0, DateTimeKind.Utc))
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsBetweenDatesAsync(It.IsAny<Guid>(), day, day))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCompletionRateAsync(day, day);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(100));
    }

    [Test]
    public async Task CompletionRate_WhenLastLogIsUndone_DoesNotCountDay()
    {
        var day = new DateOnly(2026, 1, 10);
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, day, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, day, ActionType.Undone, new DateTime(2026, 1, 10, 11, 0, 0, DateTimeKind.Utc))
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsBetweenDatesAsync(userId, day, day))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCompletionRateAsync(day, day);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public async Task GetCompletionRate_WhenNoLogs_ReturnsZero()
    {
        var userId = Guid.NewGuid();
        var day = new DateOnly(2026, 1, 10);

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsBetweenDatesAsync(It.IsAny<Guid>(), day, day))
            .ReturnsAsync(new List<HabitLog>());

        var result = await _habitStadisticsService.GetCompletionRateAsync(day, day);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public async Task GetCompletionRate_WhenAllDaysCompleted_Returns100Percent()
    {
        var start = new DateOnly(2026, 1, 10);
        var end = new DateOnly(2026, 1, 12);
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, new DateOnly(2026, 1, 10), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026, 1, 11), ActionType.Completed, new DateTime(2026, 1, 11, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026, 1, 12), ActionType.Completed, new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc)),
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsBetweenDatesAsync(It.IsAny<Guid>(), start, end))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCompletionRateAsync(start, end);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(100));
    }

    [Test]
    public async Task GetCompletionRate_WhenMixedCompletedAndUndone_ReturnsCorrectPercentage()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 10);
        var end = new DateOnly(2026, 1, 11);

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, new DateOnly(2026,1,10), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1,11), ActionType.Completed,  new DateTime(2026, 1, 11, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1,11), ActionType.Undone,  new DateTime(2026, 1, 11, 15, 0, 0, DateTimeKind.Utc)),
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsBetweenDatesAsync(It.IsAny<Guid>(), start, end))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCompletionRateAsync(start, end);

        Assert.That(result.Value, Is.EqualTo(50));
    }

    #endregion

    #region GetCompletitionRateForHabitAsync

    [Test]
    public async Task GetCompletionRateForHabit_WhenStartDateGreaterThanEndDate_ReturnsFailure()
    {
        var start = new DateOnly(2026, 1, 11);
        var end = new DateOnly(2026, 1, 10);

        var result = await _habitStadisticsService.GetCompletionRateForHabitAsync(Guid.NewGuid(), start, end);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Start date cannot be greater than end date."));

        _userContextServiceMock.Verify(r => r.GetCurrentUserId(), Times.Never);
        _habitLogRepositoryMock.Verify(r => r.GetLogsByHabitIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task GetCompletionRateForHabit_WhenLogsOutOfRange_IgnoresThem()
    {
        var start = new DateOnly(2026, 1, 10);
        var end = new DateOnly(2026, 1, 11);
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, new DateOnly(2026,1,8), ActionType.Undone, new DateTime(2026, 1, 8, 8, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1, 9), ActionType.Undone, new DateTime(2026, 1, 9, 9, 0, 0, DateTimeKind.Utc))
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCompletionRateForHabitAsync(habitId, start, end);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public async Task GetCompletionRateForHabit_WhenNoLogsExist_ReturnsZero()
    {
        var start = new DateOnly(2026, 1, 10);
        var end = new DateOnly(2026, 1, 11);
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(new List<HabitLog>());

        var result = await _habitStadisticsService.GetCompletionRateForHabitAsync(habitId, start, end);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public async Task GetCompletionRateForHabit_WhenValidLogs_ReturnsCorrectRate()
    {
        var start = new DateOnly(2026, 1, 10);
        var end = new DateOnly(2026, 1, 11);
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, new DateOnly(2026,1,10), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1, 11), ActionType.Completed, new DateTime(2026, 1, 10, 11, 0, 0, DateTimeKind.Utc))
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCompletionRateForHabitAsync(habitId, start, end);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(100));
    }

    #endregion

    #region CurrentStreakAsync

    [Test]
    public async Task GetCurrentStreak_WhenNoLogs_ReturnsZero()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);

        _userDataTimeServiceMock.Setup(d => d.UtcNow).Returns(now);
        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(new List<HabitLog>());

        var result = await _habitStadisticsService.GetCurrentStreakAsync(habitId);

        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public async Task GetCurrentStreak_WhenOnlyTodayCompleted_ReturnsOne()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, DateOnly.FromDateTime(now), ActionType.Completed, now)
        };

        _userDataTimeServiceMock.Setup(d => d.UtcNow).Returns(now);
        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock.Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCurrentStreakAsync(habitId);

        Assert.That(result.Value, Is.EqualTo(1));
    }

    [Test]
    public async Task GetCurrentStreak_WhenTodayAndYesterdayCompleted_ReturnsTwo()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);

        _userDataTimeServiceMock.Setup(d => d.UtcNow).Returns(now);
        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, DateOnly.FromDateTime(now), ActionType.Completed, now),
            new HabitLog(habitId, DateOnly.FromDateTime(now.AddDays(-1)), ActionType.Completed, now.AddDays(-1))
        };

        _habitLogRepositoryMock
            .Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCurrentStreakAsync(habitId);

        Assert.That(result.Value, Is.EqualTo(2));
    }

    [Test]
    public async Task GetCurrentStreak_WhenGapExists_BreaksStreak()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);

        _userDataTimeServiceMock.Setup(d => d.UtcNow).Returns(now);
        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, DateOnly.FromDateTime(now), ActionType.Completed, now),
            new HabitLog(habitId, DateOnly.FromDateTime(now.AddDays(-2)), ActionType.Completed, now.AddDays(-2))
        };

        _habitLogRepositoryMock
            .Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCurrentStreakAsync(habitId);

        Assert.That(result.Value, Is.EqualTo(1));
    }

    [Test]
    public async Task GetCurrentStreak_WhenLatestLogIsNotToday_ReturnsZero()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc);

        _userDataTimeServiceMock.Setup(d => d.UtcNow).Returns(now);
        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        var logs = new List<HabitLog>
        {
            new HabitLog(
                habitId,
                DateOnly.FromDateTime(now.AddDays(-2)),
                ActionType.Completed,
                now.AddDays(-2)
            )
        };

        _habitLogRepositoryMock
            .Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetCurrentStreakAsync(habitId);

        Assert.That(result.Value, Is.EqualTo(0));
    }


    #endregion

    #region GetLongestStreakAsync

    [Test]
    public async Task GetLongestStreak_WhenNoLogs_ReturnsZero()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _userContextServiceMock
            .Setup(r => r.GetCurrentUserId())
            .Returns(Result<Guid>.Success(userId));

        _habitLogRepositoryMock
            .Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(new List<HabitLog>());

        var result = await _habitStadisticsService.GetLongestStreakAsync(habitId);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public async Task GetLongestStreak_WhenSingleCompletedDay_ReturnsOne()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var day = new DateOnly(2026, 1, 10);

        var logs = new List<HabitLog>
        {
            new HabitLog(
                habitId,
                day,
                ActionType.Completed,
                day.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)), DateTimeKind.Utc)
            )
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock
            .Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetLongestStreakAsync(habitId);

        Assert.That(result.Value, Is.EqualTo(1));
    }

    [Test]
    public async Task GetLongestStreak_WhenConsecutiveDays_ReturnsCorrectValue()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, new DateOnly(2026,1,10), ActionType.Completed, new DateTime(2026,1,10,10,0,0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1,11), ActionType.Completed, new DateTime(2026,1,11,10,0,0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1,12), ActionType.Completed, new DateTime(2026,1,12,10,0,0, DateTimeKind.Utc)),
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock
            .Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetLongestStreakAsync(habitId);

        Assert.That(result.Value, Is.EqualTo(3));
    }

    [Test]
    public async Task GetLongestStreak_WhenMultipleStreaks_ReturnsLongest()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, new DateOnly(2026,1,1), ActionType.Completed, new DateTime(2026,1,1,10,0,0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1,2), ActionType.Completed, new DateTime(2026,1,2,10,0,0, DateTimeKind.Utc)),

            new HabitLog(habitId, new DateOnly(2026,1,5), ActionType.Completed, new DateTime(2026,1,5,10,0,0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1,6), ActionType.Completed, new DateTime(2026,1,6,10,0,0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1,7), ActionType.Completed, new DateTime(2026,1,7,10,0,0, DateTimeKind.Utc)),
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock
            .Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetLongestStreakAsync(habitId);

        Assert.That(result.Value, Is.EqualTo(3));
    }

    [Test]
    public async Task GetLongestStreak_WhenLogsUnordered_ReturnsCorrectValue()
    {
        var habitId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, new DateOnly(2026,1,12), ActionType.Completed, new DateTime(2026,1,12,10,0,0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1,10), ActionType.Completed, new DateTime(2026,1,10,10,0,0, DateTimeKind.Utc)),
            new HabitLog(habitId, new DateOnly(2026,1,11), ActionType.Completed, new DateTime(2026,1,11,10,0,0, DateTimeKind.Utc)),
        };

        _userContextServiceMock.Setup(r => r.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));
        _habitLogRepositoryMock
            .Setup(r => r.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetLongestStreakAsync(habitId);

        Assert.That(result.Value, Is.EqualTo(3));
    }

    #endregion

    #region GetHabitStats
    [Test]
    public async Task GetHabitStats_WhenLogsExist_ReturnCorrectTotalCompletion()
    {
        var userId = Guid.NewGuid();
        var habitId = Guid.NewGuid();
        var today = new DateOnly(2026, 1, 3);

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, today, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-1), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-2), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc))
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _habitLogRepositoryMock.Setup(x => x.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync()).ReturnsAsync(today);

        _userDataTimeServiceMock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc));

        _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync(new HabitEntity(userId, "habit", null, null, null));

        var result = await _habitStadisticsService.GetHabtitStatsAsync(habitId);

        Assert.That(result.Value.TotalCompletion, Is.EqualTo(3));
    }

    [Test]
    public async Task GetHabitStats_CalculateCurrentStreakCorrectly()
    {
        var habitId = Guid.NewGuid();
        var today = new DateOnly(2026, 1, 3);
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, today, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-2), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-3), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-4), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc))
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _habitLogRepositoryMock.Setup(x => x.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync()).ReturnsAsync(today);

        _userDataTimeServiceMock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc));

        _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync(new HabitEntity(userId, "habit", null, null, null));

        var result = await _habitStadisticsService.GetHabtitStatsAsync(habitId);

        Assert.That(result.Value.CurrentStreak, Is.EqualTo(1));
    }

    [Test]
    public async Task GetHabitStats_CalculateLongestStreakCorrectly()
    {
        var habitId = Guid.NewGuid();
        var today = new DateOnly(2026, 1, 3);
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, today, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-1), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-2), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-4), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-5), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc))
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _habitLogRepositoryMock.Setup(x => x.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync()).ReturnsAsync(today);

        _userDataTimeServiceMock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc));

        _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync(new HabitEntity(userId, "habit", null, null, null));

        var result = await _habitStadisticsService.GetHabtitStatsAsync(habitId);

        Assert.That(result.Value.CurrentStreak, Is.EqualTo(3));
    }

    [Test]
    public async Task GetHabitStats_WhenLogsExist_CalculateCompletionRateCorretly()
    {
        var habitId = Guid.NewGuid();
        var today = new DateOnly(2026, 1, 3);
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, today, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-1), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-2), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-4), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-5), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc))
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _habitLogRepositoryMock.Setup(x => x.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync()).ReturnsAsync(today);

        _userDataTimeServiceMock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc));

        _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync(new HabitEntity(userId, "habit", null, null, null));

        var result = await _habitStadisticsService.GetHabtitStatsAsync(habitId);

        Assert.That(result.Value?.CompletionRate, Is.EqualTo(71.0));
    }

    [Test]
    public async Task GetHabitStats_WhenLogsExist_ReturnTotalTrackedDaysCorrectly()
    {
        var habitId = Guid.NewGuid();
        var today = new DateOnly(2026, 3, 20);
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, today, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-1), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-2), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-4), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-5), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc))
        };

        var habit = new HabitEntity(userId, "test", null, Period.Daily, null);

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _habitLogRepositoryMock.Setup(x => x.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync()).ReturnsAsync(today);

        _userDataTimeServiceMock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc));

        _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync(habit);

        var result = await _habitStadisticsService.GetHabtitStatsAsync(habitId);

        var createdDate = DateOnly.FromDateTime(habit.CreatedAt);
        var expectedDays = today.DayNumber - createdDate.DayNumber + 1;

        Assert.That(result.Value?.TotalTrackedDays, Is.EqualTo(expectedDays));
    }

    [Test]
    public async Task GetHabitStats_WhenLogsExist_ReturnDaysSinceLastCompletionCorrectly()
    {
        var habitId = Guid.NewGuid();
        var today = new DateOnly(2026, 3, 20);
        var userId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, today, ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-1), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-2), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-4), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-5), ActionType.Completed, new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc))
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _habitLogRepositoryMock.Setup(x => x.GetLogsByHabitIdAsync(userId, habitId))
            .ReturnsAsync(logs);

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync()).ReturnsAsync(today);

        _userDataTimeServiceMock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc));

        _habitRepositoryMock.Setup(x => x.GetByIdAsync(habitId))
            .ReturnsAsync(new HabitEntity(userId, "habit", null, null, null));

        var result = await _habitStadisticsService.GetHabtitStatsAsync(habitId);

        Assert.That(result.Value?.DaysSinceLastCompletion, Is.EqualTo(0));
    }

    #endregion

    #region GetTodaySummary

    [Test]
    public async Task GetTodaySummary_WhenHabitsExist_ReturnTotalHabitsTodayCorrectly()
    {
        var userId = Guid.NewGuid();
        var day = new DateOnly(2026, 1, 10);

        var habits = new List<HabitEntity>()
        {
            new HabitEntity(userId, "Test 1", null, null, null),
            new HabitEntity(userId, "Test 2", null, null, null)
        };
        

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _userDataTimeServiceMock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc));

        _habitRepositoryMock.Setup(x => x.GetActiveHabits(userId, null)).ReturnsAsync(habits);

        _habitLogRepositoryMock.Setup(x => x.GetLogsByDateAsync(userId, day)).ReturnsAsync(new List<HabitLog>());

        var result = await _habitStadisticsService.GetTodaySummaryAsync();

        Assert.That(result.Value?.TotalHabitsToday, Is.EqualTo(2));
    }

    [Test]
    public async Task GetTodaySummary_WhenLogsExist_ReturnCompletedHabitTodayCorrectly()
    {
        var userId = Guid.NewGuid();
        var today = new DateOnly(2026, 1, 3);
        var habitId = Guid.NewGuid();

        var logs = new List<HabitLog>()
        {
            new HabitLog(habitId, today, ActionType.Completed, new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc)),
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync()).ReturnsAsync(today);

        _habitRepositoryMock.Setup(x => x.GetActiveHabits(userId, null)).ReturnsAsync(new List<HabitEntity>());

        _habitLogRepositoryMock.Setup(x => x.GetLogsByDateAsync(userId, today)).ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetTodaySummaryAsync();

        Assert.That(result.Value?.CompletedHabitsToday, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTodaySummary_WhenLogsExist_ReturnCurrentStreakCorrectly()
    {
        var userId = Guid.NewGuid();
        var today = new DateOnly(2026, 1, 8);
        var habitId = Guid.NewGuid();

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, today, ActionType.Completed, new DateTime(2026,1,8,10,0,0,DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-1), ActionType.Completed, new DateTime(2026,1,7,10,0,0,DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-2), ActionType.Completed, new DateTime(2026,1,6,10,0,0,DateTimeKind.Utc)),
            new HabitLog(habitId, today.AddDays(-3), ActionType.Undone, new DateTime(2026,1,5,10,0,0,DateTimeKind.Utc)),
        };

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync()).ReturnsAsync(today);

        _habitRepositoryMock.Setup(x => x.GetActiveHabits(userId, null)).ReturnsAsync(new List<HabitEntity>());

        _habitLogRepositoryMock.Setup(x => x.GetLogsByDateAsync(userId, It.IsAny<DateOnly>()))
            .ReturnsAsync((Guid _, DateOnly date) => logs.Where(l => l.Date == date).ToList());

        var result = await _habitStadisticsService.GetTodaySummaryAsync();

        Assert.That(result.Value?.CurrentStreak, Is.EqualTo(3));
    }

    [Test]
    public async Task GetTodaySummary_WhenMultipleCompletions_ReturnFirstCompletionTime()
    {
        var userId = Guid.NewGuid();
        var habitId = Guid.NewGuid();
        var today = new DateOnly(2026, 1, 8);

        var firstUtc = new DateTime(2026, 1, 8, 8, 0, 0, DateTimeKind.Utc);
        var secondUtc = new DateTime(2026, 1, 8, 10, 0, 0, DateTimeKind.Utc);

        var logs = new List<HabitLog>
        {
            new HabitLog(habitId, today, ActionType.Completed, secondUtc),
            new HabitLog(habitId, today, ActionType.Completed, firstUtc)
        };

        var localTime = new DateTime(2026, 1, 8, 3, 0, 0, DateTimeKind.Utc);

        _userContextServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Result<Guid>.Success(userId));

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync())
            .ReturnsAsync(today);

        _userDataTimeServiceMock.Setup(x => x.ConvertToLocal(firstUtc))
            .ReturnsAsync(localTime);

        _habitRepositoryMock.Setup(x => x.GetActiveHabits(userId, null))
            .ReturnsAsync(new List<HabitEntity>());

        _habitLogRepositoryMock.Setup(x => x.GetLogsByDateAsync(userId, today))
            .ReturnsAsync(logs);

        var result = await _habitStadisticsService.GetTodaySummaryAsync();

        Assert.That(result.Value?.FirstCompletionAt, Is.EqualTo(localTime));
    }

    #endregion

    #region GetUserStatsAsync

    [Test]
    public async Task GetUserStatsAsync_WhenHabitsExist_ReturnTotalActiveHabits()
    {
        var userId = Guid.NewGuid();
        var today = new DateOnly(2026, 1, 15);
        var startDate = new DateOnly(2026, 1, 1);
        var endDate = new DateOnly(2026, 1, 31);

        _userContextServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Result<Guid>.Success(userId));

        _userDataTimeServiceMock.Setup(x => x.GetTodayAsync()).ReturnsAsync(today);

        _habitRepositoryMock.Setup(x => x.GetTotalActiveHabitsAsync(userId))
            .ReturnsAsync(5);

        _habitLogRepositoryMock.Setup(x => x.GetTotalCompletionsAsync(userId)).ReturnsAsync(0);
        _habitLogRepositoryMock.Setup(x => x.GetActiveDaysCountAsync(userId, startDate, today)).ReturnsAsync(0);
        _habitLogRepositoryMock.Setup(x => x.GetDistinctActiveDatesAsync(userId)).ReturnsAsync(new List<DateOnly>());
        _habitLogRepositoryMock.Setup(x => x.GetDailyActivityAsync(userId, startDate, endDate)).ReturnsAsync(new List<DailyActivityDTO>());

        _habitRepositoryMock.Setup(x => x.GetHabitPerformanceAsync(userId,
            DateOnly.FromDateTime(new DateTime(today.Year, today.Month, 1)), today))
            .ReturnsAsync(new List<HabitPerformanceDTO>());

        _habitRepositoryMock.Setup(x => x.GetPausedHabitsAsync(userId))
            .ReturnsAsync(new List<HabitEntity>());

        var result = await _habitStadisticsService.GetUserStatsAsync(startDate, endDate);

        Assert.That(result.Value.TotalActiveHabits, Is.EqualTo(5));
    }

    #endregion


}

