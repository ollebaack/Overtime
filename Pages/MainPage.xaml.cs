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

	private async void OnSaveClickedAsync(object sender, EventArgs e)
	{
		if (double.TryParse(hoursEntry.Text, out double hours) && hours > 0)
        {
            WorkedOvertime newWorkedOvertime = new(hours, DateTime.Now);
            _overtimeService.AddWorkedOvertime(newWorkedOvertime);
            hoursEntry.Text = "";

			await OvertimeItemAddAsync(newWorkedOvertime);
        }
        else
		{
			// Handle invalid input (not a valid positive integer)
			await DisplayAlert("Error", "Please enter a valid positive number for hours.", "OK");
		}
	}


    private async void OnDeleteClickedAsync(object sender, EventArgs e)
	{
		var button = sender as Button;
		var item = button?.CommandParameter as WorkedOvertime; // Replace YourItemType with the actual type of items in your list

		if (item is not null)
		{
			try
			{
				_overtimeService.DeleteWorkedOvertime(item);
				await OvertimeItemDeleteAsync(item);
			}
			catch (Exception ex)
			{
				// Handle the exception (e.g., log or show an error message)
				await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
			}
		}
    }
    private async Task ShowSuccesLabelAsync(string message)
    {
        // Display a success message using a Label
        successLabel.Text = message;

        // Show the successLabel
        successLabel.IsVisible = true;

        // Fade out animation			
        await successLabel.FadeTo(0, 7000); // 2000 milliseconds (adjust as needed)

        // Hide the successLabel after the animation completes
        successLabel.IsVisible = false;
        successLabel.Opacity = 1; // Reset opacity for potential future showings
    }

    private async Task OvertimeItemDeleteAsync(WorkedOvertime item)
    {
        overtimeItems.Remove(item);
        // Calculate the total sum and update the property
        TotalSum = overtimeItems.Sum(item => item.Hours);
		await ShowSuccesLabelAsync($"Overtime with {item.Hours} hours was removed.");
    }

	private async Task OvertimeItemAddAsync(WorkedOvertime item)
    {
		// Add the new overtime item to the ObservableCollection
		overtimeItems.Insert(0, item);

		// Calculate the total sum and update the property
		TotalSum = overtimeItems.Sum(item => item.Hours);

		await ShowSuccesLabelAsync($"Overtime of {item.Hours} hours successfully added.");
    }

    private void OnEntryCompleted(object sender, EventArgs e) => OnSaveClickedAsync(sender, e);
}