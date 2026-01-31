using HabitTrack_UI.Models.ErrorModels;
using System.Net;

namespace HabitTrack_UI.Services;
public class ErrorService
{
    public event Action<AppError>? OnError;

    public void Raise(AppError error)
    {
        OnError?.Invoke(error);
    }

    public AppError FromHttp(Exception _)
    {
        return new AppError
        {
            Type = ErrorType.Network,
            Title = "Network error",
            Message = "Could not connect to the server"
        };
    }

    public AppError FromStatusCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new AppError
            {
                Type = ErrorType.Unauthorized,
                Title = "Unauthorized",
                Message = "Session expired"
            },
            HttpStatusCode.InternalServerError => new AppError
            {
                Type = ErrorType.Server,
                Title = "Server error",
                Message = "Internal server error"
            },
            _ => new AppError
            {
                Type = ErrorType.Unknown,
                Title = "Error",
                Message = "Something went wrong"
            }
        };
    }
}

