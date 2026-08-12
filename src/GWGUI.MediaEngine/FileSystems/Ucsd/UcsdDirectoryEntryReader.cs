using System.Collections.Frozen;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Valide et lit les entrées du répertoire UCSD.</summary>
internal static class UcsdDirectoryEntryReader
{
    /// <summary>Lit les entrées qui ne traversent aucun bloc de répertoire invalide.</summary>
    public static UcsdDirectoryEntriesResult Read(SectorImage image, UcsdDirectoryHeader header, UcsdBlockReadResult directory, List<string> warnings)
    {
        var entries = new List<FileSystemEntry>();
        var usedBlocks = Enumerable.Range(UcsdFileSystemLayout.DirectoryBlock, header.DirectoryBlockCount).ToHashSet();
        var valid = directory.IsValid;
        var bytes = directory.Bytes.ToArray();
        var capacityEntries = Math.Max(0, (bytes.Length - UcsdFileSystemLayout.EntrySize) / UcsdFileSystemLayout.EntrySize);
        var maximumEntries = Math.Min(header.DeclaredFiles, Math.Min(UcsdFileSystemLayout.MaximumFileCount, capacityEntries));
        for (var index = 0; index < maximumEntries; index++) ReadEntry(image, header, directory, bytes, index, entries, usedBlocks, warnings, ref valid);
        if (header.DeclaredFiles != entries.Count) warnings.Add(UcsdFileSystemExceptions.DeclaredFileCountMismatch(header.DeclaredFiles, entries.Count));
        return new(entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), usedBlocks.ToFrozenSet(), valid);
    }

    /// <summary>Lit une entrée après validation de sa position, de son nom et de sa plage.</summary>
    private static void ReadEntry(SectorImage image, UcsdDirectoryHeader header, UcsdBlockReadResult directory, byte[] bytes, int index, ICollection<FileSystemEntry> entries, ISet<int> usedBlocks, ICollection<string> warnings, ref bool valid)
    {
        var entryIndex = index + 1;
        var offset = entryIndex * UcsdFileSystemLayout.EntrySize;
        var firstDirectoryBlock = offset / UcsdFileSystemLayout.BlockSize;
        var lastDirectoryBlock = (offset + UcsdFileSystemLayout.EntrySize - 1) / UcsdFileSystemLayout.BlockSize;
        if (!directory.PresentBlocks[firstDirectoryBlock] || !directory.PresentBlocks[lastDirectoryBlock]) { valid = false; return; }
        var entry = bytes.AsSpan(offset, UcsdFileSystemLayout.EntrySize);
        var firstBlock = UcsdPrimitives.ReadUInt16(entry, UcsdFileSystemLayout.EntryFirstBlockOffset, header.ByteOrder);
        var lastBlock = UcsdPrimitives.ReadUInt16(entry, UcsdFileSystemLayout.EntryLastBlockOffset, header.ByteOrder);
        if (firstBlock == 0 && lastBlock == 0) return;
        var name = UcsdName.Decode(entry.Slice(UcsdFileSystemLayout.EntryNameOffset, UcsdFileSystemLayout.EntryNameFieldLength), UcsdFileSystemLayout.MaximumFileNameLength);
        if (name.Length == 0) { warnings.Add(UcsdFileSystemExceptions.InvalidName(entryIndex, name)); valid = false; return; }
        if (lastBlock <= firstBlock || lastBlock > header.TotalBlocks || firstBlock < header.EndDirectory) { warnings.Add(UcsdFileSystemExceptions.InvalidRange(entryIndex, name, firstBlock, lastBlock, header.TotalBlocks)); valid = false; return; }
        var range = Enumerable.Range(firstBlock, lastBlock - firstBlock).ToArray();
        var overlap = range.Where(usedBlocks.Contains).ToArray();
        if (overlap.Length > 0) { warnings.Add(UcsdFileSystemExceptions.Overlap(entryIndex, name, overlap)); valid = false; }
        foreach (var block in range) usedBlocks.Add(block);
        var lastBytes = UcsdPrimitives.ReadUInt16(entry, UcsdFileSystemLayout.EntryLastBlockBytesOffset, header.ByteOrder);
        var content = UcsdFileContentReader.Read(image, firstBlock, lastBlock, lastBytes, name, warnings);
        var kind = (UcsdFileKind)(UcsdPrimitives.ReadUInt16(entry, UcsdFileSystemLayout.EntryKindOffset, header.ByteOrder) & UcsdFileSystemLayout.FileKindMask);
        entries.Add(new(name, FileSystemEntryKind.File, content.Size, UcsdDate.Decode(UcsdPrimitives.ReadUInt16(entry, UcsdFileSystemLayout.EntryDateOffset, header.ByteOrder)), UcsdFileKindNames.Get(kind), 0, firstBlock, content.IsValid && overlap.Length == 0, [], content.Content));
        valid &= content.IsValid;
    }
}
