using FitnessLeaderBoard.Data;
using FitnessLeaderBoard.Data.EntityClasses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessLeaderBoard.Services
{
    public class StepDataService
    {
        private ApplicationDbContext context { get; set; }

        public StepDataService(ApplicationDbContext _context)
        {
            context = _context;
        }

        public bool HasUserEnteredStepForDate(string userId, DateTime date)
        {
            return context.StepData.Any(sd => sd.UserId == userId
                && sd.Date.Date == date.Date);
        }

        public async Task<bool> AddUserStepDataForDate(string userId, DateTime date, int stepCount)
        {
            if (HasUserEnteredStepForDate(userId, date) == true)
                return false;

            // Add the user's step count for the day
            try
            {
                context.Add(new StepData
                {
                    UserId = userId,
                    Date = date,
                    StepCount = stepCount
                });

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }

            // Now computer the user's leaderboard info

            // Get the user's name to display
            var nameToDisplay
                = context.Users.Where(u => u.Id == userId)
                .Select(u =>
                // pick display name first, if exists
                u.DisplayName.Length > 0 
                ? u.DisplayName
                // else pick full name, if it exists
                : u.FullName.Length > 0 
                ? u.FullName
                // else display email address
                : u.Email)
                .FirstOrDefault();

            // Count of last 7 days of steps
            var last7daysStepCount
                = context.StepData.Where(sd => sd.UserId == userId
                && sd.Date >= DateTime.Today.AddDays(-7).Date
                && sd.Date <= DateTime.Today.Date)
                .Sum(sd => sd.StepCount);

            // Count of the last 30 days of steps
            var last30daysStepCount
                = context.StepData.Where(sd => sd.UserId == userId
                && sd.Date >= DateTime.Today.AddDays(-30).Date
                && sd.Date <= DateTime.Today.Date)
                .Sum(sd => sd.StepCount);

            var userExistsInLeaderboard
                = context.Leaderboard.Any(lb => lb.UserId == userId);

            try
            {
                if (userExistsInLeaderboard == false)
                {
                    // Add the new user
                    context.Add(new LeaderboardData
                    {
                        UserId = userId,
                        NameToDisplay = nameToDisplay,
                        DailyStepCount = stepCount,
                        LastSevenDaysStepCount = last7daysStepCount,
                        LastThirtyDaysStepCount = last7daysStepCount
                    });
                }
                else
                {
                    // Update the user's step data in the leaderboard
                    var userData
                        = context.Leaderboard.Where(lb => lb.UserId == userId)
                        .FirstOrDefault();

                    userData.NameToDisplay = nameToDisplay;
                    userData.DailyStepCount = stepCount;
                    userData.LastSevenDaysStepCount = last7daysStepCount;
                    userData.LastThirtyDaysStepCount = last30daysStepCount;
                }

                return await context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IQueryable<LeaderboardData> GetLeaderboard(int? quantity = null)
        {
            if (quantity.HasValue == false)
                // Return all results
                return context.Leaderboard
                    .AsNoTracking();
            else
                // Only return the specified quantity
                return context.Leaderboard
                    .Take(quantity.Value)
                    .AsNoTracking();
        }
    }
}
