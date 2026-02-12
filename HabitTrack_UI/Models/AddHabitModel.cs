using HabitTracker.Domain;
using System.ComponentModel.DataAnnotations;

namespace HabitTrack_UI.Models;
public class AddHabitModel
{
    [Required]
    [MaxLength(50)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(150)]
    public string Description { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }
    public Priority? Priority { get; set; }

    [Range(1, 30)]
    public int RepeatInterval { get; set; } = 1;

    [Required]
    public Period? RepeatPeriod { get; set; } = Period.Daily;
}

