using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessLeaderBoard.Data.EntityClasses
{
    public class LeaderboardData
    {
        public int LeaderboardId { get; set; }
        public string UserId { get; set; }
        public string NameToDisplay { get; set; }
        public int DailyStepCount { get; set; }
        public int LastSevenDaysStepCount { get; set; }
        public int LastThirtyDaysStepCount { get; set; }
    }
}
