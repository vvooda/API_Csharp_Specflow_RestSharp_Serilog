using System;
using TechTalk.SpecFlow;
using Microsoft.Extensions.DependencyInjection;
using ApiTestingFramework.Interfaces;
using ApiTestingFramework.Implementations;
using Serilog;

namespace ApiTestingFramework.Hooks
{
    [Binding]
    public class TestHooks
    {
        private readonly ScenarioContext _scenarioContext;

        public TestHooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Initialize Serilog globally
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo.File("logs/test.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            
            Log.Information("🚀 Test run started");
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            Log.Information("🏁 Test run completed");
            Log.CloseAndFlush();
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            var services = new ServiceCollection();
            
            // Create Serilog logger and wrap it in our ILogger
            var serilogLogger = Log.Logger.ForContext("Scenario", _scenarioContext.ScenarioInfo.Title);
            var logger = new SerilogLogger(serilogLogger);
            
            // Register dependencies
            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<IApiClient, RestSharpClientService>();
            services.AddSingleton<IResponseHandler, RestSharpResponseHandler>();
            services.AddSingleton<IJsonExtractor, JsonExtractor>();
           // services.AddSingleton<ITestReporter, HtmlTestReporter>();
            
            var serviceProvider = services.BuildServiceProvider();
            _scenarioContext.Set(serviceProvider, "ServiceProvider");
            _scenarioContext.Set(logger, "Logger");

            logger.TestStep($"Starting scenario: {_scenarioContext.ScenarioInfo.Title}");
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var logger = _scenarioContext.Get<ILogger>("Logger");
            
            // FIX: Check if TestError is null instead of using ! operator
            var isSuccess = _scenarioContext.TestError == null;
            var errorMessage = _scenarioContext.TestError?.Message;

            logger.TestResult($"Scenario completed: {_scenarioContext.ScenarioInfo.Title}", isSuccess);
            
            if (!isSuccess)
            {
                logger.Error($"Scenario failed: {errorMessage}");
            }

            // Cleanup
            var serviceProvider = _scenarioContext.Get<IServiceProvider>("ServiceProvider");
            var apiClient = serviceProvider.GetService<IApiClient>();
            
            if (apiClient is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}