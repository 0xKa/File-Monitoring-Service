namespace WorkerServiceTemplate.Models
{
    public class AppConfiguration
    {
        public DirectoryConfig Directories { get; set; } = new();
        public FileConfig Files { get; set; } = new();
    }

    public class DirectoryConfig
    {
        public string Logs { get; set; } = "Logs";
        public string Data { get; set; } = "Monitoring";
        public string Source { get; set; } = "Source";
        public string Destination { get; set; } = "Destination";
    }

    public class FileConfig
    {
        public string ApplicationLog { get; set; } = "app.log";
        public string MonitoringLog { get; set; } = "monitoring.log";
        public string ErrorLog { get; set; } = "errors.log";
        public string ConfigFile { get; set; } = "appsettings.json";


    }

}