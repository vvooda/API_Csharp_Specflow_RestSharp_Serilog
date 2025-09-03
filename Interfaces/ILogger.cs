using System;

namespace ApiTestingFramework.Interfaces
{
    public interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(Exception exception, string? message = null); // Add ? for nullable
        void TestStep(string message);
        void TestResult(string message, bool isSuccess);
        void ApiRequest(string method, string url, string? body = null); // Add ? for nullable
        void ApiResponse(int statusCode, string content);
    }
}