namespace FileMonitoringService;

using static FileMonitoringService.Utilities;
using Microsoft.Extensions.Configuration;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public const string ServiceInternalName = "FileMonitoringService";
    private string _logDirectory;
    private string _logFile;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        string logDirectoryName = _configuration["FileMonitoringLogs:LogDirectory"] ?? "Logs";
        string logFileName = _configuration["FileMonitoringLogs:ServiceLifecycleLogFile"] ?? "temp.log";

        _logDirectory = CreateDirectoryWithinProject(logDirectoryName);
        _logFile = CreateLogFile(_logDirectory, logFileName);
    }

    private void LogMessage(string message)
    {
        string fullMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: {message}";

        try
        {
            File.AppendAllText(_logFile, fullMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write to log file.");
        }



        if (Environment.UserInteractive)
            Console.WriteLine(fullMessage); // For debugging in console applications
        else
            _logger.LogInformation(fullMessage); // For logging in Windows Service applications

    }



    public override Task StartAsync(CancellationToken cancellationToken)
    {
        LogMessage(Environment.UserInteractive ? "Running in console mode (UserInteractive = true)" : "Running as a Windows Service (UserInteractive = false)");
        LogMessage("Main Log File Path: " + _logFile);
        LogMessage("Service Started.");
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        LogMessage("Service Stopped.");
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Worker Service running at: {time}", DateTimeOffset.Now);

            // await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); //Adjust the delay as needed, I used 1 minute for a heartbeat logging example.
            await Task.Delay(1000, stoppingToken);
        }
    }
}
