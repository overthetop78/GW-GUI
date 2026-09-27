using System.Globalization;
using System.IO;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.FileSystems.Console.Cartridge;

/// <summary>Exposes raw console cartridge banks without claiming a file system.</summary>
public sealed class RawCartridgeFileSystemReader : IMediaFileSystemReader
{
    private static readonly IReadOnlySet<string> SupportedFormats = new HashSet<string>(
        [MediaImageFormatIds.RawCartridge], StringComparer.OrdinalIgnoreCase);

    public string Id => FileSystemIds.RawCartridgeStructure;

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
            throw new InvalidDataException("The selected volume is not a raw cartridge structure.");
        if (!TryReadInt(document.Metadata, "bankCount", out var bankCount)
            || bankCount <= 0 || bankCount > 4096)
            throw new InvalidDataException("The raw cartridge bank count is missing or invalid.");
        if (!TryReadInt(document.Metadata, "bankSize", out var bankSize)
            || bankSize <= 0)
            throw new InvalidDataException("The raw cartridge bank size is missing or invalid.");

        var entries = new List<FileSystemEntry>(bankCount);
        for (var bank = 0; bank < bankCount; bank++)
        {
            if (!TryReadInt(document.Metadata, $"bank.{bank}.length", out var length)
                || length <= 0 || length > bankSize)
                throw new InvalidDataException($"Raw cartridge bank {bank} has an invalid length.");
            var data = new byte[length];
            blocks.ReadExactlyAsync(bank * (long)bankSize, data).AsTask().GetAwaiter().GetResult();
            var name = document.Metadata.TryGetValue($"bank.{bank}.name", out var storedName)
                ? storedName
                : $"bank{bank:D2}";
            entries.Add(new FileSystemEntry(
                name,
                FileSystemEntryKind.File,
                length,
                null,
                string.Empty,
                0,
                checked(bank * bankSize),
                true,
                [],
                data,
                nativeTypeId: "cartridge-bank",
                occupiedSize: length,
                attributes: ["bank"],
                dataValid: true,
                syntheticName: !document.Metadata.ContainsKey($"bank.{bank}.name"),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["bankNumber"] = bank.ToString(CultureInfo.InvariantCulture),
                    ["address"] = (bank * (long)bankSize).ToString(CultureInfo.InvariantCulture)
                }));
        }

        return new FileSystemVolume(
            volume.Name ?? string.Empty,
            FileSystemIds.RawCartridgeStructure,
            blocks.Capacity,
            0,
            null,
            null,
            entries,
            document.Diagnostics,
            freeSpaceKnown: false,
            attributes: [document.FormatId, "cartridge-banks"],
            fileSystemDisplayName: "Raw cartridge banks");
    }

    private static bool TryReadInt(IReadOnlyDictionary<string, string> metadata,
        string key, out int value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}
