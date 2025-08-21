using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHabitBookmarkApp.Data;
using TaskHabitBookmarkApp.Models;

namespace TaskHabitBookmarkApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext _db;
        public TasksController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? q, Priority? priority, string? showCompleted, string? sc)
        {
            bool showCompletedFlag = !string.IsNullOrEmpty(showCompleted)
                                     ? true
                                     : !string.IsNullOrEmpty(sc) ? false : true;

            var query = _db.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => t.Title.Contains(q) || (t.Notes ?? "").Contains(q));

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority);

            if (!showCompletedFlag)
                query = query.Where(t => !t.IsCompleted);

            query = query.OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate);

            return View(await query.ToListAsync());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem item)
        {
            if (!ModelState.IsValid) return View(item);
            _db.Tasks.Add(item);
            await _db.SaveChangesAsync();
            TempData["Toast"] = "Task created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Tasks.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem item)
        {
            if (id != item.Id) return BadRequest();
            if (!ModelState.IsValid) return View(item);

            var existing = await _db.Tasks.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = item.Title;
            existing.Notes = item.Notes;
            existing.DueDate = item.DueDate;
            existing.Priority = item.Priority;

            // keep completion timestamp sensible
            if (item.IsCompleted && !existing.IsCompleted)
            {
                existing.IsCompleted = true;
                existing.CompletedAt = DateTime.UtcNow;
            }
            else if (!item.IsCompleted && existing.IsCompleted)
            {
                existing.IsCompleted = false;
                existing.CompletedAt = null;
            }

            await _db.SaveChangesAsync();
            TempData["Toast"] = "Task updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.Tasks.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Tasks.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.Tasks.FindAsync(id);
            if (item == null) return NotFound();
            _db.Tasks.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Toast"] = "Task deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var item = await _db.Tasks.FindAsync(id);
            if (item == null) return NotFound();

            item.IsCompleted = !item.IsCompleted;
            item.CompletedAt = item.IsCompleted ? DateTime.UtcNow : null;

            await _db.SaveChangesAsync();
            TempData["Toast"] = item.IsCompleted ? "Task marked done." : "Task reopened.";
            return RedirectToAction(nameof(Index), new
            {
                // keep user filters stable when toggling
                q = Request.Query["q"].ToString(),
                priority = Request.Query["priority"].ToString(),
                showCompleted = Request.Query["showCompleted"].ToString(),
                sc = Request.Query["sc"].ToString()
            });
        }
    }
}
