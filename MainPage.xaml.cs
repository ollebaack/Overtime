using System.Collections.ObjectModel;

namespace Overtime;

public partial class MainPage : ContentPage
{
    private readonly OvertimeService _overtimeService;

	// Add an ObservableCollection to hold overtime items
	private readonly ObservableCollection<WorkedOvertime> overtimeItems = [];

	// Add a property to store the total sum
    private double totalSum;
    public double TotalSum
    {
        get { return totalSum; }
        set
        {
            if (totalSum != value)
            {
                totalSum = value;
                OnPropertyChanged(nameof(TotalSum));
            }
        }
    }

    public MainPage()
    {
        InitializeComponent();

        _overtimeService = new OvertimeService();

		// Set the ItemsSource of the overtimeListView to your ObservableCollection
    	overtimeListView.ItemsSource = overtimeItems;
    	LoadWorkedOvertimes();

		// Set the BindingContext to enable data binding
    	BindingContext = this;
	}

	private void LoadWorkedOvertimes()
	{
		foreach (var item in _overtimeService.GetAllWorkedOvertimes())
		{
			overtimeItems.Add(item);
		}

		// Calculate the total sum and update the property
		TotalSum = overtimeItems.Sum(item => item.Hours);
	}

	private async void OnSaveClicked(object sender, EventArgs e)
	{
		if (int.TryParse(hoursEntry.Text, out int hours) && hours > 0)
		{
			WorkedOvertime newWorkedOvertime = new(hours, DateTime.Now);
			_overtimeService.AddWorkedOvertime(newWorkedOvertime);
			hoursEntry.Text = "";

			// Add the new overtime item to the ObservableCollection
			overtimeItems.Insert(0, newWorkedOvertime);

			// Calculate the total sum and update the property
			TotalSum = overtimeItems.Sum(item => item.Hours);

			// Display a success message using a Label
			successLabel.Text = $"Overtime of {hours} hours added successfully.";

			// Show the successLabel
			successLabel.IsVisible = true;

			// Fade out animation			
			await successLabel.FadeTo(0, 7000); // 2000 milliseconds (adjust as needed)

			// Hide the successLabel after the animation completes
			successLabel.IsVisible = false;
			successLabel.Opacity = 1; // Reset opacity for potential future showings
		}
		else
		{
			// Handle invalid input (not a valid positive integer)
			await DisplayAlert("Error", "Please enter a valid positive number for hours.", "OK");
		}
	}

    private void OnEntryCompleted(object sender, EventArgs e) => OnSaveClicked(sender, e);
}