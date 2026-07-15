using System.Diagnostics;
using Microsoft.Win32;

namespace TorProxy;

/// <summary>
/// Toggles "start with Windows" via the per-user Run key (no admin required).
/// </summary>
public static class AutostartRegistry
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "TorProxy";

    private static string ExePath =>
        Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            var value = key?.GetValue(ValueName) as string;
            return !string.IsNullOrEmpty(value);
        }
        catch { return false; }
    }

    public static void Set(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (key is null)
            return;
        if (enabled)
        {
            string path = ExePath;
            if (!string.IsNullOrEmpty(path))
                key.SetValue(ValueName, $"\"{path}\"");
        }
        else if (key.GetValue(ValueName) is not null)
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }
}
