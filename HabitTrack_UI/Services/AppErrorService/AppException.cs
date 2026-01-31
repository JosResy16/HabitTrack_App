using HabitTrack_UI.Models.ErrorModels;

namespace HabitTrack_UI.Services.AppErrorService;

public class AppException : Exception
{
    public AppError Error { get; }

    public AppException(AppError error) : base(error.Message)
    {
        Error = error;
    }
}

