

using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Application.DTOs
{
    public class CategoryResponseDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
