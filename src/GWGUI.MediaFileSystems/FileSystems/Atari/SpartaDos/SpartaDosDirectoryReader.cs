using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

internal static class SpartaDosDirectoryReader
{
    public static IReadOnlyList<FileSystemEntry> ReadRoot(IMediaSectorImage image, SpartaDosDiskHeader header, ICollection<string> warnings) =>
        ReadDirectory(image, header.RootDirectoryMapSector, new HashSet<int>(), warnings).Children;

    private static DirectoryData ReadDirectory(
        IMediaSectorImage image,
        int firstMapSector,
        ISet<int> ancestors,
        ICollection<string> warnings)
    {
        if (!ancestors.Add(firstMapSector)) throw SpartaDosFileSystemExceptions.DirectoryLoop(firstMapSector);
        try
        {
            var firstDataSector = SpartaDosSectorMapReader.ReadFirstDataSector(image, firstMapSector);
            var firstData = SpartaDosDiskReader.ReadSector(image, firstDataSector);
            if (firstData.Length < SpartaDosFileSystemLayout.DirectoryEntrySize)
                throw SpartaDosFileSystemExceptions.InvalidDirectoryLength(firstMapSector, firstData.Length);
            var length = ReadLength(firstData, SpartaDosFileSystemLayout.DirectoryHeaderLengthOffset);
            if (length < SpartaDosFileSystemLayout.DirectoryEntrySize || length > image.Capacity)
                throw SpartaDosFileSystemExceptions.InvalidDirectoryLength(firstMapSector, length);
            var directoryFile = SpartaDosFileReader.Read(image, firstMapSector, length);
            var bytes = directoryFile.Content.ToArray();
            var name = SpartaDosNameCodec.Decode(bytes.AsSpan(SpartaDosFileSystemLayout.DirectoryHeaderNameOffset, SpartaDosFileSystemLayout.DirectoryHeaderNameLength));
            var entries = new List<FileSystemEntry>();
            for (var offset = SpartaDosFileSystemLayout.DirectoryFirstEntryOffset;
                 offset + SpartaDosFileSystemLayout.DirectoryEntrySize <= bytes.Length;
                 offset += SpartaDosFileSystemLayout.DirectoryEntrySize)
            {
                var flags = (SpartaDosDirectoryFlags)bytes[offset + SpartaDosFileSystemLayout.DirectoryStatusOffset];
                if (flags == SpartaDosDirectoryFlags.None) break;
                if (!flags.HasFlag(SpartaDosDirectoryFlags.InUse) || flags.HasFlag(SpartaDosDirectoryFlags.Deleted)) continue;
                var mapSector = ReadUInt16(bytes, offset + SpartaDosFileSystemLayout.DirectoryMapOffset);
                var entryLength = ReadLength(bytes, offset + SpartaDosFileSystemLayout.DirectoryLengthOffset);
                var entryName = SpartaDosNameCodec.DecodeFileName(
                    bytes.AsSpan(offset + SpartaDosFileSystemLayout.DirectoryNameOffset, SpartaDosFileSystemLayout.DirectoryNameLength),
                    bytes.AsSpan(offset + SpartaDosFileSystemLayout.DirectoryExtensionOffset, SpartaDosFileSystemLayout.DirectoryExtensionLength));
                var timestampValid = TryReadTimestamp(bytes.AsSpan(offset + SpartaDosFileSystemLayout.DirectoryDateOffset, SpartaDosFileSystemLayout.DateFieldLength),
                    bytes.AsSpan(offset + SpartaDosFileSystemLayout.DirectoryTimeOffset, SpartaDosFileSystemLayout.TimeFieldLength), out var created);
                var attributes = Attributes(flags);
                if (flags.HasFlag(SpartaDosDirectoryFlags.Subdirectory))
                {
                    var child = ReadDirectory(image, mapSector, ancestors, warnings);
                    entries.Add(new(entryName, FileSystemEntryKind.Directory, child.Length, created, string.Empty, (uint)flags,
                        mapSector, timestampValid, child.Children, nativeTypeId: SpartaDosFileSystemLayout.DirectoryNativeType,
                        occupiedSize: child.OccupiedSize, created: created, attributes: attributes, dataValid: true));
                }
                else
                {
                    var file = SpartaDosFileReader.Read(image, mapSector, entryLength);
                    if (file.IsSparse) attributes.Add(SpartaDosFileSystemLayout.SparseAttribute);
                    entries.Add(new(entryName, FileSystemEntryKind.File, entryLength, created, string.Empty, (uint)flags,
                        mapSector, timestampValid, [], file.Content, SpartaDosFileSystemLayout.FileNativeType,
                        file.OccupiedSize, created, attributes: attributes, dataValid: true));
                }
            }
            return new(name, length, directoryFile.OccupiedSize, entries.AsReadOnly());
        }
        finally
        {
            ancestors.Remove(firstMapSector);
        }
    }

    private static List<string> Attributes(SpartaDosDirectoryFlags flags)
    {
        var attributes = new List<string>();
        if (flags.HasFlag(SpartaDosDirectoryFlags.Protected)) attributes.Add(SpartaDosFileSystemLayout.ProtectedAttribute);
        if (flags.HasFlag(SpartaDosDirectoryFlags.Hidden)) attributes.Add(SpartaDosFileSystemLayout.HiddenAttribute);
        if (flags.HasFlag(SpartaDosDirectoryFlags.Archived)) attributes.Add(SpartaDosFileSystemLayout.ArchivedAttribute);
        return attributes;
    }

    private static int ReadUInt16(IReadOnlyList<byte> bytes, int offset) => bytes[offset] | bytes[offset + 1] << 8;

    private static int ReadLength(IReadOnlyList<byte> bytes, int offset) =>
        bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16;

    private static bool TryReadTimestamp(ReadOnlySpan<byte> date, ReadOnlySpan<byte> time, out DateTimeOffset? timestamp)
    {
        timestamp = null;
        if (date.IndexOfAnyExcept((byte)0) < 0 && time.IndexOfAnyExcept((byte)0) < 0) return true;
        try
        {
            timestamp = new DateTimeOffset(1900 + date[2], date[1], date[0], time[0], time[1], time[2], TimeSpan.Zero);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private sealed record DirectoryData(string Name, int Length, long OccupiedSize, IReadOnlyList<FileSystemEntry> Children);
}
