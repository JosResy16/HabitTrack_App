

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Application.UseCases.Habits
{
    public class HabitServices : IHabitsService
    {
        private readonly IHabitRepository _habitRepository;
        private readonly IUserContextService _userContextService;

        public HabitServices(IHabitRepository habitRepository, IUserContextService userContextService)
        {
            _habitRepository = habitRepository;
            _userContextService = userContextService;
        }

        public async Task<Habit> AddNewHabitAsync(HabitDTO habitDto)
        {
            if (string.IsNullOrEmpty(habitDto.Title))
                throw new ValidationException("Title is required.");

            if (habitDto.Title.Length > 100)
                throw new ValidationException("Title can not exceed 100 characters.");

            var userId = _userContextService.GetCurrentUserId();

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("Invalid user context.");

            Habit habit = new Habit
            {
                Id = Guid.NewGuid(),
                Title = habitDto.Title,
                Description = habitDto.Description,
                UserId = userId,
            };

            await _habitRepository.AddAsync(habit);
            return habit;
        }

        public async Task<Habit> GetHabitByIdAsync(Guid habitId)
        {
            var userId = _userContextService.GetCurrentUserId();

            var habit = await _habitRepository.GetByIdAsync(habitId);
            if (habit == null || habit.UserId != userId)
                throw new KeyNotFoundException("habit not found");
            return habit;
        }

        public async Task<List<Habit>> GetHabitsByUserIdAsync()
        {
            var userId = _userContextService.GetCurrentUserId();
            return await _habitRepository.GetHabitsByUserIdAsync(userId);
        }

        public async Task RemoveHabitAsync(Habit habit)
        {
            await _habitRepository.DeleteAsync(habit.Id);
        }

        public async Task<Habit?> UpdateHabit(Guid habitId, HabitDTO habitDto)
        {
            var userId = _userContextService.GetCurrentUserId();

            var existingHabit = await _habitRepository.GetByIdAsync(habitId);

            if (existingHabit == null)
                throw new KeyNotFoundException();

            if (existingHabit.UserId != userId)
                throw new UnauthorizedAccessException();

            existingHabit.Title = habitDto.Title;
            existingHabit.Description = habitDto.Description;
            await _habitRepository.UpdateAsync(existingHabit);

            return existingHabit;
        }
    }
}
