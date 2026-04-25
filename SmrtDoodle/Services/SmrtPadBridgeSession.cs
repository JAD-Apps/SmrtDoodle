using SmrtAI.Core.Ipc;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace SmrtDoodle.Services;

/// <summary>
/// SmrtDoodle-side state for the SmrtPad bridge: opens the named-pipe client SmrtPad
/// passed on the launch URI, reads the optional source PNG, and hands the result PNG
/// back when the user finishes editing.
/// </summary>
internal static class SmrtPadBridgeSession
{
    private static NamedPipeClientStream? s_pipe;

    /// <summary>The PNG SmrtPad supplied as a starting image, or <c>null</c>.</summary>
    public static byte[]? IncomingImagePng { get; private set; }

    /// <summary><c>true</c> when SmrtDoodle was launched by SmrtPad and a result is expected.</summary>
    public static bool IsActive { get; private set; }

    /// <summary>Raised once the bridge is ready (after <see cref="Start"/> connects).</summary>
    public static event EventHandler? Ready;

    /// <summary>Connects to SmrtPad's named-pipe server identified by <paramref name="pipeName"/>.</summary>
    public static void Start(string pipeName)
    {
        IsActive = true;
        _ = ConnectAsync(pipeName);
    }

    private static async Task ConnectAsync(string pipeName)
    {
        try
        {
            var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await pipe.ConnectAsync(cts.Token).ConfigureAwait(false);

            var opening = await SmrtDoodleFrame.ReadAsync(pipe, cts.Token).ConfigureAwait(false);
            if (opening is { Command: SmrtDoodleIpc.CommandEditImage })
            {
                IncomingImagePng = SmrtDoodleFrame.Decode(opening.ImagePngBase64);
            }

            s_pipe = pipe;
            Ready?.Invoke(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Info($"SmrtPad bridge connect failed: {ex.Message}");
            IsActive = false;
        }
    }

    /// <summary>Sends the rendered PNG back to SmrtPad. Returns <c>true</c> on success.</summary>
    public static async Task<bool> SendImageAsync(byte[] png, CancellationToken ct = default)
    {
        if (s_pipe is null) return false;
        var msg = new SmrtDoodleImageMessage(
            Command: SmrtDoodleIpc.CommandImageReady,
            SchemaVersion: SmrtDoodleIpc.CurrentSchemaVersion,
            ImagePngBase64: SmrtDoodleFrame.Encode(png));
        try
        {
            await SmrtDoodleFrame.WriteAsync(s_pipe, msg, ct).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Notifies SmrtPad that the user closed without inserting a drawing.</summary>
    public static async Task SendCancelAsync(CancellationToken ct = default)
    {
        if (s_pipe is null) return;
        var msg = new SmrtDoodleImageMessage(
            Command: SmrtDoodleIpc.CommandCancelled,
            SchemaVersion: SmrtDoodleIpc.CurrentSchemaVersion);
        try { await SmrtDoodleFrame.WriteAsync(s_pipe, msg, ct).ConfigureAwait(false); }
        catch { /* SmrtPad may already be gone */ }
    }

    /// <summary>Closes the pipe; safe to call multiple times.</summary>
    public static void Close()
    {
        try { s_pipe?.Dispose(); } catch { /* best-effort */ }
        s_pipe = null;
        IsActive = false;
    }
}
