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
          ot delete <id>                      Delete an entry
          ot restore <id>                     Undo a delete (until the app is next started)
          ot help                             Show this help

        Changes made while the app is open show up the next time it starts.
        """);
}
