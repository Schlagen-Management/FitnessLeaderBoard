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
        public enum StepCountType { SevenDayStepCount, ThirtyDayStepCount, AllTimeStepCount }

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
            bool results = true;

            using (var transaction = context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Add step data
                    context.Add(new StepData
                    {
                        UserId = userId,
                        Date = date,
                        StepCount = stepCount
                    });

                    // Save changes
                    results
                        = context.SaveChanges() > 0;

                    // Now compute the user's leaderboard info

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

                    // Compute the initials
                    var initials
                        = ConvertNameToInitials(nameToDisplay);

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

                    var allTimeStepCount
                        = context.StepData.Where(sd => sd.UserId == userId)
                        .Sum(sd => sd.StepCount);

                    var userExistsInLeaderboard
                        = context.Leaderboard.Any(lb => lb.UserId == userId);

                    if (userExistsInLeaderboard == false)
                    {
                        // Add the new user
                        context.Add(new LeaderboardData
                        {
                            UserId = userId,
                            NameToDisplay = nameToDisplay,
                            Initials = initials,
                            DailyStepCount = stepCount,
                            LastSevenDaysStepCount = last7daysStepCount,
                            LastThirtyDaysStepCount = last7daysStepCount,
                            AllTimeStepCount = allTimeStepCount
                        });
                    }
                    else
                    {
                        // Update the user's step data in the leaderboard
                        var userData
                            = context.Leaderboard.Where(lb => lb.UserId == userId)
                            .FirstOrDefault();

                        userData.NameToDisplay = nameToDisplay;
                        userData.Initials = initials;
                        userData.DailyStepCount = stepCount;
                        userData.LastSevenDaysStepCount = last7daysStepCount;
                        userData.LastThirtyDaysStepCount = last30daysStepCount;
                        userData.AllTimeStepCount = allTimeStepCount;
                    }

                    results
                        = await context.SaveChangesAsync() > 0;

                    (await transaction).Commit();
                }
                catch (Exception ex)
                {
                    /// TODO: Handle failure
                    return false;
                }
            }

            return results;
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

        public async Task<int> GetUsersRank(string userId, StepCountType stepCountType)
        {
            int results = 0;

            switch (stepCountType)
            {
                case StepCountType.SevenDayStepCount:
                    {
                        var stepCount
                            = await context.Leaderboard.Where(lb => lb.UserId == userId)
                            .Select(lb => lb.LastSevenDaysStepCount)
                            .FirstOrDefaultAsync();

                        results
                            = await context.Leaderboard
                            .CountAsync(lb => lb.LastSevenDaysStepCount > stepCount)
                            + 1;
                    }
                    break;
                case StepCountType.ThirtyDayStepCount:
                    {
                        var stepCount
                            = await context.Leaderboard.Where(lb => lb.UserId == userId)
                            .Select(lb => lb.LastThirtyDaysStepCount)
                            .FirstOrDefaultAsync();

                        results
                            = await context.Leaderboard
                            .CountAsync(lb => lb.LastThirtyDaysStepCount > stepCount)
                            + 1;
                    }
                    break;
                case StepCountType.AllTimeStepCount:
                    {
                        var stepCount
                            = await context.Leaderboard.Where(lb => lb.UserId == userId)
                            .Select(lb => lb.AllTimeStepCount)
                            .FirstOrDefaultAsync();

                        results
                            = await context.Leaderboard
                            .CountAsync(lb => lb.AllTimeStepCount > stepCount)
                            + 1;
                    }
                    break;
            }

            return results;
        }

        public async Task<LeaderboardData> GetUserLeaderboardInfo(string userId)
        {
            return await context.Leaderboard.FirstOrDefaultAsync(
                lb => lb.UserId == userId);
        }

        public string ConvertNameToInitials(string name)
        {

            // Compute the initials
            List<string> allInitials
                = name.Split(" ").Select(n => n.Substring(0, 1)).ToList();
            string initials = string.Empty;
            for (var index = 0; index < allInitials.Count(); index++)
            {
                if (index == 0)
                    initials += allInitials[index];

                if (index > 0 && index == allInitials.Count() - 1)
                    initials += allInitials[index];
            }

            return initials;
        }
    }
}
