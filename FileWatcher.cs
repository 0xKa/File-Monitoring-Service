namespace FileMonitoringService;


public class FileWatcher
{

    private static string _monitoringDirectory = string.Empty;
    private static string _sourceDirectory = string.Empty;
    private static string _destinationDirectory = string.Empty;

    private static string _monitoringLogFile = string.Empty;
    private static string _errorLogFile = string.Empty;

    private static FileSystemWatcher? _watcher;

    public static void SetupFileWatcher(IConfiguration configuration)
    {
        SetupLogsAndMonitoringDirectories(configuration);

        _watcher = new FileSystemWatcher(_sourceDirectory);

        _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;


        _watcher.Filter = "*.*"; // Monitor all file types
        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;

        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnError;


    }

    private static void OnError(object sender, ErrorEventArgs e)
    {
        Utilities.LogMessage($"FileSystemWatcher error: {e.GetException().Message}", _errorLogFile);
    }

    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
        Utilities.LogMessage(
            @$"File renamed from {e.OldName} to {e.Name}
            Before rename full path : {e.OldFullPath}
            After rename full path  : {e.FullPath}"
            , _monitoringLogFile
            );
    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        Utilities.LogMessage($"File Changed: {e.FullPath}", _monitoringLogFile);

    }

    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        Utilities.LogMessage($"File Created: {e.FullPath}", _monitoringLogFile);

    }

    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Utilities.LogMessage($"File Deleted: {e.FullPath}", _monitoringLogFile);
    }


    private static void SetupLogsAndMonitoringDirectories(IConfiguration configuration)
    {
        _monitoringDirectory = configuration["FileMonitoring:MonitoringDirectoryPath"] ?? throw new InvalidOperationException("MonitoringDirectoryPath not configured");
        _sourceDirectory = configuration["FileMonitoring:SourceDirectoryPath"] ?? throw new InvalidOperationException("SourceDirectoryPath not configured");
        _destinationDirectory = configuration["FileMonitoring:DestinationDirectoryPath"] ?? throw new InvalidOperationException("DestinationDirectoryPath not configured");


        if (!Directory.Exists(_monitoringDirectory))
            Directory.CreateDirectory(_monitoringDirectory);
        if (!Directory.Exists(_sourceDirectory))
            Directory.CreateDirectory(_sourceDirectory);
        if (!Directory.Exists(_destinationDirectory))
            Directory.CreateDirectory(_destinationDirectory);

        string logsDirectory = Utilities.CreateDirectoryWithinProject(configuration["FileMonitoringLogs:LogDirectory"] ?? "Logs");
        _monitoringLogFile = Utilities.CreateFile(logsDirectory, configuration["FileMonitoringLogs:MonitoringLogFile"] ?? "Monitoring.log");
        _errorLogFile = Utilities.CreateFile(logsDirectory, configuration["FileMonitoringLogs:ErrorsLogFile"] ?? "Errors.log");

        // Log that monitoring started
        Utilities.LogMessage($"File monitoring started for directory: {_sourceDirectory}", _monitoringLogFile);
    }
    
    public static void StopFileWatcher()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
            Utilities.LogMessage("File monitoring stopped", _monitoringLogFile);
        }
    }

}