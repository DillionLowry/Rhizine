using Rhizine.Core.Models;
using Rhizine.Core.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using Xunit;

// NOTE: not working
namespace Rhizine.Tests.UnitTests.Core.Services
{
    public class AsyncLoggingServiceTests
    {
        private readonly ILogger<AsyncLoggingService> _logger;
        private readonly AsyncLoggingService _loggingService;

        public AsyncLoggingServiceTests()
        {
            _logger = Substitute.For<ILogger<AsyncLoggingService>>();
            _loggingService = new AsyncLoggingService(_logger);
        }

        [Fact]
        public async Task LogDebugAsync_ShouldLogDebugMessage()
        {
            // Arrange
            var message = "Debug message";

            // Act
            await _loggingService.LogDebugAsync(message);

            // Assert
            _logger.DidNotReceiveWithAnyArgs().Log<object>(default, default, default, default, default);
            _logger.ReceivedWithAnyArgs().Log<object>(default, default, default, default, default);

        }

        [Fact]
        public async Task LogInformationAsync_ShouldLogInformationMessage()
        {
            // Arrange
            var message = "Information message";

            // Act
            await _loggingService.LogInformationAsync(message);

            // Assert
            _logger.Received().Log(LogLevel.Information, 2, Arg.Is<object>(o => o.ToString() == message), null, Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task LogWarningAsync_ShouldLogWarningMessage()
        {
            // Arrange
            var message = "Warning message";

            // Act
            await _loggingService.LogWarningAsync(message);

            // Assert
            _logger.Received().Log(LogLevel.Warning, 3, Arg.Is<object>(o => o.ToString() == message), null, Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task LogErrorAsync_ShouldLogErrorMessage()
        {
            // Arrange
            var message = "Error message";

            // Act
            await _loggingService.LogErrorAsync(message);

            // Assert
            _logger.Received().Log(LogLevel.Error, 4, Arg.Is<object>(o => o.ToString() == message), null, Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task LogErrorAsync_ShouldLogErrorMessageWithException()
        {
            // Arrange
            var message = "Error message";
            var exception = new Exception("Test exception");

            // Act
            await _loggingService.LogErrorAsync(exception, message);

            // Assert
            _logger.Received().Log(LogLevel.Error, 6, Arg.Is<object>(o => o.ToString() == message), exception, Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task LogPerformanceAsync_ShouldLogPerformanceMessage()
        {
            // Arrange
            var actionName = "TestAction";
            Action action = () => { /* Some action */ };

            // Act
            await _loggingService.LogPerformanceAsync(action, actionName);

            // Assert
            _logger.Received().Log(LogLevel.Debug, 10, Arg.Is<object>(o => o.ToString().StartsWith(actionName)), null, Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void HandleGlobalException_ShouldLogErrorMessageWithException()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            _loggingService.HandleGlobalException(exception);

            // Assert
            _logger.Received().Log(LogLevel.Error, 4, Arg.Is<object>(o => o.ToString() == "An unexpected error has occured."), exception, Arg.Any<Func<object, Exception, string>>());
        }
    }
}