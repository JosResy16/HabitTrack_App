using HabitTracker.Domain;
using System.ComponentModel.DataAnnotations;

namespace HabitTrack_UI.Models;
public class HabitFormModel
{
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; }

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    public Priority? Priority { get; set; }

    public Period? RepeatPeriod { get; set; }

    public int? RepeatInterval { get; set; }
}

