using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Functions;

internal static class ConfigurationFileAccessFunctions
{
    private const int DefaultReplacementRetryCount = 5;
    private const int DefaultReplacementRetryDelayMilliseconds = 25;

    /// <summary>Stocke un chemin relatif à la base lorsqu’il reste dans cette base, sinon conserve le chemin absolu.</summary>
    public static string? StorePath(string? path, string pathBase)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        var fullPath = Path.GetFullPath(path);
        var relative = Path.GetRelativePath(pathBase, fullPath);
        return IsInsideBase(relative) ? relative.Replace(Path.DirectorySeparatorChar, '/') : fullPath;
    }

    /// <summary>Résout un chemin stocké par rapport à sa base, sans changer les chemins déjà absolus.</summary>
    public static string? ResolvePath(string? path, string pathBase)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathFullyQualified(path)) return path;
        return Path.GetFullPath(Path.Combine(pathBase, path.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static bool IsInsideBase(string relative) =>
        !Path.IsPathFullyQualified(relative)
        && !string.Equals(relative, "..", StringComparison.Ordinal)
        && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
        && !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal);

    public static void WriteDocumentAtomically<T>(string path, T document,
        JsonSerializerOptions options, string temporarySuffix) =>
        WriteAtomicallyAsync(path, path + temporarySuffix, temporary =>
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(document, options));
            return Task.CompletedTask;
        }, static (source, target) => File.Move(source, target, overwrite: true)).GetAwaiter().GetResult();

    /// <summary>Sérialise les écritures avec le verrou fourni et le libère même si l’écriture échoue.</summary>
    /// <remarks>Le verrou est emprunté ; sa destruction reste à la charge de son propriétaire.</remarks>
    public static async Task WithSaveGateAsync(SemaphoreSlim saveGate, Func<Task> write,
        CancellationToken cancellationToken)
    {
        await saveGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await write().ConfigureAwait(false);
        }
        finally
        {
            saveGate.Release();
        }
    }

    internal static async Task WriteDocumentAtomicallyAsync<TConfiguration>(SemaphoreSlim saveGate,
        string directory, TConfiguration configuration, Func<TConfiguration, TConfiguration> prepareConfiguration,
        Func<TConfiguration, Guid> configurationId, Func<TConfiguration, TConfiguration> storePaths,
        JsonSerializerOptions options, CancellationToken cancellationToken)
    {
        await saveGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? temporary = null;
        try
        {
            configuration = prepareConfiguration(configuration);
            var machineDirectory = Path.Combine(directory, configurationId(configuration).ToString("N"));
            Directory.CreateDirectory(machineDirectory);
            var target = Path.Combine(machineDirectory, "machine.json");
            temporary = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
            await using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                await JsonSerializer.SerializeAsync(stream, storePaths(configuration), options, cancellationToken).ConfigureAwait(false);
            ReplaceFile(temporary, target);
        }
        finally
        {
            if (temporary is not null && File.Exists(temporary)) File.Delete(temporary);
            saveGate.Release();
        }
    }

    /// <summary>Écrit le document JSON dans un fichier temporaire avant de remplacer sa destination.</summary>
    public static async Task WriteDocumentAtomicallyAsync<T>(string path, T document,
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

    /// <summary>Supprime un dossier existant selon la politique de récursion choisie par le module.</summary>
    public static void DeleteDirectoryIfExists(string path, bool recursive)
    {
        if (Directory.Exists(path)) Directory.Delete(path, recursive);
    }

    internal static void DeleteIfExists(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }

    /// <summary>Énumère les documents existants des sous-dossiers, dans l’ordre ordinal insensible à la casse.</summary>
    public static IEnumerable<string> EnumerateDocumentPaths(string configurationDirectory, string fileName) =>
        Directory.EnumerateDirectories(configurationDirectory).Select(directory => Path.Combine(directory, fileName)).Where(File.Exists).Order(StringComparer.OrdinalIgnoreCase);

    /// <summary>Charge les documents sélectionnés et restaure leurs configurations avec les conversions du module.</summary>
    /// <remarks>Conserve la politique commune qui ignore les documents illisibles ou invalides.</remarks>
    public static Task<IReadOnlyList<TConfiguration>> ReadDocumentsAsync<TDocument, TConfiguration>(
        string configurationDirectory, Func<IEnumerable<string>> paths, Func<JsonElement, TDocument> deserialize,
        Func<TDocument, TConfiguration> restore, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(configurationDirectory);
        var configurations = new List<TConfiguration>();
        foreach (var path in paths())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var json = ReadAllText(path);
                using var parsed = JsonDocument.Parse(json);
                var document = deserialize(parsed.RootElement);
                configurations.Add(restore(document));
            }
            catch (JsonException) { }
            catch (IOException) { }
            catch (InvalidDataException) { }
            catch (ArgumentException) { }
        }
        return Task.FromResult<IReadOnlyList<TConfiguration>>(configurations);
    }

    /// <summary>Charge les configurations dont le schéma est accepté, puis résout leurs chemins.</summary>
    public static Task<IReadOnlyList<TConfiguration>> ReadDocumentsAsync<TConfiguration>(
        string configurationDirectory, string fileName, Func<string, TConfiguration?> deserialize,
        Func<TConfiguration, bool> isSupported, Func<TConfiguration, TConfiguration> resolve,
        CancellationToken cancellationToken) where TConfiguration : class
    {
        Directory.CreateDirectory(configurationDirectory);
        var configurations = new List<TConfiguration>();
        var paths = EnumerateDocumentPaths(configurationDirectory, fileName);
        foreach (var path in paths)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var json = ReadAllText(path);
                var configuration = deserialize(json);
                if (configuration is not null && isSupported(configuration))
                    configurations.Add(resolve(configuration));
            }
            catch (JsonException) { }
            catch (IOException) { }
        }
        return Task.FromResult<IReadOnlyList<TConfiguration>>(configurations);
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
