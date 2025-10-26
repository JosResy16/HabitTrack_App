

using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Application.DTOs
{
    public class CategoryDTO
    {
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
    }
}
