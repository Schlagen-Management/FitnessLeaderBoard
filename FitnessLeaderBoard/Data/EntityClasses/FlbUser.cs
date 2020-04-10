using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessLeaderBoard.Data.EntityClasses
{
    public class FlbUser : IdentityUser
    {
        public string FullName { get; set; }
        public string DisplayName { get; set; }
    }
}
