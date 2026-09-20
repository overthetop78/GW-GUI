using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaEngine.Interfaces.Exploration;
using GWGUI.MediaEngine.Images.Reading.Optical;
using GWGUI.MediaEngine.Images.Models.Optical;

namespace GWGUI.MediaEngine.FileSystems.Iso9660;

/// <summary>Reads ISO 9660 primary volume descriptors and their directory tree from optical tracks.</summary>
public class Iso9660FileSystemReader : IMediaFileSystemReader
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();
    private readonly OpticalSectorReader sectors = new();

    public virtual string Id => FileSystemIds.Iso9660;

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;

    public bool CanRead(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        try
        {
            _ = ReadDescriptor(document, volume);
            return true;
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException or ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public FileSystemVolume Read(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        var descriptor = ReadDescriptor(document, volume);
        var blockSize = ReadBothEndianUInt16(descriptor, Iso9660Constants.LogicalBlockSizeOffset);
        if (blockSize != Iso9660Constants.LogicalBlockSize)
            throw new NotSupportedException($"Unsupported ISO 9660 logical block size {blockSize}.");
        var volumeBlocks = ReadBothEndianUInt32(descriptor, Iso9660Constants.VolumeSpaceSizeOffset);
        var root = ReadDirectoryRecord(descriptor, Iso9660Constants.RootDirectoryRecordOffset);
        var track = ResolveTrack(document, volume);
        var warnings = new List<string>();
        var visited = new HashSet<uint>();
        var entries = ReadDirectory(track, root.Extent, root.Length, visited, warnings);
        var name = DecodeVolumeIdentifier(descriptor.AsSpan(
            Iso9660Constants.VolumeIdentifierOffset,
            Iso9660Constants.VolumeIdentifierLength));
        return new FileSystemVolume(
            name,
            Id,
            checked((long)volumeBlocks * blockSize),
            0,
            null,
            null,
            entries,
            warnings,
            freeSpaceKnown: false,
            attributes: ["ISO 9660"]);
    }

    protected virtual bool AcceptVolumeDescriptor(ReadOnlySpan<byte> descriptor) =>
        descriptor[0] == Iso9660Constants.PrimaryVolumeDescriptorType;

    protected virtual bool AcceptFileSystem(OpticalTrackDescriptor track, ReadOnlySpan<byte> descriptor) => true;

    protected virtual string DecodeVolumeIdentifier(ReadOnlySpan<byte> value) =>
        System.Text.Encoding.ASCII.GetString(value).TrimEnd(' ', '\0');

    protected virtual string DecodeFileIdentifier(ReadOnlySpan<byte> value)
    {
        var name = System.Text.Encoding.ASCII.GetString(value);
        var version = name.IndexOf(';');
        if (version >= 0) name = name[..version];
        return name.EndsWith('.') ? name[..^1] : name;
    }

    protected virtual string ResolveEntryName(ReadOnlySpan<byte> record, string decodedName) => decodedName;

    private byte[] ReadDescriptor(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        var track = ResolveTrack(document, volume);
        for (var block = Iso9660Constants.FirstVolumeDescriptorBlock; ; block++)
        {
            var descriptor = ReadUserSector(track, block);
            if (descriptor.Length < Iso9660Constants.LogicalBlockSize)
                throw new InvalidDataException("An ISO 9660 volume descriptor is incomplete.");
            if (!descriptor.AsSpan(
                    Iso9660Constants.VolumeDescriptorIdentifierOffset,
                    Iso9660Constants.VolumeDescriptorIdentifierLength)
                .SequenceEqual(System.Text.Encoding.ASCII.GetBytes(Iso9660Constants.VolumeDescriptorIdentifier)))
                throw new InvalidDataException("The ISO 9660 volume descriptor identifier is missing.");
            if (AcceptVolumeDescriptor(descriptor) && AcceptFileSystem(track, descriptor)) return descriptor;
            if (descriptor[0] == Iso9660Constants.VolumeDescriptorTerminatorType)
                throw new InvalidDataException("The requested ISO 9660 volume descriptor is absent.");
        }
    }

    private IReadOnlyList<FileSystemEntry> ReadDirectory(
        OpticalTrackDescriptor track,
        uint extent,
        uint length,
        HashSet<uint> visited,
        ICollection<string> warnings)
    {
        if (!visited.Add(extent))
        {
            warnings.Add($"ISO 9660 directory extent {extent} is referenced recursively.");
            return [];
        }
        var data = ReadExtent(track, extent, length);
        var entries = new List<FileSystemEntry>();
        var offset = 0;
        while (offset < data.Length)
        {
            var recordLength = data[offset];
            if (recordLength == 0)
            {
                offset = checked((offset / Iso9660Constants.LogicalBlockSize + 1) * Iso9660Constants.LogicalBlockSize);
                continue;
            }
            if (recordLength < Iso9660Constants.DirectoryRecordMinimumLength || offset > data.Length - recordLength)
                throw new InvalidDataException("An ISO 9660 directory record exceeds its extent.");
            var record = data.AsSpan(offset, recordLength);
            var nameLength = record[Iso9660Constants.DirectoryFileIdentifierLengthOffset];
            if (Iso9660Constants.DirectoryFileIdentifierOffset + nameLength > record.Length)
                throw new InvalidDataException("An ISO 9660 file identifier exceeds its directory record.");
            var identifier = record.Slice(Iso9660Constants.DirectoryFileIdentifierOffset, nameLength);
            if (!(nameLength == 1 && identifier[0] is 0 or 1))
            {
                var name = ResolveEntryName(record, DecodeFileIdentifier(identifier));
                var childExtent = BinaryPrimitives.ReadUInt32LittleEndian(record.Slice(Iso9660Constants.DirectoryExtentOffset, 4));
                var childLength = BinaryPrimitives.ReadUInt32LittleEndian(record.Slice(Iso9660Constants.DirectoryDataLengthOffset, 4));
                var isDirectory = (record[Iso9660Constants.DirectoryFlagsOffset] & Iso9660Constants.DirectoryFlag) != 0;
                var children = isDirectory
                    ? ReadDirectory(track, childExtent, childLength, visited, warnings)
                    : [];
                IReadOnlyList<byte>? content = null;
                if (!isDirectory && childLength <= int.MaxValue)
                    content = ReadExtent(track, childExtent, childLength);
                entries.Add(new FileSystemEntry(
                    name,
                    isDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File,
                    childLength,
                    DecodeRecordingDate(record.Slice(Iso9660Constants.DirectoryDateOffset, 7)),
                    string.Empty,
                    record[Iso9660Constants.DirectoryFlagsOffset],
                    checked((int)Math.Min(childExtent, int.MaxValue)),
                    true,
                    children,
                    content,
                    occupiedSize: checked((long)((childLength + Iso9660Constants.LogicalBlockSize - 1) / Iso9660Constants.LogicalBlockSize) * Iso9660Constants.LogicalBlockSize)));
            }
            offset += recordLength;
        }
        visited.Remove(extent);
        return entries;
    }

    private byte[] ReadExtent(OpticalTrackDescriptor track, uint extent, uint length)
    {
        if (length > int.MaxValue) throw new NotSupportedException("The ISO 9660 extent is too large to expose as one entry.");
        var result = new byte[(int)length];
        var completed = 0;
        var block = extent;
        while (completed < result.Length)
        {
            var sector = ReadUserSector(track, block++);
            var count = Math.Min(sector.Length, result.Length - completed);
            sector.AsSpan(0, count).CopyTo(result.AsSpan(completed));
            completed += count;
        }
        return result;
    }

    protected byte[] ReadRootDirectoryData(OpticalTrackDescriptor track, ReadOnlySpan<byte> descriptor)
    {
        var root = ReadDirectoryRecord(descriptor, Iso9660Constants.RootDirectoryRecordOffset);
        return ReadExtent(track, root.Extent, root.Length);
    }

    private byte[] ReadUserSector(OpticalTrackDescriptor track, long relativeSector) =>
        sectors.ReadUserDataAsync(track, relativeSector).AsTask().GetAwaiter().GetResult();

    private static OpticalTrackDescriptor ResolveTrack(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        if (document.Representation is not OpticalMediaImageRepresentation optical || optical.Tracks is null)
            throw new NotSupportedException("ISO 9660 requires optical track data.");
        return optical.Tracks.FirstOrDefault(track =>
                !track.IsAudio
                && (volume.SessionNumber is null || track.SessionNumber == volume.SessionNumber)
                && (volume.TrackNumber is null || track.TrackNumber == volume.TrackNumber))
            ?? throw new InvalidDataException("No data track is available for the requested optical volume.");
    }

    private static (uint Extent, uint Length) ReadDirectoryRecord(ReadOnlySpan<byte> source, int offset)
    {
        var length = source[offset];
        if (length < Iso9660Constants.DirectoryRecordMinimumLength || offset > source.Length - length)
            throw new InvalidDataException("The ISO 9660 root directory record is invalid.");
        return (
            BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(offset + Iso9660Constants.DirectoryExtentOffset, 4)),
            BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(offset + Iso9660Constants.DirectoryDataLengthOffset, 4)));
    }

    private static ushort ReadBothEndianUInt16(ReadOnlySpan<byte> source, int offset)
    {
        var little = BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(offset, 2));
        var big = BinaryPrimitives.ReadUInt16BigEndian(source.Slice(offset + 2, 2));
        if (little != big) throw new InvalidDataException("An ISO 9660 both-endian 16-bit value is inconsistent.");
        return little;
    }

    private static uint ReadBothEndianUInt32(ReadOnlySpan<byte> source, int offset)
    {
        var little = BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(offset, 4));
        var big = BinaryPrimitives.ReadUInt32BigEndian(source.Slice(offset + 4, 4));
        if (little != big) throw new InvalidDataException("An ISO 9660 both-endian 32-bit value is inconsistent.");
        return little;
    }

    private static DateTimeOffset? DecodeRecordingDate(ReadOnlySpan<byte> value)
    {
        if (value.Length < 7 || value[0] == 0) return null;
        try
        {
            var offset = TimeSpan.FromMinutes(unchecked((sbyte)value[6]) * 15);
            return new DateTimeOffset(value[0] + 1900, value[1], value[2], value[3], value[4], value[5], offset);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
