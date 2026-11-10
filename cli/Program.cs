using System.Globalization;
using Overtime;

using var db = new OvertimeDbContext();
db.Database.EnsureCreated();

var command = args.Length > 0 ? args[0].ToLowerInvariant() : "total";

switch (command)
{
    case "total":
        PrintTotal();
        return 0;

    case "add":
        return Add(args.Skip(1).ToArray());

    case "list":
        return List(args.Skip(1).ToArray());

    case "delete":
        return SetDeleted(args.Skip(1).ToArray(), deleted: true);

    case "restore":
        return SetDeleted(args.Skip(1).ToArray(), deleted: false);

    case "backup":
        return Backup(args.Skip(1).ToArray());

    case "stats":
        PrintStats();
        return 0;

    case "help" or "-h" or "--help" or "/?":
        PrintHelp();
        return 0;

    default:
        Console.Error.WriteLine($"Unknown command '{args[0]}'.");
        PrintHelp();
        return 1;
}

int Add(string[] options)
{
    if (options.Length == 0 || !TryParseHours(options[0], out var hours))
    {
        Console.Error.WriteLine("Usage: ot add <hours> [--date yyyy-MM-dd]");
        return 1;
    }

    var date = DateTime.Now;
    var dateIndex = Array.FindIndex(options, o => o is "--date" or "-d");
    if (dateIndex >= 0)
    {
        if (dateIndex + 1 >= options.Length
            || !DateTime.TryParseExact(options[dateIndex + 1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            Console.Error.WriteLine("--date must be yyyy-MM-dd.");
            return 1;
        }
    }

    var item = new WorkedOvertime(hours, date);
    db.WorkedOvertimes.Add(item);
    db.SaveChanges();

    Console.WriteLine($"Added {FormatHours(hours)} hours on {date:yyyy-MM-dd} (id {item.Id}).");
    PrintTotal();
    return 0;
}

int List(string[] options)
{
    var count = 10;
    if (options.Length > 0)
    {
        if (options[0] is "--all" or "-a")
        {
            count = int.MaxValue;
        }
        else if (!int.TryParse(options[0], out count) || count < 1)
        {
            Console.Error.WriteLine("Usage: ot list [count | --all]");
            return 1;
        }
    }

    var items = db.WorkedOvertimes
        .Where(item => !item.Deleted)
        .OrderByDescending(item => item.Id)
        .Take(count)
        .ToList();

    Console.WriteLine($"{"Id",5}  {"Date",-10}  {"Hours",6}");
    foreach (var item in items)
    {
        Console.WriteLine($"{item.Id,5}  {item.Date:yyyy-MM-dd}  {FormatHours(item.Hours),6}");
    }

    PrintTotal();
    return 0;
}

int SetDeleted(string[] options, bool deleted)
{
    if (options.Length == 0 || !int.TryParse(options[0], out var id))
    {
        Console.Error.WriteLine($"Usage: ot {(deleted ? "delete" : "restore")} <id>");
        return 1;
    }

    var item = db.WorkedOvertimes.Find(id);
    if (item is null)
    {
        // The app removes deleted entries for good when it starts.
        Console.Error.WriteLine($"No entry with id {id}.");
        return 1;
    }

    item.Deleted = deleted;
    db.SaveChanges();

    Console.WriteLine($"{(deleted ? "Deleted" : "Restored")} {FormatHours(item.Hours)} hours on {item.Date:yyyy-MM-dd} (id {id}).");
    PrintTotal();
    return 0;
}

int Backup(string[] options)
{
    var folder = BackupFolder();

    switch (options.Length > 0 ? options[0].ToLowerInvariant() : "show")
    {
        case "show":
            Console.WriteLine(folder is null
                ? "No backup folder set. Set one with: ot backup set <folder>"
                : $"Backup folder: {folder}\nThe app copies the data there every time it starts.");
            return 0;

        case "set":
            if (options.Length < 2 || !Path.IsPathFullyQualified(options[1]))
            {
                Console.Error.WriteLine("Usage: ot backup set <full folder path>");
                return 1;
            }

            OvertimeService.SaveDestinationPath(options[1]);
            OvertimeService.BackUpDatabase();
            Console.WriteLine($"Backup folder set to {BackupFolder()}. Backed up to {BackupFile()}.");
            return 0;

        case "remove":
            if (folder is null)
            {
                Console.WriteLine("No backup folder set.");
                return 0;
            }

            OvertimeService.SaveDestinationPath("");
            Console.WriteLine($"Backup folder removed. Existing backups in {folder} are kept.");
            return 0;

        case "now":
            if (folder is null)
            {
                Console.Error.WriteLine("No backup folder set. Set one with: ot backup set <folder>");
                return 1;
            }

            OvertimeService.BackUpDatabase();
            Console.WriteLine($"Backed up to {BackupFile()}.");
            return 0;

        default:
            Console.Error.WriteLine("Usage: ot backup [set <folder> | remove | now]");
            return 1;
    }
}

static string? BackupFolder()
{
    var folder = OvertimeService.ReadDestinationPath().Trim();
    return folder.Length == 0 ? null : Path.GetFullPath(folder).TrimEnd('\\', '/');
}

static string BackupFile()
    => Path.Combine(BackupFolder()!, OvertimeService.GenerateCopyFileName(OvertimeService.GetDatabaseSourcePath()));

void PrintStats()
{
    var stats = new OvertimeStatistics(db.WorkedOvertimes.ToList(), DateTime.Today);
    var since = stats.Months.Count > 0 ? stats.Months[0].Month.ToString("yyyy-MM") : "";

    Console.WriteLine($"Balance:            {FormatHours(stats.Balance)} h");
    Console.WriteLine($"This month:         {FormatSigned(stats.ThisMonth)} h");
    Console.WriteLine($"Last month:         {FormatSigned(stats.LastMonth)} h");
    Console.WriteLine($"Average per month:  {FormatSigned(stats.AveragePerMonth)} h (whole months since {since})");
    Console.WriteLine($"Last 6 months:      {FormatSigned(stats.RecentAveragePerMonth)} h per month");
    Console.WriteLine();
    Console.WriteLine("Hours per month (# overtime, - time off):");

    var recent = stats.Months.TakeLast(12).ToList();
    var largest = Math.Max(1, recent.Max(month => Math.Abs(month.Hours)));
    foreach (var month in recent)
    {
        var bar = new string(month.Hours >= 0 ? '#' : '-', (int)Math.Round(Math.Abs(month.Hours) / largest * 30));
        Console.WriteLine($"  {month.Month:yyyy-MM}  {FormatSigned(month.Hours),7}  {bar}");
    }
}

static string FormatSigned(double hours) => (hours > 0 ? "+" : "") + FormatHours(hours);

void PrintTotal()
{
    var total = db.WorkedOvertimes.Where(item => !item.Deleted).Sum(item => item.Hours);
    Console.WriteLine($"Total overtime: {FormatHours(total)} hours");
}

// Accept both the current culture's decimal separator ("1,5" in Swedish) and a dot ("1.5").
static bool TryParseHours(string text, out double hours)
    => double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out hours)
        || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out hours);

static string FormatHours(double hours) => hours.ToString("0.##", CultureInfo.CurrentCulture);

static void PrintHelp()
{
    Console.WriteLine("""
        Overtime CLI. Uses the same data as the Overtime app.

        Usage:
          ot [total]                          Show total overtime
          ot add <hours> [--date yyyy-MM-dd]  Add hours (negative to use time off), default today
          ot list [count | --all]             Show the latest entries (default 10)
          ot stats                            Show averages and hours per month
          ot delete <id>                      Delete an entry
          ot restore <id>                     Undo a delete (until the app is next started)
          ot backup                           Show the backup folder
          ot backup set <folder>              Set the backup folder and back up now
          ot backup remove                    Stop backing up (existing backups are kept)
          ot backup now                       Back up now
          ot help                             Show this help

        If the app is open, it picks up changes when you switch back to it.
        """);
}
