using HabitTracker.Application.Common.Interfaces;
using HabitTracker.Application.Services;
using HabitTracker.Domain;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.UseCases.Habits
{
    public class HabitQueryService : IHabitQueryService
    {
        private readonly IHabitRepository _habitRepository;
        private readonly IUserContextService _userContextService;

        public HabitQueryService(IHabitRepository habitRepository, IUserContextService userContext)
        {
            _habitRepository = habitRepository;
            _userContextService = userContext;
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
            if (habit == null || habit.UserId != userId)
                throw new KeyNotFoundException("habit not found");
            return Result<HabitEntity>.Success(habit);
        }

        public Task<Result<IEnumerable<HabitEntity>>> GetHabitHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<IEnumerable<HabitEntity>>> GetHabitsAsync(Priority? priority = null)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitRepository.GetHabitsAsync(userId, priority);
            if (habits == null)
                Result<IEnumerable<HabitEntity>>.Failure("habits not found");

            if (!habits.Any())
                Result<IEnumerable<HabitEntity>>.Success(new List<HabitEntity>());

            return Result<IEnumerable<HabitEntity>>.Success(habits);
        }

        public async Task<Result<IEnumerable<HabitEntity>>> GetHabitsByPriorityAsync(Priority? priority = null)
        {
            var userId = _userContextService.GetCurrentUserId();
            var habits = await _habitRepository.GetHabitsAsync(userId, priority);
            if (habits == null)
                Result<IEnumerable<HabitEntity>>.Failure("habits not found");

            if (!habits.Any())
                Result<IEnumerable<HabitEntity>>.Success(new List<HabitEntity>());

            return Result<IEnumerable<HabitEntity>>.Success(habits);
        }

        public Task<Result<IEnumerable<HabitEntity>>> GetTodayHabitsAsync(DateTime day)
        {
            throw new NotImplementedException();
        }
    }
}
