namespace Overtime;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Call CopyDatabase on startup
		CopyDatabaseOnStartup();

		MainPage = new AppShell();
	}

	private void CopyDatabaseOnStartup()
	{
		try
		{
			OvertimeService.BackUpDatabase();
		}
		catch (Exception ex)
		{
			// Handle the exception (e.g., log or display an error message)
			Console.WriteLine($"Error copying database on startup: {ex.Message}");
		}
	}
}
