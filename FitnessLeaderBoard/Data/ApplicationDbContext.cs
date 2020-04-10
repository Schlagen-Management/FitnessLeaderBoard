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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StepData>().HasKey(sd => sd.StepDataId);
            builder.Entity<StepData>().ToTable("StepData", "flb");
            builder.Entity<StepData>().Property(sd => sd.StepDataId)
                .UseIdentityColumn();
            builder.Entity<StepData>().Property(bc => bc.UserId)
                .IsRequired()
                .HasColumnType("nvarchar")
                .HasMaxLength(450);
            builder.Entity<StepData>().Property(sd => sd.Date)
                .IsRequired();
            builder.Entity<StepData>().Property(sd => sd.StepCount)
                .IsRequired();
        }
    }
}
