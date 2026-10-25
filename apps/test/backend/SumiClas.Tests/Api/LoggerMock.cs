namespace SumiClas.Tests.Api;

using Microsoft.Extensions.Logging;
using Moq;

public static class LoggerMock
{
    public static Mock<TLogger> Enabled<TLogger>()
        where TLogger : class, ILogger
    {
        Mock<TLogger> logger = new();
        logger.Setup(target => target.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return logger;
    }

    public static ILoggerProvider ProviderOf(ILogger logger)
    {
        Mock<ILoggerProvider> provider = new();
        provider.Setup(target => target.CreateLogger(It.IsAny<string>())).Returns(logger);
        return provider.Object;
    }

    public static void ShouldHaveLoggedTheOnlyError<TLogger>(
        this Mock<TLogger> logger,
        Exception exception,
        string messagePart)
        where TLogger : class, ILogger
    {
        logger.Verify(
            target => target.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        logger.Verify(
            target => target.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(messagePart)),
                It.Is<Exception?>(cause => ReferenceEquals(cause, exception)),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public static void ShouldHaveLoggedAnErrorCausedBy<TLogger, TException>(this Mock<TLogger> logger)
        where TLogger : class, ILogger
        where TException : Exception
        => logger.Verify(
            target => target.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<TException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
}
