using Microsoft.EntityFrameworkCore;
using TaskHabitBookmarkApp.Models;

namespace TaskHabitBookmarkApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Habit> Habits => Set<Habit>();
        public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<BookmarkTag> BookmarkTags => Set<BookmarkTag>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<BookmarkTag>()
                .HasKey(bt => new { bt.BookmarkId, bt.TagId });

            modelBuilder.Entity<BookmarkTag>()
                .HasOne(bt => bt.Bookmark)
                .WithMany(b => b.BookmarkTags)
                .HasForeignKey(bt => bt.BookmarkId);

            modelBuilder.Entity<BookmarkTag>()
                .HasOne(bt => bt.Tag)
                .WithMany(t => t.BookmarkTags)
                .HasForeignKey(bt => bt.TagId);
        }
    }
}
