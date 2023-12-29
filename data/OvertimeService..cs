using Microsoft.EntityFrameworkCore;

namespace Overtime;

public class OvertimeService
{
    private readonly OvertimeDbContext _dbContext;

    public OvertimeService()
    {
        _dbContext = new OvertimeDbContext();
        _dbContext.Database.Migrate();
    }

    public void AddWorkedOvertime(WorkedOvertime workedOvertime)
    {
        _dbContext.WorkedOvertimes.Add(workedOvertime);
        _dbContext.SaveChanges();
    }

    public List<WorkedOvertime> GetAllWorkedOvertimes()
    {
        return _dbContext.WorkedOvertimes.ToList();
    }
}
