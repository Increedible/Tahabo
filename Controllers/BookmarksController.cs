using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHabitBookmarkApp.Data;
using TaskHabitBookmarkApp.Models;

namespace TaskHabitBookmarkApp.Controllers
{
    public class BookmarksController : Controller
    {
        private readonly AppDbContext _db;
        public BookmarksController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? q, string? tag, bool favorites = false, string sort = "new")
        {
            var query = _db.Bookmarks
                .Include(b => b.BookmarkTags).ThenInclude(bt => bt.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(b => b.Title.Contains(q) || (b.Notes ?? "").Contains(q) || b.Url.Contains(q));

            if (!string.IsNullOrWhiteSpace(tag))
                query = query.Where(b => b.BookmarkTags.Any(bt => bt.Tag.Name == tag));

            if (favorites)
                query = query.Where(b => b.IsFavorite);

            // Always put favorites first, then apply secondary sort
            query = sort switch
            {
                "clicks" => query.OrderByDescending(b => b.IsFavorite)
                                 .ThenByDescending(b => b.Clicks),
                "title"  => query.OrderByDescending(b => b.IsFavorite)
                                 .ThenBy(b => b.Title),
                _        => query.OrderByDescending(b => b.IsFavorite)
                                 .ThenByDescending(b => b.CreatedAt),
            };

            ViewBag.AllTags = await _db.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToListAsync();
            ViewBag.SelectedTag = tag;
            ViewBag.Query = q;
            ViewBag.Favorites = favorites;
            ViewBag.Sort = sort;

            return View(await query.ToListAsync());
        }

        public IActionResult Create() => View(new BookmarkEditVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookmarkEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var bookmark = new Bookmark
            {
                Title = vm.Title,
                Url = vm.Url,
                Notes = vm.Notes,
                IsFavorite = vm.IsFavorite
            };

            await ApplyTags(bookmark, vm.TagsCsv);
            _db.Bookmarks.Add(bookmark);
            await _db.SaveChangesAsync();
            TempData["Toast"] = "Bookmark added.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var b = await _db.Bookmarks
                .Include(x => x.BookmarkTags).ThenInclude(x => x.Tag)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (b == null) return NotFound();

            var vm = new BookmarkEditVm
            {
                Id = b.Id,
                Title = b.Title,
                Url = b.Url,
                Notes = b.Notes,
                IsFavorite = b.IsFavorite,
                TagsCsv = string.Join(", ", b.BookmarkTags.Select(bt => bt.Tag.Name))
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookmarkEditVm vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var b = await _db.Bookmarks
                .Include(x => x.BookmarkTags).ThenInclude(x => x.Tag)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (b == null) return NotFound();

            b.Title = vm.Title;
            b.Url = vm.Url;
            b.Notes = vm.Notes;
            b.IsFavorite = vm.IsFavorite;

            // reset and re-apply tags
            _db.BookmarkTags.RemoveRange(b.BookmarkTags);
            b.BookmarkTags.Clear();
            await ApplyTags(b, vm.TagsCsv);

            await _db.SaveChangesAsync();
            TempData["Toast"] = "Bookmark updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var b = await _db.Bookmarks
                .Include(x => x.BookmarkTags).ThenInclude(x => x.Tag)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (b == null) return NotFound();
            return View(b);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var b = await _db.Bookmarks.FindAsync(id);
            if (b == null) return NotFound();
            return View(b);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var b = await _db.Bookmarks.FindAsync(id);
            if (b == null) return NotFound();
            _db.Bookmarks.Remove(b);
            await _db.SaveChangesAsync();
            TempData["Toast"] = "Bookmark deleted.";
            return RedirectToAction(nameof(Index));
        }

        // increments clicks and redirects to the URL
        [HttpGet]
        public async Task<IActionResult> Visit(int id)
        {
            var b = await _db.Bookmarks.FindAsync(id);
            if (b == null) return NotFound();

            b.Clicks += 1;
            await _db.SaveChangesAsync();

            return Redirect(b.Url);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var b = await _db.Bookmarks.FindAsync(id);
            if (b == null) return NotFound();

            b.IsFavorite = !b.IsFavorite;
            await _db.SaveChangesAsync();
            TempData["Toast"] = b.IsFavorite ? "Marked as favorite." : "Removed from favorites.";
            return RedirectToAction(nameof(Index), new
            {
                // keep user filters stable when toggling
                q = Request.Query["q"].ToString(),
                tag = Request.Query["tag"].ToString(),
                favorites = Request.Query["favorites"].ToString(),
                sort = Request.Query["sort"].ToString()
            });
        }

        private async Task ApplyTags(Bookmark b, string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return;

            var names = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .Select(x => x.ToLowerInvariant())
                           .Distinct()
                           .ToList();

            var existing = await _db.Tags.Where(t => names.Contains(t.Name)).ToListAsync();
            var missing = names.Except(existing.Select(t => t.Name))
                               .Select(n => new Tag { Name = n });

            foreach (var tag in missing)
                _db.Tags.Add(tag);

            await _db.SaveChangesAsync();

            var tags = await _db.Tags.Where(t => names.Contains(t.Name)).ToListAsync();
            foreach (var t in tags)
                b.BookmarkTags.Add(new BookmarkTag { Bookmark = b, Tag = t });
        }
    }

    // simple edit VM so the form can accept comma-separated tags
    public class BookmarkEditVm
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(160)]
        public string Title { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.Url, System.ComponentModel.DataAnnotations.StringLength(2048)]
        public string Url { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(2000)]
        public string? Notes { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Favorite")]
        public bool IsFavorite { get; set; }

        public string? TagsCsv { get; set; }
    }
}
