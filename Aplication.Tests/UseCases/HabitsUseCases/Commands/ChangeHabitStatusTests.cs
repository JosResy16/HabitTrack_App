

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Application.UseCases.Habits;
using HabitTracker.Domain.Entities;
using Moq;

namespace Application.Tests.UseCases.HabitsUseCases.Commands;
public class ChangeHabitStatusTests
{
    private Mock<IHabitRepository> _habitRepositoryMock;
    private Mock<IUserContextService> _userContextServiceMock;
    private Mock<IHabitLogService> _habitLogServiceMock;
    private Mock<IUserDataTimeService> _userDataTimeService;
    private HabitServices _habitService;

    [SetUp]
    public void SetUp()
    {
        _habitRepositoryMock = new Mock<IHabitRepository>();
        _userContextServiceMock = new Mock<IUserContextService>();
        _habitLogServiceMock = new Mock<IHabitLogService>();
        _userDataTimeService = new Mock<IUserDataTimeService>();
        _habitService = new HabitServices(_habitRepositoryMock.Object, _userContextServiceMock.Object, _habitLogServiceMock.Object, _userDataTimeService.Object);
    }

    #region ChangeHabitStatus
    public async Task ChangeHabitStatus_ReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var habit = new HabitEntity(userId, "title", null, null, null);

        _userContextServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Result<Guid>.Success(userId));
        _habitRepositoryMock.Setup(x => x.GetByIdAsync(habit.Id));

        var result = await _habitService.ChangeHabitStatusAsync(habit.Id);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(habit.IsPaused, Is.True);

        _habitRepositoryMock.Verify(x => x.GetByIdAsync(habit.Id), Times.Once);
    }

    public async Task ChangeHabitStatus_WhenUserIsNotOwner_ResultFailure()
    {
        var userId = Guid.NewGuid();
        var habit = new HabitEntity(userId, "title", null, null, null);

        _userContextServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Result<Guid>.Success(userId));
        _habitRepositoryMock.Setup(x => x.GetByIdAsync(Guid.NewGuid()))
            .ReturnsAsync(habit);

        var result = await _habitService.ChangeHabitStatusAsync(Guid.NewGuid());

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Not authorized"));

        _habitRepositoryMock.Verify(x => x.GetByIdAsync(habit.Id), Times.Once);
    }

    public async Task ChangeHabitStatus_WhenHabitNotExist_ResultFailure()
    {
        var userId = Guid.NewGuid();
        var habitId = Guid.NewGuid();

        _userContextServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Result<Guid>.Success(userId));
        _habitRepositoryMock.Setup(x => x.GetByIdAsync(Guid.NewGuid()))
            .ReturnsAsync((HabitEntity?)null);

        var result = await _habitService.ChangeHabitStatusAsync(habitId);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Habit not found"));

        _habitRepositoryMock.Verify(x => x.GetByIdAsync(habitId), Times.Once);
    }

    #endregion
}


