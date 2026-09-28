using System.Buffers.Binary;
using System.IO;
using System.Text;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.FileSystems.Xbox.Xdvdfs;

/// <summary>Expose les fichiers et dossiers d'une image Xbox XDVDFS/XISO.</summary>
public sealed class XdvdfsFileSystemReader : IMediaFileSystemReader
{
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes(XdvdfsFileSystemConstants.MagicText);

    public string Id => XdvdfsFileSystemConstants.FileSystemId;

    public bool CanRead(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        try
        {
            _ = ReadContext(document, volume);
            return true;
        }
        catch (Exception exception) when (exception is InvalidDataException
            or NotSupportedException or ArgumentOutOfRangeException or OverflowException)
        {
            return false;
        }
    }

    public FileSystemVolume Read(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        var context = ReadContext(document, volume);
        var warnings = new List<string>();
        var entries = context.RootSize == 0
            ? []
            : ReadDirectory(context, context.RootSector, context.RootSize, warnings);
        return new FileSystemVolume(
            XdvdfsFileSystemConstants.FileSystemDisplayName,
            Id,
            context.Track.SectorCount * context.Track.UserDataLength,
            0,
            null,
            null,
            entries,
            warnings,
            freeSpaceKnown: false,
            attributes: [XdvdfsFileSystemConstants.FileAttribute],
            fileSystemDisplayName: XdvdfsFileSystemConstants.FileSystemDisplayName);
    }

