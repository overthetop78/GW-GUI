using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Crée un volume ProDOS avec répertoires, fichiers, index et bitmap.</summary>
public sealed class ProDosVolumeWriter
{
    /// <summary>Crée une image ProDOS de 140 ou 800 Kio.</summary>
    public SectorImage Create(MigrationPlan plan, string formatId)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var geometry = ResolveGeometry(formatId);
        var state = new BuildState(geometry.BlockCount);
        state.ReserveSystemBlocks();
        WriteRootDirectory(state, plan);
        WriteBitmap(state);
        return CreateImage(formatId, geometry, state.Blocks);
    }

    private static (int BlockCount, int Cylinders, int Heads, int SectorsPerTrack, Func<int, SectorAddress> Address) ResolveGeometry(string formatId)
    {
        if (formatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase)) return (AppleIIGeometry.TrackCount * AppleIIGeometry.ProDosBlocksPerTrack, AppleIIGeometry.TrackCount, 1, AppleIIGeometry.ProDosBlocksPerTrack, logical => new(logical / AppleIIGeometry.ProDosBlocksPerTrack, 0, logical % AppleIIGeometry.ProDosBlocksPerTrack));
        if (formatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase)) return (MacintoshGcrGeometry.SingleSidedBlockCount * MacintoshGcrGeometry.DoubleSidedHeadCount, MacintoshGcrGeometry.CylinderCount, MacintoshGcrGeometry.DoubleSidedHeadCount, MacintoshGcrGeometry.MaximumSectorsPerTrack, logical => MacintoshGcrGeometry.Address(logical, MacintoshGcrGeometry.DoubleSidedHeadCount));
        throw ProDosVolumeWriterExceptions.UnsupportedFormat(formatId);
    }

    private static void WriteRootDirectory(BuildState state, MigrationPlan plan)
    {
        if (plan.Entries.Count > ProDosFileSystemLayout.RootDirectoryBlockCount * ProDosFileSystemLayout.EntriesPerDirectoryBlock - 1) throw ProDosVolumeWriterExceptions.DiskFull();
        var rootBlocks = Enumerable.Range(ProDosFileSystemLayout.RootBlock, ProDosFileSystemLayout.RootDirectoryBlockCount).ToArray();
        LinkDirectoryBlocks(state.Blocks, rootBlocks);
        WriteVolumeHeader(state.Blocks[ProDosFileSystemLayout.RootBlock], plan.VolumeName, plan.Entries.Count, state.BlockCount);
        WriteEntries(state, plan.Entries, rootBlocks, ProDosFileSystemLayout.RootBlock, true);
    }

    private static void WriteEntries(BuildState state, IReadOnlyList<MigrationEntry> entries, IReadOnlyList<int> directoryBlocks, int headerBlock, bool volumeDirectory)
    {
        var slot = volumeDirectory ? 1 : 1;
        foreach (var entry in entries)
        {
            var blockIndex = slot / ProDosFileSystemLayout.EntriesPerDirectoryBlock;
            var entryIndex = slot % ProDosFileSystemLayout.EntriesPerDirectoryBlock;
            if (blockIndex >= directoryBlocks.Count) throw ProDosVolumeWriterExceptions.DiskFull();
            var offset = ProDosFileSystemLayout.HeaderOffset + entryIndex * ProDosFileSystemLayout.EntrySize;
            if (entry.Kind == FileSystemEntryKind.Directory) WriteDirectoryEntry(state, entry, state.Blocks[directoryBlocks[blockIndex]], offset, headerBlock, directoryBlocks[blockIndex], entryIndex + 1);
            else if (entry.Kind == FileSystemEntryKind.File && entry.Content is not null) WriteFileEntry(state, entry, state.Blocks[directoryBlocks[blockIndex]], offset, headerBlock);
            else throw new InvalidDataException($"The ProDOS entry '{entry.SourcePath}' is not writable.");
            slot++;
        }
    }

    private static void WriteDirectoryEntry(BuildState state, MigrationEntry entry, byte[] parent, int offset, int parentHeaderBlock, int parentBlock, int parentEntry)
    {
        var blockCount = Math.Max(1, (entry.Children.Count + 1 + ProDosFileSystemLayout.EntriesPerDirectoryBlock - 1) / ProDosFileSystemLayout.EntriesPerDirectoryBlock);
        var blocks = state.Allocate(blockCount);
        LinkDirectoryBlocks(state.Blocks, blocks);
        WriteSubdirectoryHeader(state.Blocks[blocks[0]], entry.TargetName, entry.Children.Count, parentBlock, parentEntry);
        WriteEntries(state, entry.Children, blocks, blocks[0], false);
        WriteEntry(parent, offset, entry, ProDosStorageType.Subdirectory, ProDosFileType.Directory, blocks[0], blocks.Count, checked(blocks.Count * ProDosFileSystemLayout.BlockSize), parentHeaderBlock);
    }

    private static void WriteFileEntry(BuildState state, MigrationEntry entry, byte[] parent, int offset, int parentHeaderBlock)
    {
        var content = entry.Content!.ToArray();
        if (content.Length > ProDosFileSystemLayout.MaximumFileLength) throw new InvalidDataException($"The ProDOS file '{entry.SourcePath}' exceeds the 24-bit length field.");
        var allocation = WriteFileStorage(state, content);
        WriteEntry(parent, offset, entry, allocation.StorageType, ProDosFileType.Binary, allocation.KeyBlock, allocation.BlocksUsed, content.Length, parentHeaderBlock);
    }

    private static (ProDosStorageType StorageType, int KeyBlock, int BlocksUsed) WriteFileStorage(BuildState state, byte[] content)
    {
        var dataCount = Math.Max(1, (content.Length + ProDosFileSystemLayout.BlockSize - 1) / ProDosFileSystemLayout.BlockSize);
        var dataBlocks = state.Allocate(dataCount);
        for (var index = 0; index < dataBlocks.Count; index++) content.AsSpan(index * ProDosFileSystemLayout.BlockSize, Math.Min(ProDosFileSystemLayout.BlockSize, Math.Max(0, content.Length - index * ProDosFileSystemLayout.BlockSize))).CopyTo(state.Blocks[dataBlocks[index]]);
        if (dataCount == 1) return (ProDosStorageType.Seedling, dataBlocks[0], 1);
        if (dataCount <= ProDosFileSystemLayout.IndexPointerCount)
        {
            var indexBlock = state.Allocate(1)[0];
            for (var index = 0; index < dataBlocks.Count; index++) ProDosPrimitives.WriteIndexPointer(state.Blocks[indexBlock], index, dataBlocks[index]);
            return (ProDosStorageType.Sapling, indexBlock, dataCount + 1);
        }
        var childCount = (dataCount + ProDosFileSystemLayout.IndexPointerCount - 1) / ProDosFileSystemLayout.IndexPointerCount;
        var master = state.Allocate(1)[0];
        var children = state.Allocate(childCount);
        for (var child = 0; child < children.Count; child++)
        {
            ProDosPrimitives.WriteIndexPointer(state.Blocks[master], child, children[child]);
            var first = child * ProDosFileSystemLayout.IndexPointerCount;
            var count = Math.Min(ProDosFileSystemLayout.IndexPointerCount, dataBlocks.Count - first);
            for (var index = 0; index < count; index++) ProDosPrimitives.WriteIndexPointer(state.Blocks[children[child]], index, dataBlocks[first + index]);
        }
        return (ProDosStorageType.Tree, master, dataCount + childCount + 1);
    }

    private static void WriteEntry(byte[] block, int offset, MigrationEntry entry, ProDosStorageType storageType, ProDosFileType fileType, int keyBlock, int blocksUsed, int length, int headerBlock)
    {
        WriteName(block, offset, storageType, entry.TargetName);
        block[offset + ProDosFileSystemLayout.FileTypeOffset] = (byte)fileType;
        ProDosPrimitives.WriteUInt16(block, offset + ProDosFileSystemLayout.KeyBlockOffset, keyBlock);
        ProDosPrimitives.WriteUInt16(block, offset + ProDosFileSystemLayout.BlocksUsedOffset, blocksUsed);
        ProDosPrimitives.WriteUInt24(block, offset + ProDosFileSystemLayout.EndOfFileOffset, length);
        ProDosDateTime.Write(block, offset + ProDosFileSystemLayout.CreatedDateOffset, entry.Modified);
        block[offset + ProDosFileSystemLayout.AccessOffset] = entry.RawAttributes is > 0 and <= byte.MaxValue ? (byte)entry.RawAttributes : ProDosFileSystemLayout.DefaultAccess;
        ProDosDateTime.Write(block, offset + ProDosFileSystemLayout.ModifiedDateOffset, entry.Modified);
        ProDosPrimitives.WriteUInt16(block, offset + ProDosFileSystemLayout.HeaderPointerOffset, headerBlock);
    }

    private static void WriteVolumeHeader(byte[] block, string name, int fileCount, int totalBlocks)
    {
        WriteName(block, ProDosFileSystemLayout.HeaderOffset, ProDosStorageType.VolumeHeader, name);
        ProDosDateTime.Write(block, ProDosFileSystemLayout.HeaderOffset + ProDosFileSystemLayout.CreatedDateOffset, DateTimeOffset.UtcNow);
        block[ProDosFileSystemLayout.HeaderOffset + ProDosFileSystemLayout.AccessOffset] = ProDosFileSystemLayout.DefaultAccess;
        block[ProDosFileSystemLayout.HeaderEntryLengthOffset] = ProDosFileSystemLayout.EntrySize;
        block[ProDosFileSystemLayout.HeaderEntryLengthOffset + 1] = ProDosFileSystemLayout.EntriesPerDirectoryBlock;
        ProDosPrimitives.WriteUInt16(block, ProDosFileSystemLayout.HeaderEntryLengthOffset + 2, fileCount);
        ProDosPrimitives.WriteUInt16(block, ProDosFileSystemLayout.BitmapBlockOffset, ProDosFileSystemLayout.DefaultBitmapBlock);
        ProDosPrimitives.WriteUInt16(block, ProDosFileSystemLayout.TotalBlocksOffset, totalBlocks);
    }

    private static void WriteSubdirectoryHeader(byte[] block, string name, int fileCount, int parentBlock, int parentEntry)
    {
        WriteName(block, ProDosFileSystemLayout.HeaderOffset, ProDosStorageType.SubdirectoryHeader, name);
        block[ProDosFileSystemLayout.SubdirectoryReservedOffset] = ProDosFileSystemLayout.SubdirectoryReservedValue;
        ProDosDateTime.Write(block, ProDosFileSystemLayout.HeaderOffset + ProDosFileSystemLayout.CreatedDateOffset, DateTimeOffset.UtcNow);
        block[ProDosFileSystemLayout.SubdirectoryVersionOffset] = ProDosFileSystemLayout.SubdirectoryVersion;
        block[ProDosFileSystemLayout.HeaderOffset + ProDosFileSystemLayout.AccessOffset] = ProDosFileSystemLayout.DefaultAccess;
        block[ProDosFileSystemLayout.HeaderEntryLengthOffset] = ProDosFileSystemLayout.EntrySize;
        block[ProDosFileSystemLayout.HeaderEntryLengthOffset + 1] = ProDosFileSystemLayout.EntriesPerDirectoryBlock;
        ProDosPrimitives.WriteUInt16(block, ProDosFileSystemLayout.HeaderEntryLengthOffset + 2, fileCount);
        ProDosPrimitives.WriteUInt16(block, ProDosFileSystemLayout.SubdirectoryParentBlockOffset, parentBlock);
        block[ProDosFileSystemLayout.SubdirectoryParentEntryOffset] = checked((byte)parentEntry);
        block[ProDosFileSystemLayout.SubdirectoryParentEntryLengthOffset] = ProDosFileSystemLayout.EntrySize;
    }

    private static void WriteName(byte[] block, int offset, ProDosStorageType storageType, string name)
    {
        if (!new ProDosNamePolicy().IsValid(name)) throw new InvalidDataException($"The ProDOS name '{name}' is invalid.");
        block[offset + ProDosFileSystemLayout.StorageAndNameLengthOffset] = (byte)((byte)storageType << ProDosFileSystemLayout.StorageTypeShift | name.Length);
        System.Text.Encoding.ASCII.GetBytes(name).CopyTo(block, offset + ProDosFileSystemLayout.NameOffset);
    }

    private static void LinkDirectoryBlocks(byte[][] blocks, IReadOnlyList<int> directoryBlocks)
    {
        for (var index = 0; index < directoryBlocks.Count; index++)
        {
            var block = blocks[directoryBlocks[index]];
            ProDosPrimitives.WriteUInt16(block, ProDosFileSystemLayout.PreviousBlockOffset, index == 0 ? 0 : directoryBlocks[index - 1]);
            ProDosPrimitives.WriteUInt16(block, ProDosFileSystemLayout.NextBlockOffset, index + 1 == directoryBlocks.Count ? 0 : directoryBlocks[index + 1]);
        }
    }

    private static void WriteBitmap(BuildState state)
    {
        var bitmap = state.Blocks[ProDosFileSystemLayout.DefaultBitmapBlock];
        for (var block = 0; block < state.BlockCount; block++) if (!state.Used[block]) bitmap[block / ProDosFileSystemLayout.BitsPerByte] |= (byte)(ProDosFileSystemLayout.BitmapHighBitMask >> (block & (ProDosFileSystemLayout.BitsPerByte - 1)));
    }

    private static SectorImage CreateImage(string formatId, (int BlockCount, int Cylinders, int Heads, int SectorsPerTrack, Func<int, SectorAddress> Address) geometry, byte[][] data)
    {
        var blocks = data.Select((block, logical) => new SectorBlock(logical, geometry.Address(logical), block));
        return new(formatId, ProDosFileSystemLayout.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks, logicalBlockCount: geometry.BlockCount);
    }

    private sealed class BuildState(int blockCount)
    {
        public int BlockCount { get; } = blockCount;
        public byte[][] Blocks { get; } = Enumerable.Range(0, blockCount).Select(_ => new byte[ProDosFileSystemLayout.BlockSize]).ToArray();
        public bool[] Used { get; } = new bool[blockCount];

        public void ReserveSystemBlocks()
        {
            for (var block = 0; block <= ProDosFileSystemLayout.DefaultBitmapBlock; block++) Used[block] = true;
        }

        public IReadOnlyList<int> Allocate(int count)
        {
            var allocated = Enumerable.Range(ProDosFileSystemLayout.DefaultBitmapBlock + 1, BlockCount - ProDosFileSystemLayout.DefaultBitmapBlock - 1).Where(block => !Used[block]).Take(count).ToArray();
            if (allocated.Length != count) throw ProDosVolumeWriterExceptions.DiskFull();
            foreach (var block in allocated) Used[block] = true;
            return allocated;
        }
    }
}
