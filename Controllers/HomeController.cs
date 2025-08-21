using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHabitBookmarkApp.Data;

namespace TaskHabitBookmarkApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;

            // show open tasks count (more intuitive than "due today")
            var openCount = await _db.Tasks.CountAsync(t => !t.IsCompleted);

            var activeHabits = await _db.Habits.OrderByDescending(h => h.Streak).Take(5).ToListAsync();

            var topTags = await _db.BookmarkTags
                .GroupBy(bt => bt.Tag.Name)
                .Select(g => new { Tag = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var recentBookmarks = await _db.Bookmarks
                .OrderByDescending(b => b.CreatedAt).Take(6).ToListAsync();

            // Completions series: prefer CompletedAt; if it's null, fall back to CreatedAt so the chart isn't empty.
            var from = today.AddDays(-13);
            var completed = await _db.Tasks
                .Where(t => t.IsCompleted && ((t.CompletedAt ?? t.CreatedAt) >= from))
                .Select(t => new { When = (t.CompletedAt ?? t.CreatedAt) })
                .ToListAsync();

            var series = Enumerable.Range(0, 14).Select(i =>
            {
                var d = from.AddDays(i).Date;
                var count = completed.Count(t => t.When.Date == d);
                return new { date = d.ToString("yyyy-MM-dd"), count };
            }).ToList();

            ViewBag.OpenCount = openCount;
            ViewBag.ActiveHabits = activeHabits;
            ViewBag.TopTags = topTags;
            ViewBag.RecentBookmarks = recentBookmarks;
            ViewBag.CompletionSeries = series;

            return View();
        }

        public IActionResult Privacy() => View();

        [Route("Home/StatusCode/{code:int}")]
        public new IActionResult StatusCode(int code)
        {
            if (code == 404) return View("NotFound");
            ViewData["Code"] = code;
            return View("StatusCode");
        }
    }
}
