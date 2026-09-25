using System.Text.Json;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class StateStoreFunctions
{
    internal static string GetMachineDirectory(string stateRoot, Guid configurationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stateRoot);
        if (configurationId == Guid.Empty) throw new ArgumentException(nameof(configurationId));
        return Path.Combine(Path.GetFullPath(stateRoot), StateStoreConstants.AtariDirectoryName,
            configurationId.ToString(StateStoreConstants.MachineIdentifierFormat));
    }

    internal static string ValidateStateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalized = name.Trim();
        if (normalized.Length > StateStoreConstants.MaximumStateNameLength
            || string.Equals(normalized, StateStoreConstants.CurrentDirectoryName, StringComparison.Ordinal)
            || string.Equals(normalized, StateStoreConstants.ParentDirectoryName, StringComparison.Ordinal)
            || string.Equals(normalized, StateStoreConstants.QuickStateName,
                StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(StateStoreConstants.CurrentDirectoryName, StringComparison.Ordinal)
            || normalized.IndexOfAny(Path.GetInvalidFileNameChars()) >= StateConstants.FirstBufferIndex
            || normalized.Contains(Path.DirectorySeparatorChar)
            || normalized.Contains(Path.AltDirectorySeparatorChar))
            throw new ArgumentException(nameof(name));
        return normalized;
    }

    internal static string GetFileStem(StoredStateCategory category, string name) => category switch
    {
        StoredStateCategory.Quick => StateStoreConstants.QuickStateName,
        StoredStateCategory.Named => ValidateStateName(name),
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    internal static void WriteBytesAtomically(string path, byte[] bytes) =>
        WriteAtomically(path, stream => stream.Write(bytes));

    internal static void WriteMetadataAtomically(string path, StoredStateMetadata metadata) =>
        WriteAtomically(path, stream => JsonSerializer.Serialize(stream, metadata,
            StateStoreConstants.JsonOptions));

    internal static void WriteAtomically(string path, Action<Stream> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + StateStoreConstants.TemporaryFileExtension;
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None,
                       StateStoreConstants.CopyBufferSize, FileOptions.WriteThrough))
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

    internal static StoredStateMetadata ReadMetadata(string path)
    {
        for (var retry = StateStoreConstants.FirstRetryIndex;
             retry < StateStoreConstants.MetadataReadRetryCount; retry++)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                return JsonSerializer.Deserialize<StoredStateMetadata>(stream,
                           StateStoreConstants.JsonOptions)
                       ?? throw new InvalidDataException(StateConstants.InvalidHeaderError);
            }
            catch (IOException) when (retry + StateStoreConstants.NextRetryCount
                                      < StateStoreConstants.MetadataReadRetryCount)
            {
                Thread.Sleep(StateStoreConstants.MetadataReadRetryDelayMilliseconds);
            }
        }
        throw new FileNotFoundException(path);
    }

    internal static string MetadataPath(string machineDirectory, string stem) =>
        Path.Combine(machineDirectory, stem + StateStoreConstants.MetadataFileExtension);

    internal static string StatePath(string machineDirectory, string stem) =>
        Path.Combine(machineDirectory, stem + StateStoreConstants.StateFileExtension);

    internal static string CapturePath(string machineDirectory, string stem) =>
        Path.Combine(machineDirectory, stem + StateStoreConstants.CaptureFileExtension);
}
