using System.IO;
using System.Reflection;

namespace TorProxy;

/// <summary>
/// Extracts the embedded Tor binaries (tor.exe, lyrebird.exe, hashes.txt) next
/// to the app data on first run, so the whole tool ships as a single .exe.
/// Extraction is idempotent: a file is rewritten only when missing or a
/// different size, and a locked file (a previous tor still running) is left as-is.
/// </summary>
public static class BinaryExtractor
{
    public sealed record Result(string TorPath, string LyrebirdPath);

    public static Result EnsureBinaries(string binDir)
    {
        Directory.CreateDirectory(binDir);
        string tor = Extract("tor.exe", binDir, required: true)!;
        string lyre = Extract("lyrebird.exe", binDir, required: true)!;
        // Optional: lets DpiBypass.Tor verify tor.exe against its pinned SHA-256.
        Extract("hashes.txt", binDir, required: false);
        return new Result(tor, lyre);
    }

    private static string? Extract(string resourceName, string dir, bool required)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var res = asm.GetManifestResourceStream(resourceName);
        if (res is null)
        {
            if (required)
                throw new InvalidOperationException(
                    $"Встроенный файл «{resourceName}» не найден в сборке. " +
                    "Соберите приложение с заполненной папкой third_party\\tor.");
            return null;
        }

        string target = Path.Combine(dir, resourceName);
        if (File.Exists(target) && new FileInfo(target).Length == res.Length)
            return target; // already the right file

        try
        {
            using var fs = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None);
            res.CopyTo(fs);
        }
        catch (IOException)
        {
            // Target is locked (e.g. tor.exe running from a previous session).
            // The on-disk copy is the same binary, so this is safe to ignore.
        }
        return target;
    }
}
