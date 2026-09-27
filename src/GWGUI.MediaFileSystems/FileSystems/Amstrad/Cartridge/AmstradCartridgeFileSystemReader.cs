using System.IO;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.FileSystems.Amstrad.Cartridge;

/// <summary>Exposes declared CPC cartridge and ROM banks without interpreting them as a file system.</summary>
public sealed class AmstradCartridgeFileSystemReader : IMediaFileSystemReader
{
    private const int BankLength = 16 * 1024;
    private static readonly IReadOnlySet<string> SupportedFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        MediaImageFormatIds.AmstradCpr,
        MediaImageFormatIds.AmstradRom
    };

    public string Id => FileSystemIds.AmstradMediaStructure;

    public bool CanRead(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        return SupportedFormats.Contains(document.FormatId)
            && document.Representation is IMediaBlockRepresentation blocks
            && volume.Start == 0
            && volume.Length == blocks.Capacity;
    }

    public FileSystemVolume Read(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        if (!CanRead(document, volume) || document.Representation is not IMediaBlockRepresentation blocks)
            throw new InvalidDataException("The selected volume is not a supported Amstrad cartridge or ROM structure.");
        return ReadCartridge(document, volume, blocks);
    }

    private static FileSystemVolume ReadCartridge(
        IMediaImageDocument document,
        MediaVolumeDescriptor volume,
        IMediaBlockRepresentation blocks)
    {
        if (!TryReadInt(document.Metadata, "bankCount", out var bankCount) || bankCount is <= 0 or > 32)
            throw new InvalidDataException("The Amstrad cartridge bank count is missing or invalid.");
        var entries = new List<FileSystemEntry>(bankCount);
        for (var bank = 0; bank < 32; bank++)
        {
            if (!TryReadInt(document.Metadata, $"bank.{bank}.length", out var length)) continue;
            if (length <= 0 || length > BankLength) throw new InvalidDataException($"Amstrad bank {bank} has an invalid length.");
            var data = new byte[length];
            blocks.ReadExactlyAsync(bank * (long)BankLength, data).AsTask().GetAwaiter().GetResult();
            var storedName = document.Metadata.TryGetValue($"bank.{bank}.chunkId", out var chunkId)
                ? chunkId
                : $"bank{bank:D2}";
            entries.Add(new FileSystemEntry(
                storedName,
                FileSystemEntryKind.File,
                length,
                null,
                string.Empty,
                0,
                bank * BankLength,
                true,
                [],
                data,
                nativeTypeId: "cartridge-bank",
                occupiedSize: length,
                attributes: ["bank"],
                dataValid: true,
                syntheticName: !document.Metadata.ContainsKey($"bank.{bank}.chunkId"),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["bankNumber"] = bank.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["address"] = (bank * BankLength).ToString(System.Globalization.CultureInfo.InvariantCulture)
                }));
        }
        if (entries.Count != bankCount) throw new InvalidDataException("The declared Amstrad cartridge bank count does not match its mapped banks.");
        return new FileSystemVolume(
            volume.Name ?? string.Empty,
            FileSystemIds.AmstradMediaStructure,
            blocks.Capacity,
            0,
            null,
            null,
            entries,
            document.Diagnostics,
            freeSpaceKnown: false,
            attributes: [document.FormatId, "cartridge-banks"],
            fileSystemDisplayName: "Amstrad cartridge banks");
    }

    private static bool TryReadInt(IReadOnlyDictionary<string, string> metadata, string key, out int value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && int.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out value);
    }

}
