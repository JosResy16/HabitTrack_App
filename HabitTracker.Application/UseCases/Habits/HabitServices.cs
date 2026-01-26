using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using HabitTracker.Application.Services;

namespace HabitTracker.Application.UseCases.Habits
{
    public class HabitServices : IHabitsService
    {
        private readonly IHabitRepository _habitRepository;
        private readonly IUserContextService _userContextService;
        private readonly IHabitLogService _habitLogService;

        public HabitServices(IHabitRepository habitRepository, IUserContextService userContextService, IHabitLogService habitLogService)
        {
            _habitRepository = habitRepository;
            _userContextService = userContextService;
            _habitLogService = habitLogService;
        }

        public async Task<Result<HabitResponseDTO>> AddNewHabitAsync(CreateHabitDTO habitDto)
        {
            var userId = _userContextService.GetCurrentUserId();

            var existHabitWithSameTitle = await _habitRepository.GetByTitleAsync(userId.Value, habitDto.Title);
            if (existHabitWithSameTitle != null)
                return Result<HabitResponseDTO>.Failure("Already exists an habit with the same Title");

            var habit = new HabitEntity(
                userId: userId.Value,
                title: habitDto.Title,
                description: habitDto.Description,
                repeatPeriod: habitDto.RepeatPeriod,
                repeatInterval: habitDto.RepeatInterval)
            {
                CategoryId = habitDto.CategoryId,
                Priority = habitDto.Priority,
                RepeatCount = habitDto.RepeatCount,
                Duration = habitDto.Duration
            };

            await _habitRepository.AddAsync(habit);
            await _habitLogService.AddLogAsync(habit.Id, ActionType.Created);

            await _habitRepository.SaveChangesAsync();

            var response = new HabitResponseDTO
            {
                Id = habit.Id,
                Title = habit.Title,
                Description = habit.Description,
                CategoryId = habit.CategoryId,
                Priority = habit.Priority,
                RepeatCount = habit.RepeatCount,
                RepeatInterval = habit.RepeatInterval,
                RepeatPeriod = habit.RepeatPeriod,
                Duration = habit.Duration
            };
            return Result<HabitResponseDTO>.Success(response);
        }

        public async Task<Result> MarkHabitAsDone(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result.Failure("Habit not found");

            if (habit.UserId != userId.Value)
                return Result.Failure("Not authorized");

            habit.MarkHabitAsDone();

            await _habitLogService.AddLogAsync(habitId, ActionType.Completed);

            await _habitRepository.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> RemoveHabitAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result.Failure("Habit not found");

            if (habit.UserId != userId.Value)
                return Result.Failure("Not authorized");

            if (habit.IsDeleted)
                return Result.Success();

            habit.SoftDelete();

            await _habitLogService.AddLogAsync(habitId, ActionType.Removed);

            await _habitRepository.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> UndoHabitCompletion(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result.Failure("Habit not found");

            if (habit.UserId != userId.Value)
                return Result.Failure("Not authorized");

            if (!habit.IsCompleted)
                return Result.Failure("Habit is not marked as completed");

            habit.UndoCompletion();

            await _habitLogService.AddLogAsync(habitId, ActionType.Undone);

            await _habitRepository.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<HabitResponseDTO?>> UpdateHabitAsync(Guid habitId, CreateHabitDTO habitDto)
        {
            var userId = _userContextService.GetCurrentUserId();

            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result<HabitResponseDTO?>.Failure("Habit not found");

            if (habit.UserId != userId.Value)
                return Result<HabitResponseDTO?>.Failure("Not authorized");

            if (!string.Equals(habit.Title, habitDto.Title, StringComparison.OrdinalIgnoreCase))
            {
                var existHabitWithSameTitle = await _habitRepository.GetByTitleAsync(userId.Value, habitDto.Title);
                if (existHabitWithSameTitle != null && existHabitWithSameTitle.Id != habitId)
                    return Result<HabitResponseDTO?>.Failure("Already exists an habit with the same Title");
            }

            habit.UpdateDetails(
                title: habitDto.Title,
                description: habitDto.Description,
                priority: habitDto.Priority
            );

            habit.Duration = habitDto.Duration;
            habit.RepeatInterval = habitDto.RepeatInterval;
            habit.CategoryId = habitDto.CategoryId;
            
            await _habitLogService.AddLogAsync(habitId, ActionType.Updated);

            await _habitRepository.SaveChangesAsync();

            var response = new HabitResponseDTO
            {
                Id = habit.Id,
                Title = habit.Title,
                Description = habit.Description,
                CategoryId = habit.CategoryId,
                Priority = habit.Priority,
                RepeatCount = habit.RepeatCount,
                RepeatInterval = habit.RepeatInterval,
                RepeatPeriod = habit.RepeatPeriod,
                Duration = habit.Duration
            };

            return Result<HabitResponseDTO?>.Success(response);
        }
    }
}
