using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

/// <summary>Lit les blocs et entrées du répertoire plat MFS.</summary>
internal static class MacMfsDirectoryReader
{
    /// <summary>Lit les entrées valides de la plage de répertoire annoncée.</summary>
    public static IReadOnlyList<FileSystemEntry> Read(SectorImage image, int directoryStart, int directoryLength, MacMfsAllocationMap map, int allocationStart, uint allocationSize, List<string> warnings)
    {
        var entries = new List<FileSystemEntry>();
        var directoryEnd = Math.Min((long)directoryStart + directoryLength, image.BlockCount);
        if (directoryStart < 0 || directoryLength < 0 || directoryStart > image.BlockCount || directoryEnd < directoryStart) return entries;
        for (var blockNumber = directoryStart; blockNumber < directoryEnd; blockNumber++) ReadBlock(image, blockNumber, map, allocationStart, allocationSize, entries, warnings);
        return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Lit toutes les entrées actives d'un bloc de répertoire présent.</summary>
    private static void ReadBlock(SectorImage image, int blockNumber, MacMfsAllocationMap map, int allocationStart, uint allocationSize, ICollection<FileSystemEntry> entries, List<string> warnings)
    {
        var read = MacMfsBlockReader.Read(image, blockNumber, 1, MacMfsFileSystemLayout.DirectoryDescription, warnings);
        if (!read.IsValid) return;
        var bytes = read.Bytes.ToArray();
        var offset = 0;
        while (offset + MacMfsFileSystemLayout.MinimumDirectoryEntryLength <= bytes.Length && (bytes[offset + MacMfsFileSystemLayout.FlagsOffset] & MacMfsFileSystemLayout.ActiveEntryMask) != 0)
        {
            var entryOffset = offset;
            var flags = bytes[offset + MacMfsFileSystemLayout.FlagsOffset];
            var finderInfo = bytes.AsSpan(offset + MacMfsFileSystemLayout.FinderInfoOffset, MacMfsFileSystemLayout.FinderInfoLength);
            var fileNumber = MacFileSystemPrimitives.ReadUInt32(bytes, offset + MacMfsFileSystemLayout.FileNumberOffset);
            var dataStart = MacFileSystemPrimitives.ReadUInt16(bytes, offset + MacMfsFileSystemLayout.DataForkStartOffset);
            var dataLength = MacFileSystemPrimitives.ReadUInt32(bytes, offset + MacMfsFileSystemLayout.DataForkLogicalLengthOffset);
            var resourceStart = MacFileSystemPrimitives.ReadUInt16(bytes, offset + MacMfsFileSystemLayout.ResourceForkStartOffset);
            var resourceLength = MacFileSystemPrimitives.ReadUInt32(bytes, offset + MacMfsFileSystemLayout.ResourceForkLogicalLengthOffset);
            var modified = MacFileSystemTime.FromSeconds(MacFileSystemPrimitives.ReadUInt32(bytes, offset + MacMfsFileSystemLayout.ModifiedDateOffset));
            var nameLength = bytes[offset + MacMfsFileSystemLayout.NameLengthOffset];
            if (nameLength > MacMfsFileSystemLayout.MaximumNameLength || offset + MacMfsFileSystemLayout.NameOffset + nameLength > bytes.Length)
            {
                warnings.Add(MacFileSystemExceptions.InvalidDirectoryEntry(blockNumber, entryOffset));
                break;
            }
            var name = MacFileSystemPrimitives.DecodeName(bytes.AsSpan(offset + MacMfsFileSystemLayout.NameOffset, nameLength));
            var dataFork = MacMfsForkReader.Read(image, map, allocationStart, allocationSize, dataStart, dataLength, name, MacMfsFileSystemLayout.DataForkName, warnings);
            var resourceFork = MacMfsForkReader.Read(image, map, allocationStart, allocationSize, resourceStart, resourceLength, name, MacMfsFileSystemLayout.ResourceForkName, warnings);
            var type = System.Text.Encoding.ASCII.GetString(finderInfo[..MacMfsFileSystemLayout.FinderTypeLength]).Trim('\0', ' ');
            var description = string.IsNullOrWhiteSpace(type) ? MacMfsFileSystemLayout.DefaultFileDescription : type;
            entries.Add(new(name, FileSystemEntryKind.File, (long)dataLength + resourceLength, modified, description, flags, MacFileSystemPrimitives.ToStorageReference(fileNumber), dataFork.IsValid && resourceFork.IsValid, [], SelectExposedContent(dataFork, resourceFork)));
            offset += MacMfsFileSystemLayout.NameOffset + nameLength;
            if (offset % MacMfsFileSystemLayout.EntryAlignment != 0) offset++;
            if (offset <= entryOffset) break;
        }
    }

    /// <summary>Expose le data fork lorsqu'il contient des octets, sinon le resource fork utilisé par de nombreuses applications Macintosh classiques.</summary>
    private static IReadOnlyList<byte> SelectExposedContent(MacMfsForkResult dataFork, MacMfsForkResult resourceFork) => dataFork.Content.Count > 0 ? dataFork.Content : resourceFork.Content;
}
