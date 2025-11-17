using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public class HabitQueryService : IHabitQueryService
    {
        private readonly IHabitRepository _habitRepository;
        private readonly IUserContextService _userContextService;
        private readonly IHabitLogService _habitLogService;

        public HabitQueryService(IHabitRepository habitRepository, IUserContextService userContext, IHabitLogService habitLogService)
        {
            _habitRepository = habitRepository;
            _userContextService = userContext;
            _habitLogService = habitLogService;
        }

        public async Task<Result<IEnumerable<HabitEntity>>> GetHabitsByCategoryAsync(Guid categoryId)
        {
            var userId = _userContextService.GetCurrentUserId();

            var habits = await _habitRepository.GetHabitsByCategoryIdAsync(categoryId, userId);
            if (habits == null)
                return Result<IEnumerable<HabitEntity>>.Success(new List<HabitEntity>());

            return Result<IEnumerable<HabitEntity>>.Success(habits);
        }

        public async Task<Result<HabitEntity>> GetHabitByIdAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();

            var habit = await _habitRepository.GetByIdAsync(habitId);
            if (habit == null)
                return Result<HabitEntity>.Failure("Habit not found");

            if (habit.UserId != userId)
                return Result<HabitEntity>.Failure("Not authorize");

            return Result<HabitEntity>.Success(habit);
        }

        public async Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitHistoryAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("Habit not found");

            if (habit.UserId != userId)
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("Not authorized");

            var logs = await _habitLogService.GetLogsByHabitAsync(habitId);

            var history = logs.Value?
                .OrderByDescending(l => l.Date)
                .Select(l => new HabitHistoryDTO
                {
                    HabitId = l.HabitId,
                    HabitTitle = habit.Title,
                    Date = l.Date,
                    ActionType = l.ActionType,
                    
                })
                .ToList();

            return Result<IEnumerable<HabitHistoryDTO>>.Success(history);
        }

        public async Task<Result<IEnumerable<HabitEntity>>> GetUserHabitsAsync(Priority? priority = null)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitRepository.GetHabitsAsync(userId, priority);

            return Result<IEnumerable<HabitEntity>>.Success(habits);
        }

        public async Task<Result<IEnumerable<HabitTodayDTO>>> GetTodayHabitsAsync(DateTime day)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitRepository.GetHabitsByUserIdAsync(userId);
            var todayLogs = await _habitLogService.GetLogsByDateAsync(day);

            if (!habits.Any())
                return Result<IEnumerable<HabitTodayDTO>>.Success(new List<HabitTodayDTO>());

            var todayHabits = habits.Select(habit =>
            {
                var log = todayLogs.Value?
                    .Where(l => l.HabitId == habit.Id)
                    .OrderByDescending(l => l.Date)
                    .FirstOrDefault();

                bool isCompletedToday = log?.ActionType == ActionType.Completed;

                return new HabitTodayDTO
                {
                    Id = habit.Id,
                    Title = habit.Title,
                    Description = habit.Description,
                    CategoryId = habit.CategoryId,
                    Priority = habit.Priority,
                    IsCompletedToday = isCompletedToday,
                    LastTimeDoneAt = habit.LastTimeDoneAt
                };
            }).ToList();

            return Result<IEnumerable<HabitTodayDTO>>.Success(todayHabits);
        }

        public async Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitsBetweenDatesAsync(DateTime startDate, DateTime endDate)
        {
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogService.GetLogsBetweenDatesAsync(startDate, endDate);

            if (!logs.Value.Any())
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("No logs found in the given range");

            var history = logs.Value.Select(l => new HabitHistoryDTO
            {
                HabitId = l.Id,
                HabitTitle = l.Habit.Title,
                Date = l.Date,
                ActionType = l.ActionType
            }).ToList();

            return Result<IEnumerable<HabitHistoryDTO>>.Success(history);
        }

        public async Task<Result<IEnumerable<HabitTodayDTO>>> GetHabitsByActionTypeAsync(ActionType actionType, DateTime day)
        {
            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogService.GetLogsByActionTypeAsync(actionType, day);

            if (!logs.Value.Any())
                return Result<IEnumerable<HabitTodayDTO>>.Success(new List<HabitTodayDTO>());

            var habits = logs.Value.Select(l => new HabitTodayDTO
            {
                Id = l.HabitId,
                Title = l.Habit.Title,
                Description = l.Habit.Description,
                Priority = l.Habit.Priority,
                IsCompletedToday = actionType == ActionType.Completed,
                LastTimeDoneAt = l.Habit.LastTimeDoneAt
            }).ToList();

            return Result<IEnumerable<HabitTodayDTO>>.Success(habits);
        }
    }
}
