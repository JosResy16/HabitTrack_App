

using HabitTracker.Domain;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Application.DTOs;
public class UpdateHabitDTO
{
    [Required]
    public string Title { get; set; }

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    public Priority? Priority { get; set; }

    public Period? RepeatPeriod { get; set; }

    public int? RepeatInterval { get; set; }
    public TimeOnly? Duration { get; set; }
}

