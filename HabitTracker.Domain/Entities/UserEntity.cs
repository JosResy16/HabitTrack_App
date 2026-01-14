

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Domain.Entities
{
    public class UserEntity
    {
        public Guid Id { get; private set; }

        public string UserName { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string PasswordHashed { get; private set; } = string.Empty;
        public UserRole Role { get; private set; } = UserRole.User;
        public string? RefreshToken { get; private set; } = string.Empty;
        public DateTime? RefreshTokenExpiryTime { get; private set; }

        public string? PasswordResetToken { get; private set; }
        public DateTime? PasswordResetTokenExpiry { get; private set; }

        private UserEntity() { }

        public UserEntity(string userName, string email)
        {
            UserName = userName;
            Email = email;
        }

        public void SetPassword(string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
            {
                throw new ArgumentException("Password hash can not be empty");
            }

            PasswordHashed = hashedPassword;
        }
        public bool VerifyPassword(string password)
        {
            return new PasswordHasher<UserEntity>()
                .VerifyHashedPassword(this, PasswordHashed, password)
                != PasswordVerificationResult.Failed;
        }

        public bool IsValidRefreshToken(string token)
        {
            return RefreshToken == token &&
                   RefreshTokenExpiryTime > DateTime.UtcNow;
        }

        public void UpdateRefreshToken(string? token, DateTime expiry)
        {
            RefreshToken = token;
            RefreshTokenExpiryTime = expiry;
        }

        public void SetPasswordResetToken(string token, DateTime expiry)
        {
            PasswordResetToken = token;
            PasswordResetTokenExpiry = expiry;
        }

        public void ClearPasswordResetToken()
        {
            PasswordResetToken = null;
            PasswordResetTokenExpiry = null;
        }
    }
}
