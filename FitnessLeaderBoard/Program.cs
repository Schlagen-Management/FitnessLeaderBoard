using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FitnessLeaderBoard.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace FitnessLeaderBoard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext
                    = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Ensure the DB is created
                dbContext.Database.EnsureCreated();

                // Perform any migrations
                if (dbContext.Database.GetPendingMigrations().Any())
                    dbContext.Database.Migrate();

#if DEBUG
                // Seed the development database with sample data
                DevelopmentSampleData.Initialize(dbContext);
#endif
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
