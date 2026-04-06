using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace SmrtDoodle.Helpers;

/// <summary>
/// Runs CPU-intensive image operations on a background thread with progress reporting
/// and cancellation support. Uses DispatcherQueue for UI-safe progress callbacks.
/// </summary>
public static class BackgroundOperation
{
    /// <summary>
    /// Execute an operation on a background thread. Progress is marshalled to the UI thread.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    /// <param name="operation">The work to execute off-thread.</param>
    /// <param name="dispatcherQueue">The UI dispatcher for progress callbacks.</param>
    /// <param name="onProgress">Optional UI-thread progress callback (0-100).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<T> RunAsync<T>(
        Func<IProgress<double>, CancellationToken, T> operation,
        DispatcherQueue? dispatcherQueue = null,
        Action<double>? onProgress = null,
        CancellationToken ct = default)
    {
        IProgress<double>? progress = null;
        if (onProgress != null && dispatcherQueue != null)
        {
            progress = new Progress<double>(value =>
            {
                dispatcherQueue.TryEnqueue(() => onProgress(value));
            });
        }
        else if (onProgress != null)
        {
            progress = new Progress<double>(onProgress);
        }

        return await Task.Run(() => operation(progress ?? new Progress<double>(), ct), ct);
    }

    /// <summary>
    /// Execute an async operation on a background thread.
    /// </summary>
    public static async Task<T> RunAsync<T>(
        Func<IProgress<double>, CancellationToken, Task<T>> operation,
        DispatcherQueue? dispatcherQueue = null,
        Action<double>? onProgress = null,
        CancellationToken ct = default)
    {
        IProgress<double>? progress = null;
        if (onProgress != null && dispatcherQueue != null)
        {
            progress = new Progress<double>(value =>
            {
                dispatcherQueue.TryEnqueue(() => onProgress(value));
            });
        }
        else if (onProgress != null)
        {
            progress = new Progress<double>(onProgress);
        }

        return await Task.Run(() => operation(progress ?? new Progress<double>(), ct), ct);
    }
}

/// <summary>
/// Monitors approximate GPU/managed memory usage and provides
/// a warning threshold check for large canvas operations.
/// </summary>
public static class MemoryMonitor
{
    /// <summary>
    /// Estimate memory required for a canvas of given dimensions with the specified layer count.
    /// Each layer = width * height * 4 bytes (RGBA). Undo buffer adds ~2x.
    /// </summary>
    /// <param name="width">Canvas width in pixels.</param>
    /// <param name="height">Canvas height in pixels.</param>
    /// <param name="layerCount">Number of layers.</param>
    /// <returns>Estimated bytes required.</returns>
    public static long EstimateMemoryBytes(int width, int height, int layerCount)
    {
        long bytesPerLayer = (long)width * height * 4;
        long layerMemory = bytesPerLayer * layerCount;
        long undoMemory = bytesPerLayer * 2; // estimate 2 undo snapshots
        return layerMemory + undoMemory;
    }

    /// <summary>
    /// Check if the current process is approaching memory limits.
    /// Returns true if available memory is below the threshold.
    /// </summary>
    /// <param name="warningThresholdMb">Threshold in megabytes. Default 512 MB remaining.</param>
    public static bool IsMemoryLow(long warningThresholdMb = 512)
    {
        var currentBytes = GC.GetTotalMemory(forceFullCollection: false);
        // For 64-bit, we use a practical limit rather than actual physical memory
        const long practicalLimitBytes = 4L * 1024 * 1024 * 1024; // 4 GB practical limit
        return currentBytes > practicalLimitBytes - (warningThresholdMb * 1024 * 1024);
    }

    /// <summary>
    /// Get current managed memory usage in megabytes.
    /// </summary>
    public static double GetManagedMemoryMb()
    {
        return GC.GetTotalMemory(forceFullCollection: false) / (1024.0 * 1024.0);
    }
}
