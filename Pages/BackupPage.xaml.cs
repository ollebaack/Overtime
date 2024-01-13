using Microsoft.Maui.Controls;
using System;
using System.IO;

namespace Overtime
{
    public partial class BackupPage : ContentPage
    {
        public BackupPage()
        {
            InitializeComponent();

            // Set the default text of the Entry to the existing content of the file
            SetDefaultText();
        }

        private void SetDefaultText()
        {
            try
            {
                var path = OvertimeService.BackupFileName;
                // Check if the file exists
                if (File.Exists(path))
                {
                    // If it exists, read the content and set it as the default text
                    string existingContent = File.ReadAllText(path);
                    backupPathEntry.Text = existingContent;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void OnBackupClicked(object sender, EventArgs e)
        {
            try
            {
                var path = OvertimeService.BackupFileName;
                var oldPath = OvertimeService.ReadDestinationPath();

                if (oldPath != OvertimeService.EnsureEndSlash(backupPathEntry.Text))
                {
                    var sourcePath = OvertimeService.GetDatabaseSourcePath();
                    var fileName = OvertimeService.GenerateCopyFileName(sourcePath);

                    var oldFile = Path.Combine(oldPath, fileName);
                    if (OvertimeService.DeleteFileIfExists(oldFile))
                    {
                        DisplayAlert("Success", $"Old backup file '{fileName}' deleted.", "OK");
                    }
                    // Update the content of the file with the new text entered by the user
                    File.WriteAllText(path, backupPathEntry.Text);
                    DisplayAlert("Success", "Backup destination updated successfully.", "OK");

                    OvertimeService.BackUpDatabase();
                }
                else
                {
                    DisplayAlert("Success", "Backup destination is unchanged.", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void OnEntryCompleted(object sender, EventArgs e) => OnBackupClicked(sender, e);
    }
}
