using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHabitBookmarkApp.Data;
using TaskHabitBookmarkApp.Models;
using TaskHabitBookmarkApp.Services;

namespace TaskHabitBookmarkApp.Controllers
{
    public class HabitsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly StreakService _streaks;

        public HabitsController(AppDbContext db, StreakService streaks)
        {
            _db = db;
            _streaks = streaks;
        }

        public async Task<IActionResult> Index()
        {
            var habits = await _db.Habits.OrderByDescending(h => h.Streak).ToListAsync();
            return View(habits);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Habit habit)
        {
            if (!ModelState.IsValid) return View(habit);
            _db.Habits.Add(habit);
            await _db.SaveChangesAsync();
            TempData["Toast"] = "Habit created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var habit = await _db.Habits.FindAsync(id);
            if (habit == null) return NotFound();
            return View(habit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Habit habit)
        {
            if (id != habit.Id) return BadRequest();
            if (!ModelState.IsValid) return View(habit);

            var existing = await _db.Habits.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = habit.Name;
            existing.Description = habit.Description;
            existing.Frequency = habit.Frequency;

            await _db.SaveChangesAsync();
            TempData["Toast"] = "Habit updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var habit = await _db.Habits.FindAsync(id);
            if (habit == null) return NotFound();
            return View(habit);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var habit = await _db.Habits.FindAsync(id);
            if (habit == null) return NotFound();
            return View(habit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var habit = await _db.Habits.FindAsync(id);
            if (habit == null) return NotFound();
            _db.Habits.Remove(habit);
            await _db.SaveChangesAsync();
            TempData["Toast"] = "Habit deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(int id)
        {
            var habit = await _db.Habits.FindAsync(id);
            if (habit == null) return NotFound();

            var changed = _streaks.CheckIn(habit);
            if (changed)
            {
                await _db.SaveChangesAsync();
                TempData["Toast"] = "Checked in. Keep it going!";
            }
            else
            {
                TempData["Toast"] = "Already checked in for this period.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
