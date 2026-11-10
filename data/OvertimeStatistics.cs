namespace Overtime;

public record MonthTotal(DateTime Month, double Hours);

public record BalancePoint(DateTime Date, double Balance);

// Figures and series for the Statistics page and "ot stats". Deleted entries are ignored.
public class OvertimeStatistics
{
    public OvertimeStatistics(IEnumerable<WorkedOvertime> entries, DateTime today)
    {
        var items = entries
            .Where(item => !item.Deleted)
            .OrderBy(item => item.Date)
            .ThenBy(item => item.Id)
            .ToList();

        Balance = items.Sum(item => item.Hours);

        // Running balance at the end of each day that has entries.
        var running = 0.0;
        BalanceOverTime = items
            .GroupBy(item => item.Date.Date)
            .Select(day => new BalancePoint(day.Key, running += day.Sum(item => item.Hours)))
            .ToList();

        // Every month from the first entry to this month, including months without entries.
        var thisMonth = new DateTime(today.Year, today.Month, 1);
        var byMonth = items
            .GroupBy(item => new DateTime(item.Date.Year, item.Date.Month, 1))
            .ToDictionary(month => month.Key, month => month.Sum(item => item.Hours));
        var firstMonth = byMonth.Count > 0 ? byMonth.Keys.Min() : thisMonth;

        var months = new List<MonthTotal>();
        for (var month = firstMonth; month <= thisMonth; month = month.AddMonths(1))
        {
            months.Add(new MonthTotal(month, byMonth.GetValueOrDefault(month)));
        }

        Months = months;
        ThreeMonthAverage = months
            .Select((_, i) => months.Skip(Math.Max(0, i - 2)).Take(Math.Min(3, i + 1)).Average(month => month.Hours))
            .ToList();
        ThisMonth = byMonth.GetValueOrDefault(thisMonth);
        LastMonth = byMonth.GetValueOrDefault(thisMonth.AddMonths(-1));

        // Averages use whole months only, so a half-finished month doesn't drag them down.
        var completeMonths = months.Where(month => month.Month < thisMonth).ToList();
        AveragePerMonth = completeMonths.Count > 0 ? completeMonths.Average(month => month.Hours) : 0;
        RecentAveragePerMonth = completeMonths.Count > 0 ? completeMonths.TakeLast(6).Average(month => month.Hours) : 0;
    }

    public double Balance { get; }

    public double ThisMonth { get; }

    public double LastMonth { get; }

    // Over all whole months since the first entry.
    public double AveragePerMonth { get; }

    // Over the last 6 whole months.
    public double RecentAveragePerMonth { get; }

    public IReadOnlyList<BalancePoint> BalanceOverTime { get; }

    public IReadOnlyList<MonthTotal> Months { get; }

    // Per month: the average of that month and the two before it, which smooths out single busy months.
    public IReadOnlyList<double> ThreeMonthAverage { get; }
}
