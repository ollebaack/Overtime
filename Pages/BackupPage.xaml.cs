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
                DisplayAlertAsync("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void OnBackupClicked(object sender, EventArgs e)
        {
            try
            {
                var oldPath = OvertimeService.ReadDestinationPath();

                if (oldPath != OvertimeService.EnsureEndSlash(backupPathEntry.Text))
                {
                    // Backups in the old folder are kept.
                    OvertimeService.SaveDestinationPath(backupPathEntry.Text);
                    DisplayAlertAsync("Success", "Backup destination updated successfully.", "OK");

                    OvertimeService.BackUpDatabase();
                }
                else
                {
                    DisplayAlertAsync("Success", "Backup destination is unchanged.", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlertAsync("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void OnEntryCompleted(object sender, EventArgs e) => OnBackupClicked(sender, e);
    }
}
