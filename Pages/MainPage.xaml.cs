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

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Reload when switching back to the app, to show changes made with the ot CLI.
		if (!reloadsOnActivate && Window is not null)
		{
			Window.Activated += (_, _) => LoadWorkedOvertimes();
			reloadsOnActivate = true;
		}

		await BackupReminderPage.ShowIfNeededAsync(Navigation);
	}

	private bool reloadsOnActivate;

	private void LoadWorkedOvertimes()
	{
		overtimeItems.Clear();

		foreach (var item in _overtimeService.GetAll())
		{
			overtimeItems.Add(item);
		}

		// Calculate the total sum and update the property
		TotalSum = GetSum();
	}

	private async void OnSaveClickedAsync(object sender, EventArgs e)
	{
		if (double.TryParse(hoursEntry.Text, out double hours))
        {
            var addedItem = await _overtimeService.AddAsync(new(hours, DateTime.Now));
            hoursEntry.Text = "";

			await OvertimeItemAddAsync(addedItem);
        }
        else
		{
			// Handle invalid input (not a valid positive integer)
			await DisplayAlertAsync("Error", "Please enter a valid positive number for hours.", "OK");
		}
	}


	private async void OnDeleteClickedAsync(object sender, EventArgs e)
	{
		var button = sender as Button;
		var item = button?.CommandParameter as WorkedOvertime;

		if (item is not null)
		{
			try
			{
				// Update the UI (remove the item from the ObservableCollection)
				await OvertimeItemDeleteAsync(item);
			}
			catch (Exception ex)
			{
				// Handle the exception
				await DisplayAlertAsync("Error", $"An error occurred: {ex.Message}", "OK");
			}
		}
	}

    private async void OnUndoDeleteClickedAsync(object sender, EventArgs e)
    {
		var button = sender as Button;
		var item = button?.CommandParameter as WorkedOvertime;

		if (item is not null)
		{
			try
			{
        		await OvertimeItemUndoDeletedAsync(item);
			}
			catch (Exception ex)
			{
				// Handle the exception
				await DisplayAlertAsync("Error", $"An error occurred: {ex.Message}", "OK");
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
        await successLabel.FadeToAsync(0, 7000); // 2000 milliseconds (adjust as needed)

        // Hide the successLabel after the animation completes
        successLabel.IsVisible = false;
        successLabel.Opacity = 1; // Reset opacity for potential future showings
    }

    private async Task OvertimeItemDeleteAsync(WorkedOvertime item)
    {
		var deletedItem = await _overtimeService.DeleteAsync(item);
        var deletedOvertimeItem = overtimeItems.FirstOrDefault(i => i.Id == item.Id);

		if (deletedOvertimeItem != null)
		{
			deletedOvertimeItem.Deleted = true;

			// Calculate the total sum and update the property
			TotalSum = GetSum();
			await ShowSuccesLabelAsync($"Overtime with {item.Hours} hours was removed.");
		}
    }


    private async Task OvertimeItemUndoDeletedAsync(WorkedOvertime item)
	{
		var unDeletedItem = await _overtimeService.UndoDeleteAsync(item);

		item.Deleted = false;

		// Calculate the total sum and update the property
		TotalSum = GetSum();

		await ShowSuccesLabelAsync($"Overtime with {item.Hours} hours was successfully un removed.");
	}

	private async Task OvertimeItemAddAsync(WorkedOvertime item)
    {
		// Add the new overtime item to the ObservableCollection
		overtimeItems.Insert(0, item);

		// Calculate the total sum and update the property
		TotalSum = GetSum();

		await ShowSuccesLabelAsync($"Overtime of {item.Hours} hours successfully added.");
    }
    private double GetSum() 
		=> overtimeItems
            .Where(x => !x.Deleted)
            .Sum(item => item.Hours);

    private void OnEntryCompleted(object sender, EventArgs e) => OnSaveClickedAsync(sender, e);
}