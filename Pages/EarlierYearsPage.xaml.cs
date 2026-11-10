using System.Collections.ObjectModel;

namespace Overtime;

public partial class EarlierYearsPage : ContentPage
{
    private readonly OvertimeService _overtimeService = new();
    private readonly ObservableCollection<YearGroup> _years = [];

    // Years start closed; the ones the user opens stay open while the app runs.
    private readonly HashSet<int> _openYears = [];

    public EarlierYearsPage()
    {
        InitializeComponent();
        yearsView.ItemsSource = _years;
    }

    // Reloaded every time the tab is opened. Deleted entries are left out.
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _years.Clear();
        var earlier = _overtimeService.GetAll()
            .Where(item => !item.Deleted && item.Date.Year < DateTime.Today.Year)
            .GroupBy(item => item.Date.Year)
            .OrderByDescending(year => year.Key);

        foreach (var year in earlier)
        {
            _years.Add(new YearGroup(year.Key, year, _openYears.Contains(year.Key)));
        }

        emptyLabel.IsVisible = _years.Count == 0;
    }

    private void OnYearTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not YearGroup year)
        {
            return;
        }

        year.Toggle();
        if (year.IsExpanded)
        {
            _openYears.Add(year.Year);
        }
        else
        {
            _openYears.Remove(year.Year);
        }
    }
}
