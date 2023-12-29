using Microsoft.EntityFrameworkCore;

namespace Overtime;

public class OvertimeDbContext : DbContext
{
    public DbSet<WorkedOvertime> WorkedOvertimes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OvertimeDb.db3");
        string dbPath = "C:/Source/Overtime/OvertimeDb.db3";
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
}
