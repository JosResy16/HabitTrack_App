

using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Application.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username can not be empty")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email can not be empty")]
        [EmailAddress(ErrorMessage ="Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password can not be empty")]
        [MinLength(8, ErrorMessage = "Password must contain at least 8 characters")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage ="passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
