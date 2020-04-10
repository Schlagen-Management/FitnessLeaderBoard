using FitnessLeaderBoard.Data;
using FitnessLeaderBoard.Data.EntityClasses;
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

        public IList<AggregrateStepData> GetDailyStepData(DateTime date)
        {
            var todaysSteps
                = from steps in context.StepData
                  join user in context.Users on steps.UserId equals user.Id
                  where steps.Date.Date == date.Date
                  select new
                  {
                      UserId = steps.UserId,
                      DisplayName = user.DisplayName,
                      StepCount = steps.StepCount
                  };

            var results
                = from steps in todaysSteps
                  group steps by steps.UserId into usersSteps
                  orderby usersSteps.Sum(us => us.StepCount) descending
                  select new AggregrateStepData
                  {
                      UserId = usersSteps.Key,
                      DisplayName = usersSteps.First().DisplayName,
                      StepCount = usersSteps.Sum(us => us.StepCount)
                  };

            return results.ToList();
        }

        public IList<AggregrateStepData> GetStepDataForTimePeriod(
            DateTime startDate, DateTime endDate)
        {
            var todaysSteps
                = from steps in context.StepData
                  join user in context.Users on steps.UserId equals user.Id
                  where steps.Date.Date >= startDate.Date 
                  && steps.Date.Date <= endDate.Date
                  select new
                  {
                      UserId = steps.UserId,
                      DisplayName = user.DisplayName,
                      StepCount = steps.StepCount
                  };

            var results
                = from steps in todaysSteps
                  group steps by steps.UserId into usersSteps
                  orderby usersSteps.Sum(us => us.StepCount) descending
                  select new AggregrateStepData
                  {
                      UserId = usersSteps.Key,
                      DisplayName = usersSteps.First().DisplayName,
                      StepCount = usersSteps.Sum(us => us.StepCount)
                  };

            return results.ToList();
        }

        public IList<AggregrateStepData> GetAllTimeStepData()
        {
            var todaysSteps
                = from steps in context.StepData
                  join user in context.Users on steps.UserId equals user.Id
                  select new
                  {
                      UserId = steps.UserId,
                      DisplayName = user.DisplayName,
                      StepCount = steps.StepCount
                  };

            var results
                = from steps in todaysSteps
                  group steps by steps.UserId into usersSteps
                  orderby usersSteps.Sum(us => us.StepCount) descending
                  select new AggregrateStepData
                  {
                      UserId = usersSteps.Key,
                      DisplayName = usersSteps.First().DisplayName,
                      StepCount = usersSteps.Sum(us => us.StepCount)
                  };

            return results.ToList();
        }

        public bool HasUserEnteredStepForDate(string userId, DateTime date)
        {
            return context.StepData.Any(sd => sd.UserId == userId
                && sd.Date.Date == date.Date);
        }
    }
}
