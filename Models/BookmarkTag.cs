namespace TaskHabitBookmarkApp.Models
{
    public class BookmarkTag
    {
        public int BookmarkId { get; set; }
        public Bookmark Bookmark { get; set; } = default!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = default!;
    }
}
