using System.Security.Cryptography;
using System.Text;

namespace GWGUI.Emulation.Functions;

public static class ConfigurationFileAccessFunctions
{
    private const int DefaultReplacementRetryCount = 5;
    private const int DefaultReplacementRetryDelayMilliseconds = 25;

    public static string ReadAllText(string path) => WithFileLock(path, () => File.ReadAllText(path));

    public static void ReplaceFile(string source, string target,
        int retryCount = DefaultReplacementRetryCount,
        int retryDelayMilliseconds = DefaultReplacementRetryDelayMilliseconds) =>
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