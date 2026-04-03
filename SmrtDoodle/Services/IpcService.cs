using Microsoft.Graphics.Canvas;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace SmrtDoodle.Services;

/// <summary>
/// Handles inter-process communication with SmrtPad.
/// Launch protocol: SmrtDoodle.exe --smrtpad --temp-file "C:\...\temp.png"
/// On close/insert, SmrtDoodle writes the PNG to the temp path and exits with code 0.
/// SmrtPad polls or watches for the temp file then inserts it.
/// </summary>
public class IpcService
{
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

    public void CleanupTempFile()
    {
        if (TempFilePath != null && File.Exists(TempFilePath))
        {
            try { File.Delete(TempFilePath); } catch { }
        }
    }
}
