using System;
using Serilog;
using ApiTestingFramework.Interfaces;

namespace ApiTestingFramework.Implementations
{
    public class SerilogLogger : ILogger
    {
        private readonly Serilog.ILogger _logger;

        public SerilogLogger(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Info(string message)
        {
            _logger.Information(message);
        }

        public void Warn(string message)
        {
            _logger.Warning(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(Exception exception, string? message = null) // Add ? for nullable
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.Error(exception, "An error occurred");
            }
            else
            {
                _logger.Error(exception, message);
            }
        }

        public void TestStep(string message)
        {
            _logger.Information($"🔹 {message}");
        }

        public void TestResult(string message, bool isSuccess)
        {
            var emoji = isSuccess ? "✅" : "❌";
            if (isSuccess)
            {
                _logger.Information($"{emoji} {message}");
            }
            else
            {
                _logger.Error($"{emoji} {message}");
            }
        }

        public void ApiRequest(string method, string url, string? body = null) // Add ? for nullable
        {
            _logger.Information("🌐 {Method} {Url}", method, url);
            
            if (!string.IsNullOrEmpty(body))
            {
                _logger.Debug("Request Body: {RequestBody}", body);
            }
        }

        public void ApiResponse(int statusCode, string content)
        {
            if (statusCode >= 200 && statusCode < 300)
            {
                _logger.Information("📥 Response: Status {StatusCode}", statusCode);
            }
            else
            {
                _logger.Error("📥 Response: Status {StatusCode}", statusCode);
            }
            
            if (!string.IsNullOrEmpty(content))
            {
                _logger.Debug("Response Body: {ResponseContent}", content);
            }
        }
    }
}