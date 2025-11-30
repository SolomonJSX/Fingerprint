using Microsoft.Extensions.Logging;

namespace Fingerprint.Alg.Providers;

public class LoggerProvider : ILoggerProvider
{
    public void Dispose() { }

    public ILogger CreateLogger(string categoryName) => new Logger.Logger(categoryName);
}