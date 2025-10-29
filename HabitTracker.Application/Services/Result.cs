using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Services
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? ErrorMessage { get; }
        private Result(bool isSuccess, T? value, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }
        public static Result<T> Success(T value) => new Result<T>(true, value, null);
        public static Result<T> Failure(string errorMessage) => new Result<T>(false, default, errorMessage);
    }

    public class Result
    {
        public bool IsSuccess {  get; }
        public string? ErrorMessage { get; }

        private Result(bool isSucces, string? errorMessage)
        {
            IsSuccess = isSucces;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new Result(true, null);

        public static Result Failure(string error) => new Result(false, error);
    }
}
