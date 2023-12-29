namespace Overtime;

public partial class MainPage : ContentPage
{
    private readonly OvertimeService _overtimeService;

    public MainPage()
    {
        InitializeComponent();

        _overtimeService = new OvertimeService();
	}

	private async void OnSaveClicked(object sender, EventArgs e)
	{

		if (int.TryParse(hoursEntry.Text, out int hours))
		{
			WorkedOvertime newWorkedOvertime = new(hours, DateTime.Now);
			_overtimeService.AddWorkedOvertime(newWorkedOvertime);
		}
		else
		{
			// Handle invalid input (not a valid integer)
			await DisplayAlert("Error", "Please enter a valid number for hours.", "OK");
		}
	}

	private async void OnShowClicked(object sender, EventArgs e)
	{
		// Example: Retrieve all worked overtime
        var allWorkedOvertimes = _overtimeService.GetAllWorkedOvertimes();
        foreach (var workedOvertime in allWorkedOvertimes)
        {
            // Handle each workedOvertime as needed
            Console.WriteLine($"Date: {workedOvertime.Date}, Hours: {workedOvertime.Hours}");
        }
	}
}

