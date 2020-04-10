using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessLeaderBoard.Data.EntityClasses
{
    public class StepData
    {
        public int StepDataId { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public int StepCount { get; set; }
    }
}
