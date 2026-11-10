namespace Overtime;

public partial class UpdatePage : ContentPage
{
    private static bool checkedThisSession;
    private readonly AvailableUpdate _update;

    public UpdatePage(AvailableUpdate update)
    {
        InitializeComponent();

        _update = update;
        titleLabel.Text = $"Overtime {update.Version} is available";
        versionLabel.Text = $"You have {UpdateChecker.CurrentVersion}. Updating takes about half a minute and keeps your data.";
        notesLabel.Text = update.ReleaseNotes;
        notesLabel.IsVisible = update.ReleaseNotes.Length > 0;
    }

    // Checked once per start; shows nothing when up to date or offline.
    public static async Task ShowIfAvailableAsync(INavigation navigation)
    {
        if (checkedThisSession)
        {
            return;
        }

        checkedThisSession = true;
        var update = await UpdateChecker.CheckAsync();
        if (update is not null)
        {
            await navigation.PushModalAsync(new UpdatePage(update), animated: false);
        }
    }

    private async void OnSkipClickedAsync(object sender, EventArgs e)
    {
        UpdateChecker.Skip(_update);
        await Navigation.PopModalAsync(animated: false);
    }

    private async void OnLaterClickedAsync(object sender, EventArgs e) => await Navigation.PopModalAsync(animated: false);

    private void OnUpdateClicked(object sender, EventArgs e)
    {
        UpdateChecker.StartInstall();
        Application.Current?.Quit();
    }
}
