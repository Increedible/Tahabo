using System.ComponentModel.DataAnnotations;

namespace TaskHabitBookmarkApp.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Notes { get; set; }

        [Display(Name = "Due date")]
        public DateTime? DueDate { get; set; }

        public Priority Priority { get; set; } = Priority.Normal;

        [Display(Name = "Completed")]
        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }
}
