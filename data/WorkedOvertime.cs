namespace Overtime;
public record WorkedOvertime
{
    public int Id { get; init; }
    public double Hours { get; init; }
    public DateTime Date { get; init; }

    public WorkedOvertime(int id, double hours, DateTime date)
    {
        Id = id;
        Hours = hours;
        Date = date;
    }

    public WorkedOvertime(double hours, DateTime date)
    {
        Hours = hours;
        Date = date;
    }
}
