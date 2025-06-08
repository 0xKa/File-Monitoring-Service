namespace FileMonitoringService;
public class Utilities
{

    public static void LogMessage(string message, string logFile)
    {
        string fullMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: {message}";

        try
        {
            File.AppendAllText(logFile, fullMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }

        if (Environment.UserInteractive)
            Console.WriteLine(fullMessage); // For debugging in console applications
    }



    public static string GetProjectRootDirectory()
    {
        if (Environment.UserInteractive)
            // Running in console or debugger
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
        else
            // Running as a Windows Service, AppContext.BaseDirectory will be the Publish/ folder
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\"));
    }


    public static string CreateDirectoryWithinProject(string directoryName)
    {
        string newDirectory = Path.Combine(GetProjectRootDirectory(), directoryName);

        if (!Directory.Exists(newDirectory))
            Directory.CreateDirectory(newDirectory);

        return newDirectory;
    }

    public static string CreateFile(string directoryPath, string fileName, bool overwrite = false)
    {
        string filePath = Path.Combine(directoryPath, fileName);

        if (!File.Exists(filePath) || overwrite)
        {
            using var stream = File.Create(filePath); // Auto-disposes
        }

        return filePath;
    }


}
