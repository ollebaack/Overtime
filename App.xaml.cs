namespace Overtime;

public partial class App : Application
{
	private readonly OvertimeService _overtimeService = new();
	
	public App()
	{
		InitializeComponent();

		// Call CopyDatabase on startup
		HandleDatabaseOnStartup();
	}

	// The version in the title shows which release is running, e.g. after an update.
	protected override Window CreateWindow(IActivationState? activationState)
		=> new(new AppShell()) { Title = $"Overtime {UpdateChecker.CurrentVersion}" };

	private void HandleDatabaseOnStartup()
	{

		try
		{
			_overtimeService.HandleDatabase();
		}
		catch (Exception ex)
		{
			// Handle the exception (e.g., log or display an error message)
			Console.WriteLine($"Error copying database on startup: {ex.Message}");
		}
	}
}
