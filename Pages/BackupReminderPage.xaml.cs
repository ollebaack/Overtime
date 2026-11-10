namespace Overtime;

public partial class BackupReminderPage : ContentPage
{
    private const string DontRemindKey = "DontRemindBackup";
    private static bool shownThisSession;

    public BackupReminderPage()
    {
        InitializeComponent();
    }

    // Shown once per start, while no backup folder is set and the user hasn't opted out.
    public static async Task ShowIfNeededAsync(INavigation navigation)
    {
        if (shownThisSession
            || Preferences.Get(DontRemindKey, false)
            || !string.IsNullOrWhiteSpace(OvertimeService.ReadDestinationPath()))
        {
            return;
        }

        shownThisSession = true;
        await navigation.PushModalAsync(new BackupReminderPage(), animated: false);
    }

    private void OnDontRemindLabelTapped(object sender, TappedEventArgs e)
        => dontRemindCheckBox.IsChecked = !dontRemindCheckBox.IsChecked;

    private async void OnNotNowClickedAsync(object sender, EventArgs e) => await CloseAsync();

    private async void OnSetUpClickedAsync(object sender, EventArgs e)
    {
        await CloseAsync();
        await Shell.Current.GoToAsync("//backup");
    }

    private async Task CloseAsync()
    {
        if (dontRemindCheckBox.IsChecked)
        {
            Preferences.Set(DontRemindKey, true);
        }

        await Navigation.PopModalAsync(animated: false);
    }
}
