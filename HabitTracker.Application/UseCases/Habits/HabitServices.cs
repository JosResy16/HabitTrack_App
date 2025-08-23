

using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.DTOs;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public class HabitServices : IHabitsService
    {
        private readonly IHabitRepository _habitRepository;

        public HabitServices(IHabitRepository habitRepository)
        {
            _habitRepository = habitRepository;
        }

        public async Task<Habit> AddNewHabitAsync(Guid userId, HabitDTO habitDto)
        {
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

        public async Task<Habit> GetHabitByIdAsync(Guid habitId, Guid userId)
        {
            var habit = await _habitRepository.GetByIdAsync(habitId);
            if (habit == null || habit.UserId != userId)
                throw new Exception("habit not found");
            return habit;
        }

        public async Task<List<Habit>> GetHabitsByUserIdAsync(Guid id)
        {
            return await _habitRepository.GetHabitsByUserIdAsync(id);
        }

        public async Task RemoveHabitAsync(Habit habit)
        {
            await _habitRepository.DeleteAsync(habit.Id);
        }

        public async Task<Habit?> UpdateHabit(Guid habitId, HabitDTO habitDto, Guid userId)
        {
            var existingHabit = await _habitRepository.GetByIdAsync(habitId);
            if (existingHabit == null)
                return null;

            if (existingHabit.UserId != userId)
                return null;

            existingHabit.Title = habitDto.Title;
            existingHabit.Description = habitDto.Description;
            await _habitRepository.UpdateAsync(existingHabit);

            return existingHabit;
        }
    }
}
