using System.Buffers.Binary;
using System.IO;
using System.Text;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.FileSystems.Nintendo.GameCube;

/// <summary>Expose la table FST des images optiques GameCube et Wii.</summary>
public sealed class GameCubeFstFileSystemReader : IMediaFileSystemReader
{
    public string Id => GameCubeFstFileSystemConstants.FileSystemId;

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
        var entries = ReadDirectory(context, 0, context.EntryCount, 0, warnings);
        return new FileSystemVolume(
            GameCubeFstFileSystemConstants.FileSystemDisplayName,
            Id,
            context.Track.SectorCount * context.Track.UserDataLength,
            0,
            null,
            null,
            entries,
            warnings,
            freeSpaceKnown: false,
            attributes: [GameCubeFstFileSystemConstants.FileAttribute],
            fileSystemDisplayName: GameCubeFstFileSystemConstants.FileSystemDisplayName);
    }

    private static GameCubeFstContext ReadContext(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        if (!string.Equals(document.FormatId, MediaImageFormatIds.OpticalIso, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The document is not an optical ISO image.");
        if (!document.Metadata.TryGetValue("sourceExtension", out var extension)
            || (!string.Equals(extension, GameCubeFstFileSystemConstants.SourceExtension, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(extension, GameCubeFstFileSystemConstants.IsoExtension, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("The document is not a GameCube image.");
        if (document.Representation is not IMediaOpticalRepresentation optical || optical.Tracks is null)
            throw new NotSupportedException("GameCube FST requires optical track data.");
        var track = optical.Tracks.FirstOrDefault(candidate =>
            !candidate.IsAudio
            && (volume.SessionNumber is null || candidate.SessionNumber == volume.SessionNumber)
            && (volume.TrackNumber is null || candidate.TrackNumber == volume.TrackNumber))
            ?? throw new InvalidDataException("No data track is available for the GameCube FST volume.");
        if (track.UserDataLength != GameCubeFstFileSystemConstants.SectorSize)
            throw new NotSupportedException("GameCube FST requires 2048-byte optical sectors.");

        var header = ReadBytes(track, 0, GameCubeFstFileSystemConstants.DiscHeaderLength);
        if (ReadUInt32(header, GameCubeFstFileSystemConstants.DiscMagicOffset)
            != GameCubeFstFileSystemConstants.DiscMagic)
            throw new InvalidDataException("The GameCube disc header is invalid.");
        var fstOffset = ReadUInt32(header, GameCubeFstFileSystemConstants.FstOffsetField);
        var fstSize = ReadUInt32(header, GameCubeFstFileSystemConstants.FstSizeField);
        if (fstOffset < GameCubeFstFileSystemConstants.SectorSize
            || fstSize < GameCubeFstFileSystemConstants.FstEntrySize)
            throw new InvalidDataException("The GameCube FST descriptor is invalid.");
        ValidateByteRange(track, fstOffset, fstSize);
        var table = ReadBytes(track, fstOffset, checked((int)fstSize));
        var rootWord = ReadUInt32(table, 0);
        if ((rootWord & GameCubeFstFileSystemConstants.DirectoryFlag) == 0
            || (rootWord & GameCubeFstFileSystemConstants.NameOffsetMask) != 0)
            throw new InvalidDataException("The GameCube FST root entry is invalid.");
        var entryCount = ReadUInt32(table, 8);
        if (entryCount is < 1 or > GameCubeFstFileSystemConstants.MaximumEntryCount
            || checked((ulong)entryCount * GameCubeFstFileSystemConstants.FstEntrySize) > (ulong)table.Length)
            throw new InvalidDataException("The GameCube FST entry count is invalid.");
        var stringTableOffset = checked((int)entryCount * GameCubeFstFileSystemConstants.FstEntrySize);
        return new GameCubeFstContext(
            track,
            table,
            checked((int)entryCount),
            stringTableOffset,
            fstOffset);
    }

    private static IReadOnlyList<FileSystemEntry> ReadDirectory(
        GameCubeFstContext context,
        int directoryIndex,
        int endIndex,
        int depth,
        ICollection<string> warnings)
    {
        if (depth > GameCubeFstFileSystemConstants.MaximumDirectoryDepth)
            throw new InvalidDataException("The GameCube FST directory tree is too deep.");
        var nextIndex = checked((int)ReadUInt32(context.Table, directoryIndex * GameCubeFstFileSystemConstants.FstEntrySize + 8));
        if (nextIndex <= directoryIndex || nextIndex > context.EntryCount || nextIndex > endIndex)
            throw new InvalidDataException("The GameCube FST directory range is invalid.");
        var entries = new List<FileSystemEntry>();
        var index = directoryIndex + 1;
        while (index < nextIndex)
        {
            var entryOffset = checked(index * GameCubeFstFileSystemConstants.FstEntrySize);
            var typeAndName = ReadUInt32(context.Table, entryOffset);
            var isDirectory = (typeAndName & GameCubeFstFileSystemConstants.DirectoryFlag) != 0;
            var name = ReadName(context, typeAndName & GameCubeFstFileSystemConstants.NameOffsetMask);
            var location = ReadUInt32(context.Table, entryOffset + 4);
            var length = ReadUInt32(context.Table, entryOffset + 8);
            if (isDirectory)
            {
                var childEnd = checked((int)length);
                var children = ReadDirectory(context, index, nextIndex, depth + 1, warnings);
                entries.Add(CreateEntry(name, FileSystemEntryKind.Directory, 0, index, children, null, true));
                index = childEnd;
                continue;
            }

            ValidateByteRange(context.Track, location, length);
            IReadOnlyList<byte>? content = length <= GameCubeFstFileSystemConstants.MaximumFileContentLength
                ? ReadBytes(context.Track, location, checked((int)length))
                : null;
            if (content is null)
                warnings.Add($"GameCube file '{name}' is larger than the content limit and was not loaded.");
            entries.Add(CreateEntry(name, FileSystemEntryKind.File, length, index, [], content, content is not null));
            index++;
        }
        return entries;
    }

    private static FileSystemEntry CreateEntry(
        string name,
        FileSystemEntryKind kind,
        uint length,
        int index,
        IReadOnlyList<FileSystemEntry> children,
        IReadOnlyList<byte>? content,
        bool dataValid) => new(
            name,
            kind,
            length,
            null,
            string.Empty,
            0,
            index,
            true,
            children,
            content,
            nativeTypeId: kind == FileSystemEntryKind.Directory
                ? GameCubeFstFileSystemConstants.DirectoryEntryType
                : GameCubeFstFileSystemConstants.FileEntryType,
            occupiedSize: length,
            attributes: [GameCubeFstFileSystemConstants.FileAttribute],
            dataValid: dataValid,
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [GameCubeFstFileSystemConstants.OffsetMetadata] = index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                [GameCubeFstFileSystemConstants.LengthMetadata] = length.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });

    private static string ReadName(GameCubeFstContext context, uint offset)
    {
        var start = checked(context.StringTableOffset + (int)offset);
        if (start >= context.Table.Length)
            throw new InvalidDataException("The GameCube FST name offset is outside the string table.");
        var end = Array.IndexOf(context.Table, (byte)0, start);
        if (end < 0) end = context.Table.Length;
        var name = Encoding.UTF8.GetString(context.Table, start, end - start);
        return string.IsNullOrWhiteSpace(name)
            ? $"entry-{(start - context.StringTableOffset):D6}"
            : name;
    }

    private static uint ReadUInt32(byte[] data, int offset) =>
        BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset, sizeof(uint)));

    private static byte[] ReadBytes(IMediaOpticalTrack track, uint byteOffset, int length)
    {
        if (length == 0) return [];
        ValidateByteRange(track, byteOffset, checked((uint)length));
        var result = new byte[length];
        var firstSector = byteOffset / GameCubeFstFileSystemConstants.SectorSize;
        var sectorOffset = checked((int)(byteOffset % GameCubeFstFileSystemConstants.SectorSize));
        var remaining = length;
        var destination = 0;
        while (remaining > 0)
        {
            var sector = track.ReadUserDataAsync(firstSector, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            var available = Math.Min(remaining, sector.Length - sectorOffset);
            if (available <= 0) throw new InvalidDataException("The GameCube optical sector has no readable data.");
            sector.AsSpan(sectorOffset, available).CopyTo(result.AsSpan(destination, available));
            destination += available;
            remaining -= available;
            firstSector++;
            sectorOffset = 0;
        }
        return result;
    }

    private static void ValidateByteRange(IMediaOpticalTrack track, uint offset, uint length)
    {
        var capacity = checked((ulong)track.SectorCount * (ulong)track.UserDataLength);
        if ((ulong)offset > capacity || (ulong)length > capacity - offset)
            throw new InvalidDataException("The GameCube FST extent is outside the optical track.");
    }

    private sealed record GameCubeFstContext(
        IMediaOpticalTrack Track,
        byte[] Table,
        int EntryCount,
        int StringTableOffset,
        uint FstOffset);
}
