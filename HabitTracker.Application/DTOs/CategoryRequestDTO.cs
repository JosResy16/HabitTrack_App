

using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Application.DTOs
{
    public class CategoryRequestDTO
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = string.Empty;
    }
}
