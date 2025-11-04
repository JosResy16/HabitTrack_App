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
        private readonly IHabitLogRepository _habitLogRepository;

        public HabitQueryService(IHabitRepository habitRepository, IUserContextService userContext, IHabitLogRepository habitLogRepository)
        {
            _habitRepository = habitRepository;
            _userContextService = userContext;
            _habitLogRepository = habitLogRepository;
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

            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(habitId);

            var history = logs
                .OrderByDescending(l => l.Date)
                .Select(l => new HabitHistoryDTO
                {
                    Date = l.Date,
                    Action = l.ActionType.ToString(),
                    Description = l.ActionType switch
                    {
                        ActionType.Created => "Habit was created.",
                        ActionType.Updated => "Habit was updated.",
                        ActionType.Removed => "Habit was deleted.",
                        ActionType.Completed => "Habit was marked as completed.",
                        ActionType.Undone => "Habit was unmarked as completed.",
                        ActionType.Archived => "Habit was archived",
                        ActionType.Unarchived => "Habit was unarchived",
                        _ => "Unknown action."
                    }
                })
                .ToList();

            return Result<IEnumerable<HabitHistoryDTO>>.Success(history);
        }

        public async Task<Result<IEnumerable<HabitEntity>>> GetUserHabitsAsync(Priority? priority = null)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitRepository.GetHabitsAsync(userId, priority);

            if (!habits.Any())
                Result<IEnumerable<HabitEntity>>.Success(new List<HabitEntity>());

            return Result<IEnumerable<HabitEntity>>.Success(habits);
        }

        public async Task<Result<IEnumerable<HabitTodayDTO>>> GetTodayHabitsAsync(DateTime day)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitRepository.GetHabitsByUserIdAsync(userId);
            var todayLogs = await _habitLogRepository.GetLogsByDateAsync(userId, day);

            if (!habits.Any())
                return Result<IEnumerable<HabitTodayDTO>>.Success(new List<HabitTodayDTO>());

            var todayHabits = habits.Select(habit =>
            {
                var log = todayLogs
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
            var habits = await _habitRepository.GetHabitsByUserIdAsync(userId);

            return Result<IEnumerable<HabitHistoryDTO>>.Success(new List<HabitHistoryDTO>());
        }
    }
}
