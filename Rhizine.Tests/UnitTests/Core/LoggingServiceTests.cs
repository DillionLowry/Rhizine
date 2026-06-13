using Rhizine.Core.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using Xunit;

namespace Rhizine.Tests.UnitTests.Core.Services
{
    public class LoggingServiceTests
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly LoggingService _loggingService;

        public LoggingServiceTests()
        {
            _logger = Substitute.For<ILogger<LoggingService>>();
            _loggingService = new LoggingService(_logger);
        }

        [Fact]
        public void LogDebug_ShouldLogDebugMessage()
        {
            // Arrange
            var message = "Debug message";

            // Act
            _loggingService.LogDebug(message);

            // Assert
            _logger.Received().Log(LogLevel.Debug, 0, Arg.Is<object>(o => o.ToString() == message), null, Arg.Any<Func<object, Exception, string>>());
        }

        // Repeat similar tests for LogInformation, LogWarning, LogError

        [Fact]
        public void LogError_ShouldLogErrorMessageWithException()
        {
            // Arrange
            var message = "Error message";
            var exception = new Exception("Test exception");

            // Act
            _loggingService.LogError(exception, message);

            // Assert
            _logger.Received().Log(LogLevel.Error, 0, Arg.Is<object>(o => o.ToString() == message), exception, Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LogPerformance_ShouldLogPerformanceMessage()
        {
            // Arrange
            var actionName = "TestAction";
            Action action = () => { /* Some action */ };

            // Act
            _loggingService.LogPerformance(action, actionName);

            // Assert
            _logger.Received().Log(LogLevel.Information, 0, Arg.Is<object>(o => o.ToString().StartsWith(actionName)), null, Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void HandleGlobalException_ShouldLogErrorMessageWithException()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            _loggingService.HandleGlobalException(exception);

            // Assert
            _logger.Received().Log(LogLevel.Error, 0, Arg.Is<object>(o => o.ToString() == "An unexpected error has occured."), exception, Arg.Any<Func<object, Exception, string>>());
        }
    }
}
