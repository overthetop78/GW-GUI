using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Functions;

/// <summary>Lecture, écriture et vérification des sauvegardes partagées par les modules indépendants.</summary>
/// <remarks>WriteState et ReadState appliquent le format commun à tous les modules. Les projections d’identité et de contenu restent définies par le modèle émulé ; aucun ancien format de famille n’est sélectionné.</remarks>
public static class EmulationStateFileFunctions
{
    private static readonly JsonSerializerOptions StateJsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Écrit une sauvegarde dans le format unique du SDK, après contrôle de son en-tête et de ses octets.</summary>
    public static void WriteState<TModel, TCore>(string path, EmulationSavedStateHeader<TModel, TCore> header,
        ReadOnlySpan<byte> state)
    {
        if (!IsHeaderValid(header) || header.FormatVersion != EmulationSavedStateConstants.CurrentFormatVersion
            || state.IsEmpty || !string.Equals(header.StateSha256, HashBytes(state), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(nameof(EmulationSavedStateHeader<TModel, TCore>));
        WriteFile(path, header, state, EmulationSavedStateConstants.Magic, "." + Guid.NewGuid().ToString("N") + ".tmp",
            StateJsonOptions, useFileReplace: true);
    }

    /// <summary>Lit exclusivement le format commun et vérifie l’en-tête, la version et l’intégrité de l’état.</summary>
    /// <remarks>Le module fournit son erreur localisée. Les anciens marqueurs et les en-têtes incomplets sont rejetés.</remarks>
    public static (EmulationSavedStateHeader<TModel, TCore> Header, byte[] State) ReadState<TModel, TCore>(
        string path, Func<Exception> invalid)
    {
        try
        {
            return ReadFile<EmulationSavedStateHeader<TModel, TCore>>(path, EmulationSavedStateConstants.Magic,
                EmulationSavedStateConstants.MaximumHeaderLength, StateJsonOptions, static header => header.StateSha256,
                invalid, static header => header.FormatVersion == EmulationSavedStateConstants.CurrentFormatVersion
                    && IsHeaderValid(header), length => { if (length <= 0 || length > Array.MaxLength) throw invalid(); },
                EmulationSavedStateConstants.ReadRetryCount, EmulationSavedStateConstants.ReadRetryDelayMilliseconds);
        }
        catch (EndOfStreamException) { throw invalid(); }
        catch (JsonException) { throw invalid(); }
    }

    /// <summary>Applique les mêmes contrôles de compatibilité de cœur, modèle, contenu et configuration à tous les modules.</summary>
    /// <remarks>Les empreintes sont calculées seulement après validation de l’identité, puis contenu avant configuration.</remarks>
    public static bool IsCompatible<TModel, TCore, TConfiguration, TOption>(EmulationSavedStateHeader<TModel, TCore> header,
        TModel model, TCore emulator, IEmulationCoreRuntime<TConfiguration, TOption> core,
        Func<string> configurationHash, Func<string> contentHash) =>
        header.FormatVersion == EmulationSavedStateConstants.CurrentFormatVersion
        && IsHeaderValid(header)
        && CoreIdentityMatches(EqualityComparer<TCore>.Default.Equals(header.Core, emulator),
            header.CoreName, header.CoreVersion, header.CoreSha256, core)
        && EqualityComparer<TModel>.Default.Equals(header.Model, model)
        && string.Equals(header.ContentSha256, contentHash(), StringComparison.OrdinalIgnoreCase)
        && string.Equals(header.ConfigurationSha256, configurationHash(), StringComparison.OrdinalIgnoreCase);

    /// <summary>Calcule une empreinte JSON canonique : clés triées ordinalement, ordre des tableaux conservé.</summary>
    public static string HashDocument<T>(T value, JsonSerializerOptions? options = null)
    {
        var document = JsonSerializer.SerializeToElement(value, options ?? StateJsonOptions);
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        WriteCanonical(document, writer);
        writer.Flush();
        return HashBytes(stream.GetBuffer().AsSpan(0, checked((int)stream.Length)));
    }

    private static void WriteCanonical(JsonElement element, Utf8JsonWriter writer)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            foreach (var property in element.EnumerateObject().OrderBy(property => property.Name, StringComparer.Ordinal))
            {
                writer.WritePropertyName(property.Name);
                WriteCanonical(property.Value, writer);
            }
            writer.WriteEndObject();
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            foreach (var item in element.EnumerateArray()) WriteCanonical(item, writer);
            writer.WriteEndArray();
        }
        else element.WriteTo(writer);
    }

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
        if (File.Exists(path)) return EmulationSdk.CoreDiagnostics.CalculateSha256(path);
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

    /// <summary>Vérifie les champs obligatoires et les identifiants du module, sous forme d’enums définis ou de texte non vide.</summary>
    /// <remarks>Le module doit aussi vérifier la version exacte et la correspondance des identités avec la machine courante.</remarks>
    public static bool IsHeaderValid<TModel, TCore>(EmulationSavedStateHeader<TModel, TCore> header) =>
        header.FormatVersion > 0
        && IsIdentityValid(header.Core)
        && IsIdentityValid(header.Model)
        && !string.IsNullOrWhiteSpace(header.CoreName)
        && !string.IsNullOrWhiteSpace(header.CoreVersion)
        && !string.IsNullOrWhiteSpace(header.CoreSha256)
        && !string.IsNullOrWhiteSpace(header.ConfigurationSha256)
        && !string.IsNullOrWhiteSpace(header.ContentSha256)
        && !string.IsNullOrWhiteSpace(header.StateSha256);

    private static bool IsIdentityValid<T>(T value) => value is Enum enumeration
        ? Enum.IsDefined(enumeration.GetType(), enumeration)
        : value is string text && !string.IsNullOrWhiteSpace(text);
}
