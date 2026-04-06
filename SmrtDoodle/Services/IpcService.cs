using Microsoft.Graphics.Canvas;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmrtDoodle.Services;

/// <summary>
/// Handles inter-process communication with SmrtPad.
/// Supports two IPC mechanisms:
/// 1. Temp file: SmrtDoodle writes PNG to temp path, SmrtPad polls for it (legacy).
/// 2. Named pipe: SmrtDoodle sends completion message via "SmrtDoodle_IPC" pipe (preferred).
/// Launch protocol: SmrtDoodle.exe --smrtpad --temp-file "C:\...\temp.png"
/// </summary>
public class IpcService
{
    private const string PipeName = "SmrtDoodle_IPC";
    private const int PipeTimeoutMs = 5000;

    public bool IsLaunchedFromSmrtPad { get; private set; }
    public string? TempFilePath { get; private set; }

    public void ParseArguments(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--smrtpad")
            {
                IsLaunchedFromSmrtPad = true;
            }
            else if (args[i] == "--temp-file" && i + 1 < args.Length)
            {
                TempFilePath = args[++i];
            }
        }
    }

    public string GetOrCreateTempFilePath()
    {
        if (!string.IsNullOrEmpty(TempFilePath)) return TempFilePath;
        var dir = Path.Combine(Path.GetTempPath(), "SmrtDoodle");
        Directory.CreateDirectory(dir);
        TempFilePath = Path.Combine(dir, $"smrtdoodle_{Guid.NewGuid():N}.png");
        return TempFilePath;
    }

    /// <summary>
    /// Notify SmrtPad that the image is ready via named pipe.
    /// Falls back silently if pipe server isn't running.
    /// </summary>
    public async Task<bool> NotifyImageReadyAsync(string filePath)
    {
        try
        {
            using var cts = new CancellationTokenSource(PipeTimeoutMs);
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            await client.ConnectAsync(PipeTimeoutMs, cts.Token);
            var message = Encoding.UTF8.GetBytes($"READY|{filePath}");
            await client.WriteAsync(message, cts.Token);
            await client.FlushAsync(cts.Token);
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    /// <summary>
    /// Start a named pipe server that waits for commands from SmrtPad.
    /// Runs in background until cancelled.
    /// </summary>
    public async Task<string?> WaitForCommandAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            await server.WaitForConnectionAsync(cancellationToken);
            var buffer = new byte[4096];
            int bytesRead = await server.ReadAsync(buffer, cancellationToken);
            return bytesRead > 0 ? Encoding.UTF8.GetString(buffer, 0, bytesRead) : null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public void CleanupTempFile()
    {
        if (TempFilePath != null && File.Exists(TempFilePath))
        {
            try { File.Delete(TempFilePath); } catch { }
        }
    }

    /// <summary>
    /// Clean up any orphaned temp files from previous sessions (older than 1 hour).
    /// Call this at app startup.
    /// </summary>
    public static void CleanupOrphanedTempFiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), "SmrtDoodle");
        if (!Directory.Exists(dir)) return;

        try
        {
            var cutoff = DateTime.UtcNow.AddHours(-1);
            foreach (var file in Directory.GetFiles(dir, "smrtdoodle_*.png"))
            {
                try
                {
                    if (File.GetLastWriteTimeUtc(file) < cutoff)
                        File.Delete(file);
                }
                catch { }
            }
        }
        catch { }
    }
}
