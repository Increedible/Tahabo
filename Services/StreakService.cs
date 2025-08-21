using System.Globalization;
using TaskHabitBookmarkApp.Models;

namespace TaskHabitBookmarkApp.Services
{
    public class StreakService
    {
        private static DateTime TodayUtcDateOnly() => DateTime.UtcNow.Date;

        public bool CheckIn(Habit habit, DateTime? asOf = null)
        {
            var today = (asOf ?? TodayUtcDateOnly()).Date;

            if (habit.Frequency == Frequency.Daily)
                return CheckInDaily(habit, today);

            return CheckInWeekly(habit, today);
        }

        private static bool CheckInDaily(Habit habit, DateTime today)
        {
            if (habit.LastDoneDate.HasValue && habit.LastDoneDate.Value.Date == today)
                return false; // already done today

            if (habit.LastDoneDate.HasValue && habit.LastDoneDate.Value.Date == today.AddDays(-1))
                habit.Streak += 1;
            else
                habit.Streak = 1;

            habit.BestStreak = Math.Max(habit.BestStreak, habit.Streak);
            habit.LastDoneDate = today;
            return true;
        }

        private static bool CheckInWeekly(Habit habit, DateTime today)
        {
            var (start, end) = WeekRange(today);

            if (habit.LastDoneDate.HasValue)
            {
                var last = habit.LastDoneDate.Value.Date;
                var (prevStart, prevEnd) = WeekRange(today.AddDays(-7));

                if (last >= start && last <= end)
                    return false; // already checked in this week

                if (last >= prevStart && last <= prevEnd)
                    habit.Streak += 1;
                else
                    habit.Streak = 1;
            }
            else
            {
                habit.Streak = 1;
            }

            habit.BestStreak = Math.Max(habit.BestStreak, habit.Streak);
            habit.LastDoneDate = today;
            return true;
        }

        private static (DateTime start, DateTime end) WeekRange(DateTime any)
        {
            // ISO week: Monday start
            int diff = (7 + (any.DayOfWeek - DayOfWeek.Monday)) % 7;
            var start = any.AddDays(-diff).Date;
            var end = start.AddDays(6).Date;
            return (start, end);
        }
    }
}
