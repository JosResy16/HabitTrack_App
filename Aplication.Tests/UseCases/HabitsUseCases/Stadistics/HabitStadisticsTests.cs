using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using Moq;
using System.Security.Cryptography;

namespace Application.Tests.UseCases.HabitsUseCases.Stadistics;

internal class HabitStadisticsTests
{
    private Mock<IHabitLogRepository> _habitLogRepositoryMock;
    private Mock<IUserContextService> _userContextServiceMock;
    private Mock<IDateTimeProvider> _dateTimeProviderMock;
    private IHabitStatisticsService _habitStadisticsService;

    [SetUp]
    public void Setup()
    {
        _habitLogRepositoryMock = new Mock<IHabitLogRepository>();
        _userContextServiceMock = new Mock<IUserContextService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();

        _habitStadisticsService = new HabitStatisticsService(
            _habitLogRepositoryMock.Object,
            _userContextServiceMock.Object,
            _dateTimeProviderMock.Object
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

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(now);
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

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(now);
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

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(now);
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

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(now);
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

        _dateTimeProviderMock.Setup(d => d.UtcNow).Returns(now);
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

}

