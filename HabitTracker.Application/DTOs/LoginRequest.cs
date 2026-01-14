

using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Application.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email can not be empty")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "password can not be empty")]
        public string Password { get; set; } = string.Empty;
    }
}
