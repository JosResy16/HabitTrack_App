

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HabitTracker.Infrastructure.Context
{
    public class HabitTrackDbContextFactory
    : IDesignTimeDbContextFactory<HabitTrackDBContext>
    {
        public HabitTrackDBContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets("e33c99e9-1684-4db0-a2d2-baf1be4a8885")
            .Build();

            var connectionString = configuration.GetConnectionString("UserDatabase");

            var options = new DbContextOptionsBuilder<HabitTrackDBContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new HabitTrackDBContext(options);
        }
    }
}
