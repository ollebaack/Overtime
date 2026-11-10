# Installs or updates Overtime from the latest GitHub release. No admin rights needed.
#   irm https://raw.githubusercontent.com/ollebaack/Overtime/main/install.ps1 | iex
$ErrorActionPreference = 'Stop'

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\Overtime'
$release = Invoke-RestMethod 'https://api.github.com/repos/ollebaack/Overtime/releases/latest'
$asset = $release.assets | Where-Object name -eq 'Overtime-win-x64.zip'
if (-not $asset) { throw "Release $($release.tag_name) has no Overtime-win-x64.zip." }

Get-Process Overtime -ErrorAction SilentlyContinue | Stop-Process -Force

$zip = Join-Path $env:TEMP 'Overtime-win-x64.zip'
Write-Host "Downloading Overtime $($release.tag_name)..."
Invoke-WebRequest $asset.browser_download_url -OutFile $zip

# The data (OvertimeDb.db3) lives in %LOCALAPPDATA%, not in the install folder, so replacing it is safe.
if (Test-Path $installDir) { Remove-Item $installDir -Recurse -Force }
Expand-Archive $zip -DestinationPath $installDir
Remove-Item $zip

$shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut(
    (Join-Path ([Environment]::GetFolderPath('Programs')) 'Overtime.lnk'))
$shortcut.TargetPath = Join-Path $installDir 'Overtime.exe'
$shortcut.WorkingDirectory = $installDir
$shortcut.Save()

Write-Host "Overtime $($release.tag_name) installed to $installDir. Start it from the Start menu."
