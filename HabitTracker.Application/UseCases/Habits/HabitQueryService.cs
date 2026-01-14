using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using HabitTracker.Shared.DTOs;
using HabitTracker.Shared.Enums;

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

        public async Task<Result<IEnumerable<HabitResponseDTO>>> GetHabitsByCategoryAsync(Guid categoryId)
        {
            var userId = _userContextService.GetCurrentUserId();

            var habits = await _habitRepository.GetHabitsByCategoryIdAsync(categoryId, userId);

            List<HabitResponseDTO> habitsDto = habits.Select(habit => MappingToHabitResponseDto(habit)).ToList();

            return Result<IEnumerable<HabitResponseDTO>>.Success(habitsDto);
        }

        public async Task<Result<HabitResponseDTO>> GetHabitByIdAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();

            var habit = await _habitRepository.GetByIdAsync(habitId);
            if (habit == null)
                return Result<HabitResponseDTO>.Failure("Habit not found");

            if (habit.UserId != userId)
                return Result<HabitResponseDTO>.Failure("Not authorized");

            return Result<HabitResponseDTO>.Success(MappingToHabitResponseDto(habit));
        }

        public async Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitHistoryAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("Habit not found");

            if (habit.UserId != userId)
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("Not authorized");

            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId, habitId);

            var history = logs
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

        public async Task<Result<IEnumerable<HabitResponseDTO>>> GetUserHabitsAsync(Priority? priority = null)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitRepository.GetHabitsAsync(userId, priority);

            var habitsResponseDto = habits.Select(habit => MappingToHabitResponseDto(habit)).ToList();

            return Result<IEnumerable<HabitResponseDTO>>.Success(habitsResponseDto);
        }

        public async Task<Result<IEnumerable<HabitTodayDTO>>> GetTodayHabitsAsync(DateOnly day)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitRepository.GetHabitsAsync(userId);
            var todayLogs = await _habitLogRepository.GetLogsByDateAsync(userId, day);

            if (!habits.Any())
                return Result<IEnumerable<HabitTodayDTO>>.Success(new List<HabitTodayDTO>());

            var logsByHabitId = todayLogs
                .GroupBy(l => l.HabitId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.Date).First());

            var todayHabits = habits.Select(habit =>
            {
                logsByHabitId.TryGetValue(habit.Id, out var log);

                bool isCompletedToday = log?.ActionType == ActionType.Completed;

                return new HabitTodayDTO
                {
                    Id = habit.Id,
                    Title = habit.Title,
                    Description = habit.Description,
                    CategoryId = habit.CategoryId,
                    IsCompletedToday = isCompletedToday,
                    LastTimeDoneAt = habit.LastTimeDoneAt,
                    Priority = MapPriority(habit.Priority)
                };
            }).ToList();

            return Result<IEnumerable<HabitTodayDTO>>.Success(todayHabits);
        }

        public async Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitsBetweenDatesAsync(DateOnly startDate, DateOnly endDate)
        {

            if (startDate > endDate)
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("Start date cannot be greater than end date.");

            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsBetweenDatesAsync(userId, startDate, endDate);

            if (!logs.Any())
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("No files found in the given range");

            var history = logs.Select(l => new HabitHistoryDTO
            {
                HabitId = l.HabitId,
                HabitTitle = l.Habit.Title,
                Date = l.Date,
                ActionType = l.ActionType
            }).ToList();

            return Result<IEnumerable<HabitHistoryDTO>>.Success(history);
        }

        public async Task<Result<IEnumerable<HabitTodayDTO>>> GetHabitsByActionTypeAsync(ActionType actionType, DateOnly day)
        {
            var userId = _userContextService.GetCurrentUserId();

            var logs = await _habitLogRepository
                .GetLogsByActionTypeAsync(userId, actionType, day);

            if (!logs.Any())
                return Result<IEnumerable<HabitTodayDTO>>
                    .Success(Enumerable.Empty<HabitTodayDTO>());

            var habits = logs.Select(l => MapHabitLogToHabitTodayDto(l, actionType)).ToList();

            return Result<IEnumerable<HabitTodayDTO>>.Success(habits);
        }

        private static HabitPriority? MapPriority(Priority? priority)
        {
            return priority switch
            {
                Priority.none => HabitPriority.none,
                Priority.Low => HabitPriority.Low,
                Priority.Medium => HabitPriority.Medium,
                Priority.High => HabitPriority.High,
                Priority.VeryHigh => HabitPriority.VeryHigh,
                _ => null
            };
        }

        private static HabitResponseDTO MappingToHabitResponseDto(HabitEntity habit)
        {
            return new HabitResponseDTO
            {
                Id = Guid.NewGuid(),
                Title = habit.Title,
                Description = habit.Description,
                CategoryId = habit.CategoryId,
                Priority = habit.Priority,
                RepeatCount = habit.RepeatCount,
                RepeatInterval = habit.RepeatInterval,
                RepeatPeriod = habit.RepeatPeriod,
                Duration = habit.Duration,
                LastTimeDoneAt = habit.LastTimeDoneAt
            };
        }

        private static HabitTodayDTO MapHabitLogToHabitTodayDto(HabitLog log, ActionType actionType)
        {
            return new HabitTodayDTO
            {
                Id = log.HabitId,
                Title = log.Habit.Title,
                Description = log.Habit.Description,
                IsCompletedToday = actionType == ActionType.Completed,
                LastTimeDoneAt = log.Habit.LastTimeDoneAt,
                Priority = MapPriority(log.Habit.Priority)
            };
        }
    }
}
