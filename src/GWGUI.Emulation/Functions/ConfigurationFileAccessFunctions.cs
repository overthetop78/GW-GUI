using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Functions;

public static class ConfigurationFileAccessFunctions
{
    private const int DefaultReplacementRetryCount = 5;
    private const int DefaultReplacementRetryDelayMilliseconds = 25;

    internal static string? StorePath(string? path, string pathBase)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        var fullPath = Path.GetFullPath(path);
        var relative = Path.GetRelativePath(pathBase, fullPath);
        return IsInsideBase(relative) ? relative.Replace(Path.DirectorySeparatorChar, '/') : fullPath;
    }

    internal static string? ResolvePath(string? path, string pathBase)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathFullyQualified(path)) return path;
        return Path.GetFullPath(Path.Combine(pathBase, path.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static bool IsInsideBase(string relative) =>
        !Path.IsPathFullyQualified(relative)
        && !string.Equals(relative, "..", StringComparison.Ordinal)
        && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
        && !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal);

    internal static async Task WriteDocumentAtomicallyAsync<T>(string path, T document,
        JsonSerializerOptions options, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        var temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        await WriteAtomicallyAsync(fullPath, temporaryPath, async temporary =>
        {
            await using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write,
                             FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, document, options, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    internal static async Task WriteAtomicallyAsync(string target, string temporary, Func<string, Task> write,
        Action<string, string>? replace = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        try
        {
            await write(temporary).ConfigureAwait(false);
            if (replace is null) ReplaceFile(temporary, target);
            else replace(temporary, target);
        }
        finally { DeleteIfExists(temporary); }
    }

    internal static void DeleteIfExists(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }

    public static string ReadAllText(string path) => WithFileLock(path, () => File.ReadAllText(path));

    public static void ReplaceFile(string source, string target, int retryCount = DefaultReplacementRetryCount, int retryDelayMilliseconds = DefaultReplacementRetryDelayMilliseconds) =>
        WithFileLock(target, () =>
        {
            for (var attempt = 0; attempt < retryCount; attempt++)
            {
                try
                {
                    File.Move(source, target, true);
                    return;
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    if (attempt + 1 >= retryCount) break;
                    Thread.Sleep(retryDelayMilliseconds * (attempt + 1));
                }
            }

            File.Copy(source, target, true);
            File.Delete(source);
        });

    private static T WithFileLock<T>(string path, Func<T> action)
    {
        using var mutex = CreateMutex(path);
        var lockTaken = Wait(mutex);
        try { return action(); }
        finally { if (lockTaken) mutex.ReleaseMutex(); }
    }

    private static void WithFileLock(string path, Action action) =>
        WithFileLock(path, () =>
        {
            action();
            return true;
        });

    private static Mutex CreateMutex(string path)
    {
        var normalizedPath = Path.GetFullPath(path).ToUpperInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedPath));
        return new Mutex(false, @"Local\GWGUI.ConfigurationFile." + Convert.ToHexString(hash));
    }

    private static bool Wait(Mutex mutex)
    {
        try { mutex.WaitOne(); }
        catch (AbandonedMutexException) { }
        return true;
    }
}
