using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

namespace SmrtDoodle.Services;

/// <summary>
/// Tracks recently opened files and persists across sessions via local settings.
/// </summary>
public class RecentFilesService
{
    private const string SettingsKey = "RecentFiles";
    private const int MaxRecentFiles = 10;
    private readonly List<string> _recentFiles = new();

    public IReadOnlyList<string> RecentFiles => _recentFiles;

    public RecentFilesService()
    {
        Load();
    }

    /// <summary>Add a file path to the recent files list.</summary>
    public void AddFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        // Remove if already present, then insert at top
        _recentFiles.RemoveAll(f => string.Equals(f, filePath, StringComparison.OrdinalIgnoreCase));
        _recentFiles.Insert(0, filePath);

        // Trim to max
        while (_recentFiles.Count > MaxRecentFiles)
            _recentFiles.RemoveAt(_recentFiles.Count - 1);

        Save();
    }

    /// <summary>Clear all recent files.</summary>
    public void Clear()
    {
        _recentFiles.Clear();
        Save();
    }

    private void Load()
    {
        try
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.TryGetValue(SettingsKey, out var value) && value is string joined)
            {
                _recentFiles.Clear();
                _recentFiles.AddRange(joined.Split('|', StringSplitOptions.RemoveEmptyEntries));
            }
        }
        catch
        {
            // Ignore if settings not available (e.g., unpackaged)
        }
    }

    private void Save()
    {
        try
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values[SettingsKey] = string.Join('|', _recentFiles);
        }
        catch
        {
            // Ignore
        }
    }
}
