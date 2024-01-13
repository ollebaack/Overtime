using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Overtime;

public class OvertimeDbContext : DbContext
{
    public DbSet<WorkedOvertime> WorkedOvertimes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OvertimeDb.db3");

        // Check if the database file exists
        if (!File.Exists(dbPath))
        {
            var directoryPath = Path.GetDirectoryName(dbPath);
            if (directoryPath != null)
            {
                // If not, create the necessary directories and the empty database file
                Directory.CreateDirectory(directoryPath);
                File.Create(dbPath).Close();
            }
        }

        optionsBuilder.UseSqlite($"Data Source={dbPath}")
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug()));
    }
}
