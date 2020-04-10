using System;
using System.Collections.Generic;
using System.Text;
using FitnessLeaderBoard.Data.EntityClasses;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessLeaderBoard.Data
{
    public class ApplicationDbContext : IdentityDbContext<FlbUser>
    {
        public IList<StepData> StepData { get; set; }

        public IList<LeaderboardData> Leaderboard { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FlbUser>().Property(fu => fu.FullName)
                .HasMaxLength(256);
            builder.Entity<FlbUser>().Property(fu => fu.DisplayName)
                .HasMaxLength(256);

            builder.Entity<StepData>().HasKey(sd => sd.StepDataId);
            builder.Entity<StepData>().ToTable("StepData", "flb");
            builder.Entity<StepData>().Property(sd => sd.StepDataId)
                .UseIdentityColumn();
            builder.Entity<StepData>().Property(bc => bc.UserId)
                .IsRequired()
                .HasMaxLength(450);
            builder.Entity<StepData>().Property(sd => sd.Date)
                .IsRequired();
            builder.Entity<StepData>().Property(sd => sd.StepCount)
                .IsRequired();

            builder.Entity<LeaderboardData>().HasKey(ld => ld.LeaderboardId);
            builder.Entity<LeaderboardData>().ToTable("Leaderboard", "flb");
            builder.Entity<LeaderboardData>().Property(ld => ld.LeaderboardId)
                .UseIdentityColumn();
            builder.Entity<LeaderboardData>().Property(ld => ld.UserId)
                .IsRequired()
                .HasMaxLength(450);
            builder.Entity<LeaderboardData>().Property(ld => ld.NameToDisplay)
                .IsRequired()
                .HasMaxLength(256);
            builder.Entity<LeaderboardData>().Property(ld => ld.DailyStepCount)
                .IsRequired();
            builder.Entity<LeaderboardData>().Property(ld => ld.LastSevenDaysStepCount)
                .IsRequired();
            builder.Entity<LeaderboardData>().Property(ld => ld.LastThirtyDaysStepCount)
                .IsRequired();

            SeedDevelopmentTestData(builder);
        }

        private void SeedDevelopmentTestData(ModelBuilder builder)
        {
#if DEBUG

#endif
        }
    }
}
