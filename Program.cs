using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FileMonitoringService; //The namespace of your Worker class


Host.CreateDefaultBuilder(args)
    .UseWindowsService() // Enables running as a Windows Service
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
    })
    .Build()
    .Run();


