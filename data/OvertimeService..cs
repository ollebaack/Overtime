

using Microsoft.EntityFrameworkCore;

namespace Overtime;

public class OvertimeService
{
    private readonly OvertimeDbContext _dbContext;
    // Kept in the app's data folder; a relative path would resolve to the (read-only) install folder.
    public static readonly string BackupFileName = Path.Combine(FileSystem.AppDataDirectory, "backUpPath.txt");

    public OvertimeService()
    {
        _dbContext = new OvertimeDbContext();
        _dbContext.Database.EnsureCreated();
    }

    public async Task<WorkedOvertime> AddAsync(WorkedOvertime workedOvertime)
    {
        await _dbContext.WorkedOvertimes.AddAsync(workedOvertime);
        await _dbContext.SaveChangesAsync();

        return workedOvertime;
    }

    public async Task<WorkedOvertime?> DeleteAsync(WorkedOvertime workedOvertime)
    {
        var item = await _dbContext.WorkedOvertimes.FindAsync(workedOvertime.Id);

        if (item != null)
        {
            item.Deleted = true;
            await _dbContext.SaveChangesAsync();
        }

        return item;
    }

    public async Task<WorkedOvertime?> UndoDeleteAsync(WorkedOvertime workedOvertime)
    {
        var item = await _dbContext.WorkedOvertimes.FindAsync(workedOvertime.Id);

        if (item != null)
        {
            item.Deleted = false;
            await _dbContext.SaveChangesAsync();
        }

        return item;
    }

    public void RemoveAllDeleted()
    {
        foreach (var item in _dbContext.WorkedOvertimes.Where(item => item.Deleted))
        {
            _dbContext.WorkedOvertimes.Remove(item);
        }

        _dbContext.SaveChanges();
    }

    public async Task<List<WorkedOvertime>> GetAllAsync()
        => await _dbContext.WorkedOvertimes
            .OrderByDescending(overtime => overtime.Id)
            .ToListAsync();

    public List<WorkedOvertime> GetAll() 
        => _dbContext.WorkedOvertimes
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

    public void HandleDatabase()
    {
        BackUpDatabase();
        RemoveAllDeleted();
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
