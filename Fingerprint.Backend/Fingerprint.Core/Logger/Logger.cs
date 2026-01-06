using System.Diagnostics;
using System.Text.Json;
using Fingerprint.Core.DTOs;
using Microsoft.Extensions.Logging;

namespace Fingerprint.Core.Logger;

public class Logger(string category) : ILogger
{
    private object FormatError(Exception ex)
    {
        var trace = new StackTrace(ex, true).GetFrames();

        StackFrameInfo[] frames = null;

        if (trace != null)
        {
            frames = new StackFrameInfo[trace.Length];

            for (int i = 0; i < trace.Length; i++)
            {
                var f = trace[i];
                
                var file = f.GetFileName();

                frames[i] = new StackFrameInfo()
                {
                    Func = f.GetMethod()?.Name,
                    Source = file != null ? Path.GetFileName(file) : "Unknown",
                    Line = f.GetFileLineNumber(),
                };
            }
        }

        return new ErrorInfo()
        {
            Message = ex.Message,
            Trace = frames
        };
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var payload = new
        {
            level = logLevel.ToString(),
            category = category,
            msg = formatter(state, exception),
            error = exception != null ? FormatError(exception) : null,
            time = DateTime.UtcNow.ToString("o")
        };

        string json = JsonSerializer.Serialize(payload);
        Console.WriteLine(json);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
}