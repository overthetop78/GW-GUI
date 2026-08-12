using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Parcourt les chaînes de blocs des répertoires ProDOS.</summary>
internal static class ProDosDirectoryReader
{
    /// <summary>Lit un répertoire et ses sous-répertoires.</summary>
    public static ProDosDirectoryResult Read(SectorImage image, int firstBlock, List<string> warnings, ISet<int> globalVisited, int depth)
    {
        if (depth > ProDosFileSystemLayout.MaximumDirectoryDepth)
        {
            warnings.Add(ProDosFileSystemExceptions.DirectoryDepthExceeded(depth, firstBlock));
            return new([], false);
        }
        var entries = new List<FileSystemEntry>();
        var blockNumber = firstBlock;
        var chain = new HashSet<int>();
        var first = true;
        var valid = true;
        while (blockNumber != 0)
        {
            var cyclic = !chain.Add(blockNumber);
            var reused = !cyclic && !globalVisited.Add(blockNumber);
            if (cyclic || reused || blockNumber < 0 || blockNumber >= image.BlockCount || !image.TryGetBlock(blockNumber, out var block) || block.Data.Count != ProDosFileSystemLayout.BlockSize)
            {
                warnings.Add(ProDosFileSystemExceptions.InvalidDirectoryBlock(blockNumber, cyclic, reused));
                valid = false;
                break;
            }
            var bytes = block.Data.ToArray();
            var start = first ? ProDosFileSystemLayout.FirstVolumeEntryOffset : ProDosFileSystemLayout.FirstChainedEntryOffset;
            for (var offset = start; offset + ProDosFileSystemLayout.EntrySize <= bytes.Length; offset += ProDosFileSystemLayout.EntrySize) ReadEntry(image, bytes, offset, warnings, globalVisited, depth, entries);
            blockNumber = ProDosPrimitives.ReadUInt16(bytes, ProDosFileSystemLayout.NextBlockOffset);
            first = false;
        }
        return new(entries.OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), valid);
    }

    /// <summary>Lit une entrée de répertoire active.</summary>
    private static void ReadEntry(SectorImage image, ReadOnlySpan<byte> bytes, int offset, List<string> warnings, ISet<int> globalVisited, int depth, ICollection<FileSystemEntry> entries)
    {
        var storageType = (ProDosStorageType)(bytes[offset] >> ProDosFileSystemLayout.StorageTypeShift);
        var nameLength = bytes[offset] & ProDosFileSystemLayout.NameLengthMask;
        if (storageType == ProDosStorageType.Inactive || nameLength is 0 or > ProDosFileSystemLayout.MaximumNameLength) return;
        var name = ProDosPrimitives.ReadName(bytes, offset);
        var keyBlock = ProDosPrimitives.ReadUInt16(bytes, offset + ProDosFileSystemLayout.KeyBlockOffset);
        var endOfFile = ProDosPrimitives.ReadUInt24(bytes, offset + ProDosFileSystemLayout.EndOfFileOffset);
        var fileType = (ProDosFileType)bytes[offset + ProDosFileSystemLayout.FileTypeOffset];
        var modified = ProDosDateTime.Read(bytes, offset + ProDosFileSystemLayout.ModifiedDateOffset);
        if (storageType == ProDosStorageType.Subdirectory)
        {
            var children = Read(image, keyBlock, warnings, globalVisited, depth + 1);
            entries.Add(new(name, FileSystemEntryKind.Directory, 0, modified, ProDosFileTypeNames.Get(fileType), bytes[offset + ProDosFileSystemLayout.AccessOffset], keyBlock, children.IsValid, children.Entries));
        }
        else if (storageType is ProDosStorageType.Seedling or ProDosStorageType.Sapling or ProDosStorageType.Tree)
        {
            var content = ProDosFileContentReader.Read(image, storageType, keyBlock, endOfFile, name, warnings);
            entries.Add(new(name, FileSystemEntryKind.File, endOfFile, modified, ProDosFileTypeNames.Get(fileType), bytes[offset + ProDosFileSystemLayout.AccessOffset], keyBlock, content.IsValid, [], content.Content));
        }
    }
}
