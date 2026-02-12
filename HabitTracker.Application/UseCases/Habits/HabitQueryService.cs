using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using HabitTracker.Shared.DTOs;

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

            var habits = await _habitRepository.GetHabitsByCategoryIdAsync(categoryId, userId.Value);

            List<HabitResponseDTO> habitsDto = habits.Select(habit => MappingToHabitResponseDto(habit)).ToList();

            return Result<IEnumerable<HabitResponseDTO>>.Success(habitsDto);
        }

        public async Task<Result<HabitResponseDTO>> GetHabitByIdAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();

            var habit = await _habitRepository.GetByIdAsync(habitId);
            if (habit == null)
                return Result<HabitResponseDTO>.Failure("Habit not found");

            if (habit.UserId != userId.Value)
                return Result<HabitResponseDTO>.Failure("Not authorized");

            return Result<HabitResponseDTO>.Success(MappingToHabitResponseDto(habit));
        }

        public async Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitHistoryAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("Habit not found");

            if (habit.UserId != userId.Value)
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("Not authorized");

            var logs = await _habitLogRepository.GetLogsByHabitIdAsync(userId.Value, habitId);

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
            var habits = await _habitRepository.GetHabitsAsync(userId.Value, priority);

            var habitsResponseDto = habits.Select(habit => MappingToHabitResponseDto(habit)).ToList();

            return Result<IEnumerable<HabitResponseDTO>>.Success(habitsResponseDto);
        }

        public async Task<Result<IEnumerable<HabitTodayDTO>>> GetTodayHabitsAsync(DateOnly day)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitRepository.GetHabitsAsync(userId.Value);

            if (!habits.Any())
                return Result<IEnumerable<HabitTodayDTO>>.Success([]);

            var todayHabitsBase = habits
                .Where(h => HabitAppliesToDate(h, day))
                .ToList();

            var result = new List<HabitTodayDTO>();

            foreach (var habit in todayHabitsBase)
            {
                var lastLog = await _habitLogRepository
                    .GetLastLogForDateAsync(userId.Value, habit.Id, day);

                bool isCompletedToday =
                    lastLog?.ActionType == ActionType.Completed;

                result.Add(new HabitTodayDTO
                {
                    Id = habit.Id,
                    Title = habit.Title,
                    Description = habit.Description,
                    CategoryId = habit.CategoryId,
                    Priority = habit.Priority,
                    IsCompletedToday = isCompletedToday,
                    LastTimeDoneAt = lastLog?.Date
                });
            }

            return Result<IEnumerable<HabitTodayDTO>>.Success(result);
        }

        public async Task<Result<IEnumerable<HabitHistoryDTO>>> GetHabitsBetweenDatesAsync(DateOnly startDate, DateOnly endDate)
        {

            if (startDate > endDate)
                return Result<IEnumerable<HabitHistoryDTO>>.Failure("Start date cannot be greater than end date.");

            var userId = _userContextService.GetCurrentUserId();
            var logs = await _habitLogRepository.GetLogsBetweenDatesAsync(userId.Value, startDate, endDate);

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
                .GetLogsByActionTypeAsync(userId.Value, actionType, day);

            if (!logs.Any())
                return Result<IEnumerable<HabitTodayDTO>>
                    .Success(Enumerable.Empty<HabitTodayDTO>());

            var habits = logs.Select(l => MapHabitLogToHabitTodayDto(l, actionType)).ToList();

            return Result<IEnumerable<HabitTodayDTO>>.Success(habits);
        }

        private static bool HabitAppliesToDate(HabitEntity habit, DateOnly day)
        {
            if (habit.RepeatPeriod == null)
                return true;

            return habit.RepeatPeriod switch
            {
                Period.Daily => true,
                Period.Weekly => habit.CreatedAt.DayOfWeek == day.DayOfWeek,
                Period.Monthly => habit.CreatedAt.Day == day.Day,
                _ => false
            };
        }

        private static HabitResponseDTO MappingToHabitResponseDto(HabitEntity habit)
        {
            return new HabitResponseDTO
            {
                Id = habit.Id,
                Title = habit.Title,
                Description = habit.Description,
                CategoryId = habit.CategoryId,
                Priority = habit.Priority,
                RepeatCount = habit.RepeatCount,
                RepeatInterval = habit.RepeatInterval,
                RepeatPeriod = habit.RepeatPeriod,
                Duration = habit.Duration,
                LastTimeDoneAt = habit.LastTimeDoneAt,
                CreatedAt = habit.CreatedAt,
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
                LastTimeDoneAt = log?.Date,
                Priority = log.Habit.Priority
            };
        }
    }
}
