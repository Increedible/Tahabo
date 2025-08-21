using System.ComponentModel.DataAnnotations;

namespace TaskHabitBookmarkApp.Models
{
    public class Bookmark
    {
        public int Id { get; set; }

        [Required, StringLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required, Url, StringLength(2048)]
        public string Url { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Notes { get; set; }

        [Display(Name = "Favorite")]
        public bool IsFavorite { get; set; }

        public int Clicks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookmarkTag> BookmarkTags { get; set; } = new List<BookmarkTag>();
    }
}
