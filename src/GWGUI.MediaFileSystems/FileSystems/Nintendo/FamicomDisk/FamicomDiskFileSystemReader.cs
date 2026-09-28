using System.Globalization;
using System.IO;
using System.Text;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.FileSystems.Nintendo.FamicomDisk;

/// <summary>Expose les fichiers réellement décodables des faces Famicom Disk System.</summary>
public sealed class FamicomDiskFileSystemReader : IMediaFileSystemReader
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new HashSet<string>([MediaImageFormatIds.NintendoFamicomDisk], StringComparer.OrdinalIgnoreCase);

    public string Id => FamicomDiskFileSystemConstants.FileSystemId;

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
            throw new InvalidDataException("The selected Famicom Disk System volume is not readable.");

        if (!TryGetInt(document.Metadata, FamicomDiskMetadataConstants.SideCount, out var sideCount)
            || sideCount < 1)
            throw new InvalidDataException("The Famicom Disk System side count is missing or invalid.");
        if (!TryGetInt(document.Metadata, FamicomDiskMetadataConstants.SideLength, out var sideLength)
            || sideLength < 1)
            throw new InvalidDataException("The Famicom Disk System side length is missing or invalid.");

        var entries = new List<FileSystemEntry>();
        var warnings = new List<string>();
        for (var side = FamicomDiskFileSystemConstants.FirstSide; side < sideCount; side++)
        {
            var data = new byte[sideLength];
            blocks.ReadExactlyAsync(side * (long)sideLength, data).AsTask().GetAwaiter().GetResult();
            if (!TryReadSide(data, side, sideLength, entries, out var warning))
            {
                if (warning is not null) warnings.Add(warning);
            }
        }

        if (entries.Count == 0 && warnings.Count > 0)
            throw new InvalidDataException("No valid Famicom Disk System file block was found.");

        return new FileSystemVolume(
            FamicomDiskFileSystemConstants.FileSystemDisplayName,
            Id,
            blocks.Capacity,
            0,
            null,
            null,
            entries,
            warnings,
            freeSpaceKnown: false,
            attributes: [document.FormatId, FamicomDiskFileSystemConstants.FileAttribute],
            fileSystemDisplayName: FamicomDiskFileSystemConstants.FileSystemDisplayName);
    }

    private static bool TryReadSide(
        ReadOnlySpan<byte> side,
        int sideNumber,
        int sideLength,
        ICollection<FileSystemEntry> entries,
        out string? warning)
    {
        warning = null;
        if (side.Length != sideLength || side.Length < FamicomDiskFileSystemConstants.DiskInfoLength
            || side[0] != FamicomDiskFileSystemConstants.DiskInfoBlockCode)
        {
            warning = $"Famicom Disk System side {sideNumber} has no valid disk information block.";
            return false;
        }

        var offset = FamicomDiskFileSystemConstants.DiskInfoLength;
        if (offset + FamicomDiskFileSystemConstants.FileCountLength > side.Length
            || side[offset] != FamicomDiskFileSystemConstants.FileCountBlockCode)
        {
            warning = $"Famicom Disk System side {sideNumber} has no valid file count block.";
            return false;
        }

        var fileCount = side[offset + 1];
        offset += FamicomDiskFileSystemConstants.FileCountLength;
        for (var fileIndex = 0; fileIndex < fileCount; fileIndex++)
        {
            if (offset + FamicomDiskFileSystemConstants.FileHeaderLength > side.Length
                || side[offset] != FamicomDiskFileSystemConstants.FileHeaderBlockCode
                || !side.Slice(offset + FamicomDiskFileSystemConstants.FileIdentifierOffset,
                    FamicomDiskFileSystemConstants.FileIdentifierLength)
                    .SequenceEqual(FamicomDiskFileSystemConstants.FileIdentifier))
            {
                warning = $"Famicom Disk System side {sideNumber} has an invalid file header.";
                return false;
            }

            var fileNumber = side[offset + FamicomDiskFileSystemConstants.FileNumberOffset];
            var fileAddress = side[offset + FamicomDiskFileSystemConstants.FileAddressOffset]
                | side[offset + FamicomDiskFileSystemConstants.FileAddressOffset + 1] << 8;
            var fileLength = side[offset + FamicomDiskFileSystemConstants.FileSizeOffset]
                | side[offset + FamicomDiskFileSystemConstants.FileSizeOffset + 1] << 8;
            var fileType = side[offset + FamicomDiskFileSystemConstants.FileTypeOffset];
            var dataHeader = offset + FamicomDiskFileSystemConstants.FileHeaderLength;
            if (dataHeader >= side.Length || side[dataHeader] != FamicomDiskFileSystemConstants.FileDataBlockCode
                || fileLength > side.Length - dataHeader - 1)
            {
                warning = $"Famicom Disk System file {fileNumber} on side {sideNumber} has incomplete data.";
                return false;
            }

            var name = DecodeName(side.Slice(offset + FamicomDiskFileSystemConstants.FileNameOffset,
                FamicomDiskFileSystemConstants.FileNameLength));
            var synthetic = string.IsNullOrWhiteSpace(name);
            if (synthetic)
                name = string.Format(CultureInfo.InvariantCulture, FamicomDiskFileSystemConstants.DefaultFileNameFormat, fileNumber);
            var dataOffset = dataHeader + 1;
            entries.Add(new FileSystemEntry(
                name,
                FileSystemEntryKind.File,
                fileLength,
                null,
                string.Empty,
                0,
                sideNumber,
                true,
                [],
                side.Slice(dataOffset, fileLength).ToArray(),
                nativeTypeId: FamicomDiskFileSystemConstants.FileEntryType,
                occupiedSize: checked(FamicomDiskFileSystemConstants.FileHeaderLength + 1L + fileLength),
                attributes: [FamicomDiskFileSystemConstants.SideAttribute],
                dataValid: true,
                syntheticName: synthetic,
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [FamicomDiskFileSystemConstants.SideMetadata] = sideNumber.ToString(CultureInfo.InvariantCulture),
                    [FamicomDiskFileSystemConstants.FileNumberMetadata] = fileNumber.ToString(CultureInfo.InvariantCulture),
                    [FamicomDiskFileSystemConstants.FileAddressMetadata] = fileAddress.ToString(CultureInfo.InvariantCulture),
                    [FamicomDiskFileSystemConstants.FileTypeMetadata] = fileType.ToString(CultureInfo.InvariantCulture)
                }));
            offset = dataOffset + fileLength;
        }

        return true;
    }

    private static string DecodeName(ReadOnlySpan<byte> bytes)
        => Encoding.ASCII.GetString(bytes).TrimEnd('\0', ' ');

    private static bool TryGetInt(IReadOnlyDictionary<string, string> metadata, string key, out int value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}
