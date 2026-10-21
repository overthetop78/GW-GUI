using GWGUI.Emulation.Interfaces;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Functions;

/// <summary>Lecture, écriture et vérification des sauvegardes partagées par les modules indépendants.</summary>
/// <remarks>Chaque module conserve son marqueur, son schéma et ses règles de compatibilité. Ces opérations ne modifient pas les formats existants.</remarks>
public static class EmulationStateFileFunctions
{
    /// <summary>Vérifie l’émulateur, le nom, la version et l’empreinte du cœur avant restauration.</summary>
    public static bool CoreIdentityMatches<TConfiguration, TOption>(bool sameEmulator,
        string? name, string? version, string? sha256, IEmulationCoreRuntime<TConfiguration, TOption> core) =>
        sameEmulator
        && string.Equals(name, core.CoreName, StringComparison.Ordinal)
        && string.Equals(version, core.CoreVersion, StringComparison.Ordinal)
        && string.Equals(sha256, core.CoreSha256, StringComparison.OrdinalIgnoreCase);
    /// <summary>Compare les identités par clé et valeur ; une identité absente équivaut à un dictionnaire vide.</summary>
    public static bool IdentityValuesEqual(IReadOnlyDictionary<string, string>? left, IReadOnlyDictionary<string, string> right)
    {
        if ((left?.Count ?? 0) != right.Count) return false;
        return left is null ? right.Count == 0 : left.All(pair => right.TryGetValue(pair.Key, out var value) && value == pair.Value);
    }

