# Overtime

A small .NET MAUI app for logging overtime hours. Entries are stored in a local SQLite database
(`%LOCALAPPDATA%\OvertimeDb.db3`), which can be copied to a backup folder on every start.

## Install or update (Windows)

Run in PowerShell. No admin rights needed:

```powershell
irm https://raw.githubusercontent.com/ollebaack/Overtime/main/install.ps1 | iex
```

This downloads the latest release to `%LOCALAPPDATA%\Programs\Overtime` and adds a Start menu
shortcut. Run it again to update. Your data is kept.

## Release

Pushing a `v*` tag builds the Windows app and publishes it as a GitHub release:

```sh
git tag v1.2.0
git push origin v1.2.0
```

Every push and pull request also builds the app; the zip is available as a workflow artifact.

## Build locally

Needs the .NET 10 SDK and the MAUI workload (`dotnet workload install maui-windows`).

```sh
dotnet build Overtime.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
```
