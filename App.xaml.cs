namespace Overtime;

public partial class App : Application
{
	private readonly OvertimeService _overtimeService = new();
	
	public App()
	{
		InitializeComponent();

		// Call CopyDatabase on startup
		HandleDatabaseOnStartup();

		MainPage = new AppShell();
	}

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
