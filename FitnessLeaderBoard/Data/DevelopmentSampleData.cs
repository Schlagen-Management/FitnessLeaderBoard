using FitnessLeaderBoard.Data.EntityClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessLeaderBoard.Data
{
    public class DevelopmentSampleData
    {
        public static void Initialize(ApplicationDbContext context)
        {
#if DEBUG
            try
            {
                if (context.Users == null || context.Users.Count() == 0)
                    return;

                var userId = context.Users.Where(u => u.Email == "littlejohnjt@hotmail.com").FirstOrDefault().Id;

                if (string.IsNullOrEmpty(userId))
                    return;

                if (context.StepData != null && context.StepData.Count() == 0)
                {
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 1),
                        StepCount = 500
                    });
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 2),
                        StepCount = 500
                    });
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 3),
                        StepCount = 500
                    });
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 4),
                        StepCount = 500
                    });
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 5),
                        StepCount = 500
                    });
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 6),
                        StepCount = 500
                    });
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 7),
                        StepCount = 500
                    });
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 9),
                        StepCount = 500
                    });
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 10),
                        StepCount = 500
                    });
                    context.StepData.Add(new StepData
                    {
                        UserId = userId,
                        Date = new DateTime(2020, 4, 11),
                        StepCount = 500
                    });
                }

                if (context.Leaderboard != null && context.Leaderboard.Count() == 0)
                {
                    context.Leaderboard.Add(new LeaderboardData
                    {
                        UserId = userId,
                        NameToDisplay = "The Man",
                        DailyStepCount = 500,
                        LastSevenDaysStepCount = 3000,
                        LastThirtyDaysStepCount = 5000
                    });
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Add error logging
            }
#endif
        }
    }
}
