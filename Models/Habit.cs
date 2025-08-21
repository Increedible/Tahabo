using System.ComponentModel.DataAnnotations;

namespace TaskHabitBookmarkApp.Models
{
    public class Habit
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public Frequency Frequency { get; set; } = Frequency.Daily;

        public int Streak { get; set; }

        public int BestStreak { get; set; }

        public DateTime? LastDoneDate { get; set; } // date-only semantics; store as DateTime for simplicity
    }
}
