
namespace Overtime;

public class OvertimeService
{
    private readonly OvertimeDbContext _dbContext;
    public const string BackupFileName = "backUpPath.txt";

    public OvertimeService()
    {
        _dbContext = new OvertimeDbContext();
        _dbContext.Database.EnsureCreated();
    }

    public void AddWorkedOvertime(WorkedOvertime workedOvertime)
    {
        _dbContext.WorkedOvertimes.Add(workedOvertime);
        _dbContext.SaveChanges();
    }

    public void DeleteWorkedOvertime(WorkedOvertime workedOvertime)
    {
        _dbContext.WorkedOvertimes.Remove(workedOvertime);
        _dbContext.SaveChanges();
    }

    public List<WorkedOvertime> GetAllWorkedOvertimes() => 
        _dbContext.WorkedOvertimes
            .OrderByDescending(overtime => overtime.Id)
            .ToList();

    public static void BackUpDatabase()
    {
        try
        {
            // Read the destination path from the "backup.txt" file
            string destinationPath = ReadDestinationPath();

            // Validate the destination path
            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                return;
            }

            // Ensure the source database file exists
            string sourcePath = GetDatabaseSourcePath();

            // Combine the destination path with the generated copy file name
            string destinationFilePath = Path.Combine(destinationPath, GenerateCopyFileName(sourcePath));

            // Ensure the destination folder exists
            EnsureFolderExists(Path.GetDirectoryName(destinationFilePath) ?? throw new InvalidOperationException($"Error when getting directoryName."));

            // Copy the database file to the specified destination
            File.Copy(sourcePath, destinationFilePath, true);
        }
        catch (Exception ex)
        {
            // Handle the exception (e.g., log or rethrow)
            throw new InvalidOperationException($"Error copying database: {ex.Message}", ex);
        }
    }


    public static string GetDatabaseSourcePath()
    {
        string sourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OvertimeDb.db3");
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Source database file not found.", sourcePath);
        }

        return sourcePath;
    }

    public static string ReadDestinationPath()
    {
        try
        {
            if (File.Exists(BackupFileName))
            {
                string path = File.ReadAllText(BackupFileName);

                // Ensure the path ends with a slash ("/")
                path = EnsureEndSlash(path);

                return path;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            // Handle the exception (e.g., log or rethrow)
            throw new InvalidOperationException($"Error reading destination path from 'backup.txt': {ex.Message}", ex);
        }
    }

    public static string EnsureEndSlash(string path)
    {
        if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
        {
            path += "/";
        }

        return path;
    }

    public static bool DeleteFileIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);

            // Try to delete the parent folder if it's empty
            string? folderPath = Path.GetDirectoryName(filePath);
            if (Directory.Exists(folderPath) && !Directory.EnumerateFileSystemEntries(folderPath).Any())
            {
                Directory.Delete(folderPath);
            }

            return true;
        }

        return false;
    }
    public static void EnsureFolderExists(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    public static string GenerateCopyFileName(string originalFilePath, string prefix = "Copy_")
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilePath);
        string extension = Path.GetExtension(originalFilePath);

        return prefix + fileNameWithoutExtension + extension;
    }

}
