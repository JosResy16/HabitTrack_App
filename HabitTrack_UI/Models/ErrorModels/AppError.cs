namespace HabitTrack_UI.Models.ErrorModels;
public record AppError
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public ErrorType Type { get; init; }
}

