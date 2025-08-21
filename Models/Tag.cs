using System.ComponentModel.DataAnnotations;

namespace TaskHabitBookmarkApp.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string Name { get; set; } = string.Empty;

        public ICollection<BookmarkTag> BookmarkTags { get; set; } = new List<BookmarkTag>();
    }
}
