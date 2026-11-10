using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace Overtime;

// One year in the main list. Collapsing it empties the group, which hides its entries.
public class YearGroup : ObservableCollection<WorkedOvertime>
{
    private readonly List<WorkedOvertime> _entries;

    public YearGroup(int year, IEnumerable<WorkedOvertime> entries, bool isExpanded)
    {
        Year = year;
        _entries = entries.ToList();
        IsExpanded = isExpanded;
        ShowEntries();
    }

    public int Year { get; }

    public bool IsExpanded { get; private set; }

    public double Sum => _entries.Where(entry => !entry.Deleted).Sum(entry => entry.Hours);

    public string Title => $"{(IsExpanded ? "▾" : "▸")}  {Year}";

    public string Summary
        => $"{Sum.ToString("+0.##;-0.##;0", CultureInfo.CurrentCulture)} h · {_entries.Count} {(_entries.Count == 1 ? "entry" : "entries")}";

    public void Toggle()
    {
        IsExpanded = !IsExpanded;
        ShowEntries();
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Title)));
    }

    private void ShowEntries()
    {
        Clear();
        if (IsExpanded)
        {
            foreach (var entry in _entries)
            {
                Add(entry);
            }
        }
    }
}