    /// <summary>Écrit une sauvegarde temporaire, force ses données sur disque puis remplace sa destination.</summary>
    /// <remarks>Le module choisit le suffixe temporaire et la politique File.Replace ; le fichier temporaire est supprimé dans finally, même après échec.</remarks>
    public static void WriteFile<THeader>(string path, THeader header, ReadOnlySpan<byte> state, ReadOnlySpan<byte> magic,
        string temporarySuffix, JsonSerializerOptions options, bool useFileReplace)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + temporarySuffix;
        var headerBytes = JsonSerializer.SerializeToUtf8Bytes(header, options);
        try
        {
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                Write(stream, magic, headerBytes, state);
            if (useFileReplace && File.Exists(fullPath)) File.Replace(temporary, fullPath, destinationBackupFileName: null);
            else File.Move(temporary, fullPath, overwrite: !useFileReplace);
        }
        finally { ConfigurationFileAccessFunctions.DeleteIfExists(temporary); }
    }

    /// <summary>Écrit marqueur, longueur de l’enveloppe, enveloppe et état dans le flux emprunté, puis force sa vidange.</summary>
    /// <remarks>Le flux reste ouvert et sous la responsabilité de l’appelant.</remarks>
    public static void Write(FileStream stream, ReadOnlySpan<byte> magic, ReadOnlySpan<byte> header, ReadOnlySpan<byte> state)
    {
        stream.Write(magic);
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(length, header.Length);
        stream.Write(length);
        stream.Write(header);
        stream.Write(state);
        stream.Flush(flushToDisk: true);
    }

    /// <summary>Ouvre une sauvegarde, valide son contenu et applique la politique de reprises fournie en cas d’erreur d’entrée-sortie.</summary>
    /// <remarks>Les flux ouverts ici sont libérés ; le résultat contient son propre tableau d’état. Les erreurs de format ne sont pas transformées en reprises d’entrée-sortie.</remarks>
    public static (THeader Header, byte[] State) ReadFile<THeader>(string path, ReadOnlySpan<byte> expectedMagic,
        int maximumHeaderLength, JsonSerializerOptions options, Func<THeader, string> stateHash, Func<Exception> invalid,
        Func<THeader, bool>? validHeader, Action<int>? validatePayload, int retryCount, int retryDelayMilliseconds)
        where THeader : class
    {
        for (var retry = 0; retry < retryCount; retry++)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                return Read(stream, expectedMagic, maximumHeaderLength, options, stateHash, invalid, validHeader, validatePayload);
            }
            catch (IOException) when (retry + 1 < retryCount)
            {
                Thread.Sleep(retryDelayMilliseconds);
            }
        }
        throw new FileNotFoundException(path);
    }

    /// <summary>Valide marqueur, longueur, enveloppe et empreinte avant de renvoyer les données d’état.</summary>
    /// <remarks>Le flux est emprunté et reste ouvert. Les validations propres au module sont appelées avant acceptation du résultat.</remarks>
    public static (THeader Header, byte[] State) Read<THeader>(Stream stream, ReadOnlySpan<byte> expectedMagic,
        int maximumHeaderLength, JsonSerializerOptions options, Func<THeader, string> stateHash, Func<Exception> invalid,
        Func<THeader, bool>? validHeader = null, Action<int>? validatePayload = null) where THeader : class
    {
        Span<byte> magic = stackalloc byte[expectedMagic.Length];
        stream.ReadExactly(magic);
        if (!magic.SequenceEqual(expectedMagic)) throw invalid();
        Span<byte> lengthBytes = stackalloc byte[sizeof(int)];
        stream.ReadExactly(lengthBytes);
        var length = BinaryPrimitives.ReadInt32LittleEndian(lengthBytes);
        if (length <= 0 || length > maximumHeaderLength) throw invalid();
        var headerBytes = GC.AllocateUninitializedArray<byte>(length);
        stream.ReadExactly(headerBytes);
        var header = JsonSerializer.Deserialize<THeader>(headerBytes, options);
        if (header is null || validHeader is not null && !validHeader(header)) throw invalid();
        using var payload = new MemoryStream();
        stream.CopyTo(payload);
        var state = payload.ToArray();
        validatePayload?.Invoke(state.Length);
        if (!string.Equals(stateHash(header), HashBytes(state), StringComparison.OrdinalIgnoreCase)) throw invalid();
        return (header, state);
    }

    /// <summary>Calcule l’empreinte SHA-256 hexadécimale des octets empruntés.</summary>
    public static string HashBytes(ReadOnlySpan<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    /// <summary>Calcule l’empreinte d’un fichier ou d’un dossier en conservant le tri et la représentation des chemins du module.</summary>
    /// <remarks>Pour un dossier, les noms relatifs et leurs longueurs participent à l’empreinte avant les octets des fichiers. Les flux sont possédés et libérés par cette opération.</remarks>
    public static string HashPath(string path, string pattern, char separator, int bufferSize, Func<Exception> missing)
    {
        if (File.Exists(path)) return EmulationCoreDiagnosticFunctions.CalculateSha256(path);
        if (!Directory.Exists(path)) throw missing();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> length = stackalloc byte[sizeof(int)];
        foreach (var file in Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories).Order(StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(path, file).Replace(Path.DirectorySeparatorChar, separator);
            var name = Encoding.UTF8.GetBytes(relative);
            BinaryPrimitives.WriteInt32LittleEndian(length, name.Length);
            hash.AppendData(length);
            hash.AppendData(name);
            using var stream = File.OpenRead(file);
            var buffer = new byte[bufferSize];
            int read;
            while ((read = stream.Read(buffer)) > 0) hash.AppendData(buffer.AsSpan(0, read));
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>Trie les associations d’entrée par clé ordinale pour produire des empreintes reproductibles.</summary>
    public static IOrderedEnumerable<KeyValuePair<string, TValue>> OrderInputMappings<TValue>(
        IReadOnlyDictionary<string, TValue>? mappings) =>
        (mappings ?? new Dictionary<string, TValue>()).OrderBy(pair => pair.Key, StringComparer.Ordinal);

    /// <summary>Vérifie les champs obligatoires de l’enveloppe et les valeurs des enums du module.</summary>
    /// <remarks>Le module doit aussi vérifier la version exacte et la correspondance des identités avec la machine courante.</remarks>
    public static bool IsHeaderValid<TModel, TCore>(EmulationSavedStateHeader<TModel, TCore> header)
        where TModel : struct, Enum
        where TCore : struct, Enum =>
        header.FormatVersion > 0
        && Enum.IsDefined(header.Core)
        && Enum.IsDefined(header.Model)
        && !string.IsNullOrWhiteSpace(header.CoreName)
        && !string.IsNullOrWhiteSpace(header.CoreVersion)
        && !string.IsNullOrWhiteSpace(header.CoreSha256)
        && !string.IsNullOrWhiteSpace(header.ConfigurationSha256)
        && !string.IsNullOrWhiteSpace(header.ContentSha256)
        && !string.IsNullOrWhiteSpace(header.StateSha256);
}
