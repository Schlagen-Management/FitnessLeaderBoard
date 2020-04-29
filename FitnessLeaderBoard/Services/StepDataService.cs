using FitnessLeaderBoard.Data;
using FitnessLeaderBoard.Data.EntityClasses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessLeaderBoard.Services
{
    public class StepDataService
    {
        public enum StepCountType { SevenDayStepCount, ThirtyDayStepCount, AllTimeStepCount }

        private ApplicationDbContext context { get; set; }

        private UserManager<FlbUser> userManager { get; set; }

        public StepDataService(ApplicationDbContext _context,
            UserManager<FlbUser> _userManager)
        {
            context = _context;
            userManager = _userManager;
        }

        public bool HasUserEnteredStepForDate(string userId, DateTime date)
        {
            return context.StepData.Any(sd => sd.UserId == userId
                && sd.Date.Date == date.Date);
        }

        public async Task<string> AddUserStepDataForDate(string userId, DateTime date, int stepCount)
        {
            var results = string.Empty;

            if (HasUserEnteredStepForDate(userId, date)) 
                // The user has already entered steps for the day, note the error
                return string.Format("You have already entered steps for {0}", date.ToString("dddd MMM d"));

            // Set up the transaction
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

                    // Save these changes
                    results
                        = context.SaveChanges() > 0 ? string.Empty : "Error saving step data";

                    // Now compute the user's leaderboard info

                    // Get the user's information
                    var user = await userManager.FindByIdAsync(userId);
                    var nameToDisplay
                        = GetNameToDisplay(user);
                    var initials
                        = ConvertNameToInitials(nameToDisplay);
                    var imageLink
                        = user.ImageLink;

                    // Count of last 7 days of steps
                    var last7daysStepCount
                        = context.StepData.Where(sd => sd.UserId == userId
                        && sd.Date >= DateTime.Today.AddDays(-7).Date
                        && sd.Date <= DateTime.Today.Date)
                        .Sum(sd => sd.StepCount);

                    // Get the all-time step count
                    var allTimeStepCount
                        = context.StepData.Where(sd => sd.UserId == userId)
                        .Sum(sd => sd.StepCount);

                    var userExistsInLeaderboard
                        = context.Leaderboard.Any(lb => lb.UserId == userId);
                    if (userExistsInLeaderboard == false)
                    {
                        // The user does not exist in the leaderboard, add the new user
                        context.Add(new LeaderboardData
                        {
                            UserId = userId,
                            NameToDisplay = nameToDisplay,
                            ImageLink = imageLink,
                            Initials = initials,
                            DailyStepCount = stepCount,
                            LastSevenDaysStepCount = last7daysStepCount,
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
                        userData.ImageLink = imageLink;
                        userData.Initials = initials;
                        userData.DailyStepCount = stepCount;
                        userData.LastSevenDaysStepCount = last7daysStepCount;
                        userData.AllTimeStepCount = allTimeStepCount;
                    }

                    // Save the leaderboard changes
                    results
                        = await context.SaveChangesAsync() > 0 ? string.Empty : "Error saving leaderboard update";

                    // And commit the transaction
                    (await transaction).Commit();
                }
                catch (Exception ex)
                {
                    /// TODO: Handle failure
                    return "Unknown error";
                }
            }

            return results;
        }

        public IQueryable<LeaderboardData> GetLeaderboard()
        {
            // Return all results
            return context.Leaderboard.AsNoTracking();
        }

        public async Task<int> GetUsersRank(string userId, StepCountType stepCountType)
        {
            int results = 0;

            switch (stepCountType)
            {
                case StepCountType.SevenDayStepCount:
                    {
                        var stepCount
                            = await context.Leaderboard
                            .AsNoTracking()
                            .Where(lb => lb.UserId == userId)
                            .Select(lb => lb.LastSevenDaysStepCount)
                            .FirstOrDefaultAsync();

                        results
                            = await context.Leaderboard
                            .AsNoTracking()
                            .CountAsync(lb => lb.LastSevenDaysStepCount > stepCount)
                            + 1;
                    }
                    break;
                case StepCountType.ThirtyDayStepCount:
                    {
                        var stepCount
                            = await context.Leaderboard
                            .AsNoTracking()
                            .Where(lb => lb.UserId == userId)
                            .Select(lb => lb.LastThirtyDaysStepCount)
                            .FirstOrDefaultAsync();

                        results
                            = await context.Leaderboard
                            .AsNoTracking()
                            .CountAsync(lb => lb.LastThirtyDaysStepCount > stepCount)
                            + 1;
                    }
                    break;
                case StepCountType.AllTimeStepCount:
                    {
                        var stepCount
                            = await context.Leaderboard
                            .AsNoTracking()
                            .Where(lb => lb.UserId == userId)
                            .Select(lb => lb.AllTimeStepCount)
                            .FirstOrDefaultAsync();

                        results
                            = await context.Leaderboard
                            .AsNoTracking()
                            .CountAsync(lb => lb.AllTimeStepCount > stepCount)
                            + 1;
                    }
                    break;
            }

            return results;
        }

        public async Task<LeaderboardData> GetUserLeaderboardInfo(string userId)
        {
            var leaderBoardInfo
                = await context.Leaderboard
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    lb => lb.UserId == userId);

            var user
                = await userManager.FindByIdAsync(userId);

            // Get user information
            var nameToDisplay
                = GetNameToDisplay(user);
            var initials
                = ConvertNameToInitials(nameToDisplay);
            var imageLink
                = user != null ? user.ImageLink : string.Empty;

            if (leaderBoardInfo == null)
            {
                leaderBoardInfo
                    = new LeaderboardData
                    {
                        UserId = userId,
                        DailyStepCount = 0,
                        LastSevenDaysStepCount = 0,
                        LastThirtyDaysStepCount = 0,
                        AllTimeStepCount = 0
                    };
            }

            leaderBoardInfo.NameToDisplay = nameToDisplay;
            leaderBoardInfo.Initials = initials;
            leaderBoardInfo.ImageLink = imageLink;

            return leaderBoardInfo;
        }

        public async Task UpdateUserInfoInLeaderboard(FlbUser user)
        {
            var usersLeaderboardInfo
                = await context.Leaderboard
                .FirstOrDefaultAsync(lb => lb.UserId == user.Id);

            if (usersLeaderboardInfo == null)
                return;

            usersLeaderboardInfo
                .NameToDisplay
                = GetNameToDisplay(user);

            usersLeaderboardInfo
                .Initials = ConvertNameToInitials(
                    usersLeaderboardInfo.NameToDisplay);

            usersLeaderboardInfo
                .ImageLink = user.ImageLink;

            context.SaveChanges();
        }

        public string ConvertNameToInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            // Compute the initials
            List<string> allInitials
                = name.Split(" ")
                .Select(n => n.Substring(0, 1))
                .ToList();
            string initials = string.Empty;
            for (var index = 0; index < allInitials.Count(); index++)
            {
                // Get the first initial
                if (index == 0)
                    initials += allInitials[index];

                // and get the last initial, if it exists
                if (index > 0 && index == allInitials.Count() - 1)
                    initials += allInitials[index];
            }

            return initials;
        }

        public string GetNameToDisplay(FlbUser user)
        {
            if (user == null) return string.Empty;

            // Display name precedence: DisplayName -> Full Name -> email address
            var nameToDisplay
                = !string.IsNullOrEmpty(user.DisplayName)
                ? user.DisplayName
                : !string.IsNullOrEmpty(user.FullName)
                ? user.FullName
                : user.Email;

            return nameToDisplay;
        }
    }
}
