using System;
using System.IO;
using Serilog;
using Serilog.Formatting.Json;

namespace ApiTestingFramework.Implementations
{
    public static class SerilogConfiguration
    {
        public static Serilog.ILogger CreateLogger(string categoryName = "TestFramework")
        {
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var logFilePath = Path.Combine(logDirectory, $"test_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "ApiTestingFramework")
                .Enrich.WithProperty("Category", categoryName);

            // Console sink
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            // File sink for text logging
            loggerConfiguration.WriteTo.File(
                logFilePath,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7);

            return loggerConfiguration.CreateLogger();
        }

        public static Serilog.ILogger CreateLoggerForType<T>()
        {
            return CreateLogger(typeof(T).Name);
        }
    }
}