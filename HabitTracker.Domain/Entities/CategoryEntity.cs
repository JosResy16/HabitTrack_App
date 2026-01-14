using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Domain.Entities
{
    public class CategoryEntity
    {
        private CategoryEntity() { }

        public CategoryEntity(Guid userId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Category title is required");

            Id = Guid.NewGuid();
            UserId = userId;
            Title = title;
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }
        public Guid Id { get; private set; }
        public string? Title { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsDeleted { get; private set; }

        public void Rename(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title is required");

            Title = newTitle;
        }

        public void SoftDelete()
        {
            if (IsDeleted) return;
            IsDeleted = true;
        }
    }
}
