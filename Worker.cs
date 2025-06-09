using Microsoft.Extensions.Options;
using WorkerServiceTemplate.Models;
using static WorkerServiceTemplate.Utilities;

namespace WorkerServiceTemplate;


public struct Files
{
    public static string appLog = string.Empty; // log service lifecycle 
    public static string monitoringLog = string.Empty; // log file monitoring events
    public static string errorLog = string.Empty;

    public static string DestinationDirectory = string.Empty;
    public static string SourceDirectory = string.Empty;

    public static void InitializeFields(IOptions<AppConfiguration> config)
    {
        appLog = GetConfiguredFilePath("ApplicationLog", config.Value.Directories.Logs);
        monitoringLog = GetConfiguredFilePath("MonitoringLog", config.Value.Directories.Logs);
        errorLog = GetConfiguredFilePath("ErrorLog", config.Value.Directories.Logs);

        //Source and destination directories, FileWatcher will monitor the source directory and copy to the destination directory
        DestinationDirectory = Utilities.CreateDirectoryInside("Destination", config.Value.Directories.Monitoring);
        SourceDirectory = Utilities.CreateDirectoryInside("Source", config.Value.Directories.Monitoring);
    }

}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    public const string ServiceInternalName = "FileMonitoringService";
    private FileSystemWatcher _fileSystemWatcher;

    public Worker(ILogger<Worker> logger, IOptions<AppConfiguration> config, IServiceProvider serviceProvider)
    {
        _logger = logger;

        // Initialize utilities with service provider
        Initialize(serviceProvider);
        Files.InitializeFields(config);


        _fileSystemWatcher = new FileSystemWatcher
        {
            Path = Files.SourceDirectory,
            Filter = "*.*", // Monitor all files
            EnableRaisingEvents = true,
            IncludeSubdirectories = true
        };

        _fileSystemWatcher.Created += OnFileCreated;
        _fileSystemWatcher.Deleted += OnFileDeleted;
        _fileSystemWatcher.Changed += OnFileChanged;
        _fileSystemWatcher.Renamed += OnFileRenamed;

    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        LogMessage($"File Created: {e.FullPath}", Files.monitoringLog);

        string newFilePath = Path.Combine(Files.DestinationDirectory, $"{Guid.NewGuid()}{Path.GetExtension(e.FullPath)}");
    }
    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        LogMessage($"File Deleted: {e.FullPath}", Files.monitoringLog);
    }
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        LogMessage($"File Renamed from {e.OldName} to {e.Name}, \n\t>> Old Path: {e.OldFullPath}\n\t>> New Path: {e.FullPath}", Files.monitoringLog);
    }
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (!ShouldProcessEvent(e.FullPath)) return; // debounce logic to prevent calling the event twice

        LogMessage($"File Changed: {e.FullPath}", Files.monitoringLog);

        string newFilePath = Path.Combine(Files.DestinationDirectory, $"{Guid.NewGuid()}{Path.GetExtension(e.FullPath)}");
        File.Copy(e.FullPath, newFilePath);
        // File.Move(e.FullPath, newFilePath); //we can move
    }

    private static readonly Dictionary<string, DateTime> _lastEventTimes = new();
    private static readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(100);
    private static bool ShouldProcessEvent(string filePath)
    {
        if (_lastEventTimes.ContainsKey(filePath))
            if (DateTime.Now - _lastEventTimes[filePath] < _debounceInterval)
                return false;

        _lastEventTimes[filePath] = DateTime.Now;
        return true;
    }


    public override Task StartAsync(CancellationToken cancellationToken)
    {
        string runMode = Environment.UserInteractive ?
            "Console Mode (UserInteractive = true)" :
            "Windows Service (UserInteractive = false)";

        LogMessage($"Service '{ServiceInternalName}' Started in {runMode}.", Files.appLog);

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        LogMessage($"Service '{ServiceInternalName}' Stopped.", Files.appLog);

        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // ===== WORKER LOGIC GOES HERE =====

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // ===== END OF WORKER LOGIC =====

                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                LogMessage("Worker execution cancelled", Files.appLog);
                break;
            }
            catch (Exception ex)
            {
                LogMessage($"Error in worker execution: {ex.Message}", Files.appLog);
                _logger.LogError(ex, "Worker execution failed");

                // Wait before retrying to avoid rapid error loops
                await Task.Delay(10000, stoppingToken);
            }
        }

    }
}
