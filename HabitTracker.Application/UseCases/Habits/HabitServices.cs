

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
        private readonly IHabitLogRepository _habitLogRepository;

        public HabitServices(IHabitRepository habitRepository, IUserContextService userContextService, IHabitLogRepository habitLogRepository)
        {
            _habitRepository = habitRepository;
            _userContextService = userContextService;
            _habitLogRepository = habitLogRepository;
        }

        public async Task<Result<HabitEntity>> AddNewHabitAsync(HabitDTO habitDto)
        {
            if (string.IsNullOrEmpty(habitDto.Title))
                return Result<HabitEntity>.Failure("Title can not be empty");

            if (habitDto.Title.Length > 100)
                return Result<HabitEntity>.Failure("Title can not exceed 100 characters.");

            var userId = _userContextService.GetCurrentUserId();

            HabitEntity habit = new HabitEntity
            {
                Id = Guid.NewGuid(),
                Title = habitDto.Title,
                Description = habitDto.Description,
                UserId = userId,
            };

            await _habitRepository.AddAsync(habit);
            return Result<HabitEntity>.Success(habit);
        }

        public async Task<Result> MarkHabitAsDone(Guid habitId)
        {
            var habit = await _habitRepository.GetByIdAsync(habitId);
            if (habit == null)
                return Result.Failure("Habit not found");

            habit.Value?.MarkHabitAsDone();
            await _habitRepository.UpdateAsync(habit.Value);

            var log = new HabitLog(habit.Value.Id, DateTime.UtcNow, true );
            await _habitLogRepository.AddAsync(log);
            return Result.Success();
        }

        public async Task<Result> RemoveHabitAsync(HabitEntity habit)
        {
            var responde = await _habitRepository.DeleteAsync(habit.Id);
            if (!responde)
                return Result.Failure("Couln´t delet this habit");
            
            return Result.Success();
        }

        public async Task<Result> UndoHabitCompletion(Guid habitId)
        {
            var habit = await _habitRepository.GetByIdAsync(habitId);
            if (habit == null)
                return Result.Failure("Habit not found");

            habit.Value?.UndoCompletion();
            var result = await _habitRepository.UpdateAsync(habit.Value);
            if(!result)
                return Result.Failure("Could not update this habit");

            return Result.Success();
        }

        public async Task<Result<HabitEntity?>> UpdateHabit(Guid habitId, HabitDTO habitDto)
        {
            var userId = _userContextService.GetCurrentUserId();

            var existingHabit = await _habitRepository.GetByIdAsync(habitId);

            if (existingHabit == null)
                return Result<HabitEntity?>.Failure("Habit not found");

            if (existingHabit.Value.UserId != userId)
                return Result<HabitEntity?>.Failure("Not authorize to do this operation");

            existingHabit.Value.Title = habitDto.Title;
            existingHabit.Value.Description = habitDto.Description;
            existingHabit.Value.Priority = habitDto.Priority;
            existingHabit.Value.Duration = habitDto.Duration;
            existingHabit.Value.RepeatInterval = habitDto.RepeatInterval;
            existingHabit.Value.Category = habitDto.Category;
            
            var result = await _habitRepository.UpdateAsync(existingHabit.Value);

            return result ? 
                Result<HabitEntity?>.Success(existingHabit.Value) :
                Result<HabitEntity?>.Failure("Could not update");
        }
    }
}
