using System.ComponentModel.DataAnnotations;

namespace HabitTrack_UI.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password required")]
        [MinLength(8, ErrorMessage = "Min 8 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password required")]
        [Compare(nameof(Password), ErrorMessage="Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
