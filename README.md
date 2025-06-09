# File-Monitoring-Service

A Windows Service application built with .NET 9.0 that monitors file system changes and provides logging capabilities.

## Overview

This service monitors specified directories for file system changes and logs activities. It's designed to run as a Windows Service with configurable source and destination directories.

## Features

- File system monitoring with real-time change detection
- Configurable source and destination directories
- Comprehensive logging with timestamp prefixes
- Windows Service integration
- Dependency injection support

## Running the Service

### Development Mode

```bash
dotnet run
```

### As Windows Service

1. Build the project in Release mode and publish it:

   ```powershell
   dotnet publish -c Release -o ./publish
   ```

2. Install as Windows Service using `sc create` or PowerShell
3. Start the service through Windows Services Manager

## Logging

The service provides comprehensive logging through the [`Utilities.LogMessage`](Utilities.cs) method:

- Timestamped log entries
- Multiple log files support
- Console and Debug output support

Log files are stored in the `Logs/` directory.

## Dependencies

- .NET 9.0
- Microsoft.Extensions.Hosting (for Windows Service support)
- Microsoft.Extensions.Configuration (for configuration management)
- Microsoft.Extensions.DependencyInjection (for dependency injection)

## Development

### Prerequisites

- Visual Studio 2022 or Visual Studio Code
- .NET SDK

### Key Components

- [`Program.cs`](Program.cs) - Service host configuration and startup
- [`Worker.cs`](Worker.cs) - Main background service implementation
- [`Utilities.cs`](Utilities.cs) - Helper methods for logging and directory operations
- [`Models/AppConfiguration.cs`](Models/AppConfiguration.cs) - Configuration model

### Utility Functions

The [`Utilities`](Utilities.cs) class provides:

- [`LogMessage`](Utilities.cs) - Safe logging with timestamp
- [`GetConfiguredDirectory`](Utilities.cs) - Configuration-based directory retrieval
- [`GetConfiguredFilePath`](Utilities.cs) - Configuration-based file path retrieval
- [`CreateDirectoryInside`](Utilities.cs) - Directory creation utilities
