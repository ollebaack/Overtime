using System.Globalization;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Overtime;

public partial class StatisticsPage : ContentPage
{
    private readonly OvertimeService _overtimeService = new();

    public StatisticsPage()
    {
        InitializeComponent();
    }

    // Recalculated every time the tab is opened, so new entries show up.
    protected override void OnAppearing()
    {
        base.OnAppearing();

        var stats = new OvertimeStatistics(_overtimeService.GetAll(), DateTime.Today);
        var colors = ChartColors.For(Application.Current?.RequestedTheme ?? AppTheme.Light);

        ShowFigures(stats);
        ShowBalanceChart(stats, colors);
        ShowMonthChart(stats, colors);
    }

    private void ShowFigures(OvertimeStatistics stats)
    {
        balanceLabel.Text = $"{FormatHours(stats.Balance)} h";
        thisMonthLabel.Text = $"{FormatSigned(stats.ThisMonth)} h";
        lastMonthLabel.Text = $"{FormatSigned(stats.LastMonth)} h";
        averageCaption.Text = stats.Months.Count > 0
            ? $"Average per month since {stats.Months[0].Month.ToString("MMM yyyy", CultureInfo.CurrentCulture)}"
            : "Average per month";
        averageLabel.Text = $"{FormatSigned(stats.AveragePerMonth)} h";
        recentAverageLabel.Text = $"{FormatSigned(stats.RecentAveragePerMonth)} h";
    }

    // The balance only changes when hours are added, so it is drawn as steps.
    private void ShowBalanceChart(OvertimeStatistics stats, ChartColors colors)
    {
        balanceChart.Series =
        [
            new StepLineSeries<DateTimePoint>
            {
                Name = "Balance",
                Values = stats.BalanceOverTime.Select(point => new DateTimePoint(point.Date, point.Balance)).ToArray(),
                Stroke = new SolidColorPaint(colors.Overtime) { StrokeThickness = 2 },
                Fill = null,
                GeometrySize = 0,
                YToolTipLabelFormatter = point => $"{FormatHours(point.Model!.Value ?? 0)} h",
            },
        ];
        balanceChart.XAxes = [StyleAxis(new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMM yy", CultureInfo.CurrentCulture)), colors, gridlines: false)];
        balanceChart.YAxes = [StyleAxis(new Axis { Labeler = value => value.ToString("0", CultureInfo.CurrentCulture) }, colors, gridlines: true)];
    }

    // Overtime and time off as bars above and below zero, with a 3-month average on top.
    private void ShowMonthChart(OvertimeStatistics stats, ChartColors colors)
    {
        var months = stats.Months.TakeLast(12).ToList();
        var average = stats.ThreeMonthAverage.TakeLast(12).ToList();

        monthChart.Series =
        [
            MonthColumns("Overtime", months.Select(month => month.Hours > 0 ? month.Hours : (double?)null), colors.Overtime),
            MonthColumns("Time off", months.Select(month => month.Hours < 0 ? month.Hours : (double?)null), colors.TimeOff),
            new LineSeries<double>
            {
                Name = "3-month average",
                Values = average,
                Stroke = new SolidColorPaint(colors.SecondaryInk) { StrokeThickness = 2 },
                Fill = null,
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(colors.SecondaryInk) { StrokeThickness = 2 },
                GeometryFill = new SolidColorPaint(colors.Surface),
                LineSmoothness = 0,
                YToolTipLabelFormatter = point => $"{FormatSigned(point.Model)} h",
            },
        ];
        monthChart.XAxes = [StyleAxis(new Axis { Labels = months.Select(month => month.Month.ToString("MMM yy", CultureInfo.CurrentCulture)).ToArray() }, colors, gridlines: false)];
        monthChart.YAxes = [StyleAxis(new Axis { Labeler = value => value.ToString("0", CultureInfo.CurrentCulture) }, colors, gridlines: true)];
        monthChart.LegendTextPaint = new SolidColorPaint(colors.SecondaryInk);
    }

    private static ColumnSeries<double?> MonthColumns(string name, IEnumerable<double?> values, SKColor color) => new()
    {
        Name = name,
        Values = values.ToArray(),
        Fill = new SolidColorPaint(color),
        Rx = 4,
        Ry = 4,
        MaxBarWidth = 32,
        // Both series share each month's slot; a month is either overtime or time off.
        IgnoresBarPosition = true,
        YToolTipLabelFormatter = point => $"{FormatSigned(point.Model ?? 0)} h",
    };

    private static T StyleAxis<T>(T axis, ChartColors colors, bool gridlines) where T : Axis
    {
        axis.TextSize = 12;
        axis.LabelsPaint = new SolidColorPaint(colors.Muted);
        axis.SeparatorsPaint = gridlines ? new SolidColorPaint(colors.Gridline) { StrokeThickness = 1 } : null;
        return axis;
    }

    private static string FormatHours(double hours) => hours.ToString("0.##", CultureInfo.CurrentCulture);

    private static string FormatSigned(double hours) => (hours > 0 ? "+" : "") + FormatHours(hours);

    // Blue/red pair and chart ink, checked for contrast and colour-blind safety in light and dark mode.
    private record ChartColors(SKColor Overtime, SKColor TimeOff, SKColor SecondaryInk, SKColor Muted, SKColor Gridline, SKColor Surface)
    {
        public static ChartColors For(AppTheme theme) => theme == AppTheme.Dark
            ? new(SKColor.Parse("#3987e5"), SKColor.Parse("#e66767"), SKColor.Parse("#c3c2b7"), SKColor.Parse("#898781"), SKColor.Parse("#2c2c2a"), SKColor.Parse("#1a1a19"))
            : new(SKColor.Parse("#2a78d6"), SKColor.Parse("#e34948"), SKColor.Parse("#52514e"), SKColor.Parse("#898781"), SKColor.Parse("#e1e0d9"), SKColor.Parse("#fcfcfb"));
    }
}
