# Installs or updates Overtime from the latest GitHub release. No admin rights needed.
#   irm https://raw.githubusercontent.com/ollebaack/Overtime/main/install.ps1 | iex
$ErrorActionPreference = 'Stop'
# Windows PowerShell's progress bar makes Invoke-WebRequest and Expand-Archive many times slower.
$ProgressPreference = 'SilentlyContinue'

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\Overtime'
$release = Invoke-RestMethod 'https://api.github.com/repos/ollebaack/Overtime/releases/latest'
$asset = $release.assets | Where-Object name -eq 'Overtime-win-x64.zip'
if (-not $asset) { throw "Release $($release.tag_name) has no Overtime-win-x64.zip." }

$running = Get-Process Overtime -ErrorAction SilentlyContinue
$running | Stop-Process -Force
$running | Wait-Process -Timeout 15 -ErrorAction SilentlyContinue

$zip = Join-Path $env:TEMP 'Overtime-win-x64.zip'
Write-Host "Downloading Overtime $($release.tag_name)..."
Invoke-WebRequest $asset.browser_download_url -OutFile $zip

# The data (OvertimeDb.db3) lives in %LOCALAPPDATA%, not in the install folder, so replacing it is safe.
# Empty the folder rather than deleting it: a folder can't be deleted while a process has it as its
# current directory, and the app's "Update now" starts PowerShell there.
if (Test-Path -LiteralPath $installDir) { Get-ChildItem -LiteralPath $installDir -Force | Remove-Item -Recurse -Force }
Expand-Archive -LiteralPath $zip -DestinationPath $installDir -Force
[IO.File]::Delete($zip)  # Remove-Item fails on 8.3 temp paths such as OLLE~1.BAC

$shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut(
    (Join-Path ([Environment]::GetFolderPath('Programs')) 'Overtime.lnk'))
$shortcut.TargetPath = Join-Path $installDir 'Overtime.exe'
$shortcut.WorkingDirectory = $installDir
$shortcut.Save()

# Put the install folder on the user's PATH so the "ot" CLI works from any terminal.
# Edited in the registry so entries like %USERPROFILE%\... stay unexpanded.
$envKey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Environment', $true)
$userPath = $envKey.GetValue('Path', '', 'DoNotExpandEnvironmentNames')
if (($userPath -split ';') -notcontains $installDir) {
    $entries = @($userPath -split ';' | Where-Object { $_ }) + $installDir
    $envKey.SetValue('Path', ($entries -join ';'), 'ExpandString')
    # Setting and clearing a variable broadcasts the change, so new terminals pick up the PATH.
    [Environment]::SetEnvironmentVariable('OVERTIME_INSTALL', '1', 'User')
    [Environment]::SetEnvironmentVariable('OVERTIME_INSTALL', $null, 'User')
    Write-Host "Added $installDir to your PATH. Open a new terminal to use 'ot'."
}
$envKey.Close()

Write-Host "Overtime $($release.tag_name) installed to $installDir. Start it from the Start menu, or run 'ot help'."
