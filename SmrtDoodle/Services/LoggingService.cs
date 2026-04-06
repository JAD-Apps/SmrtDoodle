using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SmrtDoodle.Services;

/// <summary>
/// Lightweight file-based logging service for crash diagnostics and telemetry.
/// Logs are written to the app's local data folder.
/// </summary>
public sealed class LoggingService : IDisposable
{
    private static readonly Lazy<LoggingService> _instance = new(() => new LoggingService());
    public static LoggingService Instance => _instance.Value;

    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly object _lock = new();
    private bool _disposed;

    public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

    private LoggingService()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmrtDoodle", "Logs");
        Directory.CreateDirectory(_logDirectory);

        // One log file per day, auto-cleanup old logs
        _logFilePath = Path.Combine(_logDirectory, $"smrtdoodle_{DateTime.Now:yyyyMMdd}.log");
        CleanupOldLogs(maxAgeDays: 14);
    }

    public void Debug(string message, [CallerMemberName] string? caller = null)
        => Write(LogLevel.Debug, message, caller);

    public void Info(string message, [CallerMemberName] string? caller = null)
        => Write(LogLevel.Info, message, caller);

    public void Warning(string message, [CallerMemberName] string? caller = null)
        => Write(LogLevel.Warning, message, caller);

    public void Error(string message, Exception? ex = null, [CallerMemberName] string? caller = null)
    {
        var msg = ex != null ? $"{message} | {ex.GetType().Name}: {ex.Message}" : message;
        Write(LogLevel.Error, msg, caller);
    }

    public void Fatal(string message, Exception? ex = null, [CallerMemberName] string? caller = null)
    {
        var msg = ex != null ? $"{message} | {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}" : message;
        Write(LogLevel.Fatal, msg, caller);
    }

    private void Write(LogLevel level, string message, string? caller)
    {
        if (_disposed || level < MinimumLevel) return;

        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level,-7}] [{caller ?? "?"}] {message}";

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
            catch
            {
                // Logging should never crash the app
            }
        }
    }

    private void CleanupOldLogs(int maxAgeDays)
    {
        try
        {
            var cutoff = DateTime.Now.AddDays(-maxAgeDays);
            foreach (var file in Directory.GetFiles(_logDirectory, "smrtdoodle_*.log"))
            {
                if (File.GetCreationTime(file) < cutoff)
                    File.Delete(file);
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    public void Dispose()
    {
        _disposed = true;
    }
}

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Fatal = 4
}
