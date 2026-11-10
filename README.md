# Overtime

A small .NET MAUI app for logging overtime hours, with a Statistics tab that charts your balance and
hours per month. Entries are stored in a local SQLite database
(`%LOCALAPPDATA%\OvertimeDb.db3`), which can be copied to a backup folder on every start.

## Install or update (Windows)

Run in PowerShell. No admin rights needed:

```powershell
irm https://raw.githubusercontent.com/ollebaack/Overtime/main/install.ps1 | iex
```

This downloads the latest release to `%LOCALAPPDATA%\Programs\Overtime` and adds a Start menu
shortcut. When a new version is released, the app offers to update itself on startup (you can also
run the command again). Your data is kept.

## Command line

The installer also puts `ot` on your PATH. It uses the same data as the app:

```text
ot                                   Show total overtime
ot add 1,5                           Add hours today (1.5 works too; negative to use time off)
ot add 2 --date 2026-11-09           Add hours on another day
ot list [count | --all]              Show the latest entries (default 10)
ot stats                             Show balance, averages and hours per month
ot delete <id> / ot restore <id>     Delete an entry, or undo it
ot backup                            Show the backup folder
ot backup set <folder>               Set the backup folder (same as the app's Backup page) and back up now
ot backup remove                     Stop backing up; existing backups are kept
ot backup now                        Back up now
```

If the app is open, it picks up changes made with `ot` when you switch back to it.

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

The CLI is a plain console project:

```sh
dotnet run --project cli/Overtime.Cli.csproj -- list
```
