using System.Text.Json;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariStateStoreFunctions
{
    internal static string GetMachineDirectory(string stateRoot, Guid configurationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stateRoot);
        if (configurationId == Guid.Empty) throw new ArgumentException(nameof(configurationId));
        return Path.Combine(Path.GetFullPath(stateRoot), AtariStateStoreConstants.AtariDirectoryName,
            configurationId.ToString(AtariStateStoreConstants.MachineIdentifierFormat));
    }

    internal static string ValidateStateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalized = name.Trim();
        if (normalized.Length > AtariStateStoreConstants.MaximumStateNameLength
            || string.Equals(normalized, AtariStateStoreConstants.CurrentDirectoryName, StringComparison.Ordinal)
            || string.Equals(normalized, AtariStateStoreConstants.ParentDirectoryName, StringComparison.Ordinal)
            || string.Equals(normalized, AtariStateStoreConstants.QuickStateName,
                StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(AtariStateStoreConstants.CurrentDirectoryName, StringComparison.Ordinal)
            || normalized.IndexOfAny(Path.GetInvalidFileNameChars()) >= AtariStateConstants.FirstBufferIndex
            || normalized.Contains(Path.DirectorySeparatorChar)
            || normalized.Contains(Path.AltDirectorySeparatorChar))
            throw new ArgumentException(nameof(name));
        return normalized;
    }

    internal static string GetFileStem(AtariStoredStateCategory category, string name) => category switch
    {
        AtariStoredStateCategory.Quick => AtariStateStoreConstants.QuickStateName,
        AtariStoredStateCategory.Named => ValidateStateName(name),
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    internal static void WriteBytesAtomically(string path, byte[] bytes) =>
        WriteAtomically(path, stream => stream.Write(bytes));

    internal static void WriteMetadataAtomically(string path, AtariStoredStateMetadata metadata) =>
        WriteAtomically(path, stream => JsonSerializer.Serialize(stream, metadata,
            AtariStateStoreConstants.JsonOptions));

    internal static void WriteAtomically(string path, Action<Stream> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + AtariStateStoreConstants.TemporaryFileExtension;
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None,
                       AtariStateStoreConstants.CopyBufferSize, FileOptions.WriteThrough))
            {
                write(stream);
                stream.Flush(flushToDisk: true);
            }
            if (File.Exists(fullPath)) File.Replace(temporaryPath, fullPath, destinationBackupFileName: null);
            else File.Move(temporaryPath, fullPath);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    internal static AtariStoredStateMetadata ReadMetadata(string path)
    {
        for (var retry = AtariStateStoreConstants.FirstRetryIndex;
             retry < AtariStateStoreConstants.MetadataReadRetryCount; retry++)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                return JsonSerializer.Deserialize<AtariStoredStateMetadata>(stream,
                           AtariStateStoreConstants.JsonOptions)
                       ?? throw new InvalidDataException(AtariStateConstants.InvalidHeaderError);
            }
            catch (IOException) when (retry + AtariStateStoreConstants.NextRetryCount
                                      < AtariStateStoreConstants.MetadataReadRetryCount)
            {
                Thread.Sleep(AtariStateStoreConstants.MetadataReadRetryDelayMilliseconds);
            }
        }
        throw new FileNotFoundException(path);
    }

    internal static string MetadataPath(string machineDirectory, string stem) =>
        Path.Combine(machineDirectory, stem + AtariStateStoreConstants.MetadataFileExtension);

    internal static string StatePath(string machineDirectory, string stem) =>
        Path.Combine(machineDirectory, stem + AtariStateStoreConstants.StateFileExtension);

    internal static string CapturePath(string machineDirectory, string stem) =>
        Path.Combine(machineDirectory, stem + AtariStateStoreConstants.CaptureFileExtension);
}