    private static XdvdfsContext ReadContext(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        if (!string.Equals(document.FormatId, MediaImageFormatIds.MicrosoftXboxXdvdfs, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The document is not an Xbox XDVDFS image.");
        if (document.Representation is not IMediaOpticalRepresentation optical || optical.Tracks is null)
            throw new NotSupportedException("Xbox XDVDFS requires optical track data.");
        var track = optical.Tracks.FirstOrDefault(candidate =>
            !candidate.IsAudio
            && (volume.SessionNumber is null || candidate.SessionNumber == volume.SessionNumber)
            && (volume.TrackNumber is null || candidate.TrackNumber == volume.TrackNumber))
            ?? throw new InvalidDataException("No data track is available for the Xbox XDVDFS volume.");
        if (track.UserDataLength != XdvdfsFileSystemConstants.SectorSize)
            throw new NotSupportedException("Xbox XDVDFS requires 2048-byte optical sectors.");
        var descriptor = ReadSector(track, XdvdfsFileSystemConstants.VolumeDescriptorSector);
        if (!descriptor.AsSpan(XdvdfsFileSystemConstants.MagicOffset, Magic.Length).SequenceEqual(Magic)
            || !descriptor.AsSpan(XdvdfsFileSystemConstants.DescriptorTrailerOffset, Magic.Length).SequenceEqual(Magic))
            throw new InvalidDataException("The Xbox XDVDFS volume descriptor is invalid.");
        var rootSector = BinaryPrimitives.ReadUInt32LittleEndian(
            descriptor.AsSpan(XdvdfsFileSystemConstants.RootDirectorySectorOffset, sizeof(uint)));
        var rootSize = BinaryPrimitives.ReadUInt32LittleEndian(
            descriptor.AsSpan(XdvdfsFileSystemConstants.RootDirectorySizeOffset, sizeof(uint)));
        if (rootSize % XdvdfsFileSystemConstants.SectorSize != 0)
            throw new InvalidDataException("The Xbox XDVDFS root directory size is not sector-aligned.");
        if (rootSize > int.MaxValue)
            throw new NotSupportedException("The Xbox XDVDFS root directory is too large to expose.");
        ValidateExtent(track, rootSector, rootSize);
        return new XdvdfsContext(track, rootSector, rootSize);
    }

    private static IReadOnlyList<FileSystemEntry> ReadDirectory(
        XdvdfsContext context,
        uint startSector,
        uint length,
        ICollection<string> warnings)
    {
        if (length == 0) return [];
        var table = ReadExtent(context.Track, startSector, length);
        var entries = new List<FileSystemEntry>();
        var visited = new HashSet<int>();
        VisitNode(context, table, 0, 0, visited, entries, warnings);
        return entries;
    }

    private static void VisitNode(
        XdvdfsContext context,
        byte[] table,
        int entryOffset,
        int depth,
        ISet<int> visited,
        ICollection<FileSystemEntry> entries,
        ICollection<string> warnings)
    {
        if (entryOffset == 0 && entries.Count > 0 && depth == 0) return;
        if (depth > XdvdfsFileSystemConstants.MaximumDirectoryDepth)
            throw new InvalidDataException("The Xbox XDVDFS directory tree is too deep.");
        if (entryOffset < 0 || entryOffset > table.Length - XdvdfsFileSystemConstants.MinimumEntryLength
            || !visited.Add(entryOffset))
        {
            warnings.Add("The Xbox XDVDFS directory tree contains an invalid or recursive pointer.");
            return;
        }

        var left = BinaryPrimitives.ReadUInt16LittleEndian(table.AsSpan(entryOffset + XdvdfsFileSystemConstants.LeftChildOffset, 2));
        var right = BinaryPrimitives.ReadUInt16LittleEndian(table.AsSpan(entryOffset + XdvdfsFileSystemConstants.RightChildOffset, 2));
        var startSector = BinaryPrimitives.ReadUInt32LittleEndian(table.AsSpan(entryOffset + XdvdfsFileSystemConstants.FileStartSectorOffset, 4));
        var length = BinaryPrimitives.ReadUInt32LittleEndian(table.AsSpan(entryOffset + XdvdfsFileSystemConstants.FileLengthOffset, 4));
        var attributes = table[entryOffset + XdvdfsFileSystemConstants.FileAttributesOffset];
        var nameLength = table[entryOffset + XdvdfsFileSystemConstants.FileNameLengthOffset];
        var entryLength = XdvdfsFileSystemConstants.FileNameOffset + nameLength;
        var paddedLength = checked((entryLength + 3) & ~3);
        if (entryLength < XdvdfsFileSystemConstants.MinimumEntryLength
            || paddedLength > table.Length - entryOffset)
            throw new InvalidDataException("An Xbox XDVDFS directory entry exceeds its directory table.");
        var name = Encoding.ASCII.GetString(table, entryOffset + XdvdfsFileSystemConstants.FileNameOffset, nameLength).TrimEnd('\0');
        if (string.IsNullOrWhiteSpace(name))
            name = $"entry-{entryOffset / 4:D4}";

        if (left != 0) VisitNode(context, table, checked(left * 4), depth + 1, visited, entries, warnings);
        var isDirectory = (attributes & XdvdfsFileSystemConstants.DirectoryAttribute) != 0;
        IReadOnlyList<FileSystemEntry> children = [];
        IReadOnlyList<byte>? content = null;
        if (isDirectory)
        {
            if (length != 0)
                children = ReadDirectory(context, startSector, length, warnings);
        }
        else if (length <= XdvdfsFileSystemConstants.MaximumFileContentLength)
        {
            ValidateExtent(context.Track, startSector, length);
            content = ReadExtent(context.Track, startSector, length);
        }
        entries.Add(new FileSystemEntry(
            name,
            isDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File,
            length,
            null,
            string.Empty,
            attributes,
            checked((int)Math.Min(startSector, int.MaxValue)),
            true,
            children,
            content,
            nativeTypeId: isDirectory ? XdvdfsFileSystemConstants.DirectoryEntryType : XdvdfsFileSystemConstants.FileEntryType,
            occupiedSize: checked((long)((length + XdvdfsFileSystemConstants.SectorSize - 1) / XdvdfsFileSystemConstants.SectorSize) * XdvdfsFileSystemConstants.SectorSize),
            attributes: [XdvdfsFileSystemConstants.FileAttribute],
            dataValid: isDirectory || content is not null,
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [XdvdfsFileSystemConstants.SectorMetadata] = startSector.ToString(System.Globalization.CultureInfo.InvariantCulture),
                [XdvdfsFileSystemConstants.LengthMetadata] = length.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }));
        if (right != 0) VisitNode(context, table, checked(right * 4), depth + 1, visited, entries, warnings);
    }

    private static byte[] ReadExtent(IMediaOpticalTrack track, uint startSector, uint length)
    {
        if (length == 0) return [];
        ValidateExtent(track, startSector, length);
        var result = new byte[checked((int)length)];
        var offset = 0;
        var sectors = (length + XdvdfsFileSystemConstants.SectorSize - 1) / XdvdfsFileSystemConstants.SectorSize;
        for (uint index = 0; index < sectors; index++)
        {
            var sector = ReadSector(track, checked(startSector + index));
            var count = Math.Min(sector.Length, result.Length - offset);
            sector.AsSpan(0, count).CopyTo(result.AsSpan(offset));
            offset += count;
        }
        return result;
    }

    private static byte[] ReadSector(IMediaOpticalTrack track, long absoluteSector)
    {
        if (absoluteSector < track.FirstSector || absoluteSector - track.FirstSector >= track.SectorCount)
            throw new InvalidDataException("The Xbox XDVDFS sector is outside the optical track.");
        return track.ReadUserDataAsync(absoluteSector - track.FirstSector).AsTask().GetAwaiter().GetResult();
    }

    private static void ValidateExtent(IMediaOpticalTrack track, uint startSector, uint length)
    {
        if (length == 0) return;
        var sectors = (length + XdvdfsFileSystemConstants.SectorSize - 1) / XdvdfsFileSystemConstants.SectorSize;
        if (startSector < track.FirstSector || sectors > track.SectorCount - (startSector - track.FirstSector))
            throw new InvalidDataException("The Xbox XDVDFS file extent is outside the optical track.");
    }

    private sealed record XdvdfsContext(IMediaOpticalTrack Track, uint RootSector, uint RootSize);
}
