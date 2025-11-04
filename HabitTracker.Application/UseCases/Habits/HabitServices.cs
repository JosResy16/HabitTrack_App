

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;
using System.ComponentModel.DataAnnotations;
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
            if (string.IsNullOrEmpty(habitDto.Title))
                return Result<HabitResponseDTO>.Failure("Title can not be empty");

            if (habitDto.Title.Length > 100)
                return Result<HabitResponseDTO>.Failure("Title can not exceed 100 characters.");

            var userId = _userContextService.GetCurrentUserId();

            var existHabitWithSameTitle = await _habitRepository.GetByTitleAsync(userId, habitDto.Title);
            if (existHabitWithSameTitle != null)
                return Result<HabitResponseDTO>.Failure("Already exists an habit with the same Title");

            var habit = new HabitEntity
            {
                Id = Guid.NewGuid(),
                Title = habitDto.Title,
                Description = habitDto.Description,
                UserId = userId,
                CategoryId = habitDto.CategoryId,
                Priority = habitDto.Priority,
                RepeatCount = habitDto.RepeatCount,
                RepeatInterval = habitDto.RepeatInterval,
                RepeatPeriod = habitDto.RepeatPeriod,
                Duration = habitDto.Duration
            };
            var isCreated = await _habitRepository.AddAsync(habit);

            if (isCreated)
                await _habitLogService.AddLogAsync(habit.Id, ActionType.Created);

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

            if (habit.UserId != userId)
                return Result.Failure("Not authorized");

            if (habit.LastTimeDoneAt?.Date == DateTime.UtcNow.Date)
                return Result.Failure("This habit is already marked as done today");

            habit?.MarkHabitAsDone();
            var result = await _habitRepository.UpdateAsync(habit);

            if (result)
            {
                var existingLogs = await _habitLogService.GetLogsByDateAsync(DateTime.UtcNow);
                var alreadyLogged = existingLogs.Value.Any(l => l.HabitId == habit.Id);

                if (alreadyLogged)
                    return Result.Failure("Already marked this habit as done today.");

                await _habitLogService.AddLogAsync(habitId, ActionType.Completed);
            }

            return result ? Result.Success() : Result.Failure("Could not mark as done");
        }

        public async Task<Result> RemoveHabitAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result.Failure("Habit not found");

            if (habit.UserId != userId)
                return Result.Failure("Not authorized");

            var deleted = await _habitRepository.DeleteAsync(habit.Id);

            if (deleted)
                await _habitLogService.AddLogAsync(habitId, ActionType.Removed);


            return deleted ? Result.Success() : Result.Failure("Couldn't delete this habit");
        }

        public async Task<Result> UndoHabitCompletion(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result.Failure("Habit not found");

            if (habit.UserId != userId)
                return Result.Failure("Not authorized");

            if (!habit.IsCompleted)
                return Result.Failure("Habit is not marked as completed");

            habit?.UndoCompletion();
            var result = await _habitRepository.UpdateAsync(habit);

            if (result)
                await _habitLogService.AddLogAsync(habitId, ActionType.Undone);

            return result ? Result.Success() : Result.Failure("Could not update this habit") ;
        }

        public async Task<Result<HabitResponseDTO?>> UpdateHabitAsync(Guid habitId, CreateHabitDTO habitDto)
        {
            var userId = _userContextService.GetCurrentUserId();

            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
                return Result<HabitResponseDTO?>.Failure("Habit not found");

            if (habit.UserId != userId)
                return Result<HabitResponseDTO?>.Failure("Not authorize to do this operation");

            if (!string.Equals(habit.Title, habitDto.Title, StringComparison.OrdinalIgnoreCase))
            {
                var existHabitWithSameTitle = await _habitRepository.GetByTitleAsync(userId, habitDto.Title);
                if (existHabitWithSameTitle != null && existHabitWithSameTitle.Id != habitId)
                    return Result<HabitResponseDTO?>.Failure("Already exists an habit with the same Title");
            }

            habit.Title = habitDto.Title;
            habit.Description = habitDto.Description;
            habit.Priority = habitDto.Priority;
            habit.Duration = habitDto.Duration;
            habit.RepeatInterval = habitDto.RepeatInterval;
            habit.CategoryId = habitDto.CategoryId;
            
            var result = await _habitRepository.UpdateAsync(habit);

            if (result)
                await _habitLogService.AddLogAsync(habitId, ActionType.Updated);

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

            return result ? 
                Result<HabitResponseDTO?>.Success(response) :
                Result<HabitResponseDTO?>.Failure("Could not update");
        }
    }
}
