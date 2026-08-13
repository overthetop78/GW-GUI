using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Assemble les structures internes d'un volume Commodore DOS.</summary>
internal sealed class CommodoreDosVolumeBuilder(MigrationPlan plan, CommodoreDosWritableGeometry geometry, CommodoreDosWritePolicy policy)
{
    private readonly byte[][] _sectors = Enumerable.Range(0, geometry.BlockCount).Select(_ => new byte[CommodoreDosLayout.SectorSize]).ToArray();
    private readonly bool[] _free = Enumerable.Repeat(true, geometry.BlockCount).ToArray();
    private readonly List<int> _directorySectors = [];

    /// <summary>Construit l'image après validation des entrées plates.</summary>
    public SectorImage Build()
    {
        ValidatePlan();
        ReserveSystemSectors();
        AllocateDirectorySectors();
        WriteFilesAndDirectory();
        WriteSystemSectors();
        var blocks = _sectors.Select((data, logical) => new SectorBlock(logical, geometry.CreateAddress(logical), data));
        return new(geometry.FormatId, CommodoreDosLayout.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks, capacity: (long)geometry.BlockCount * CommodoreDosLayout.SectorSize, logicalBlockCount: geometry.BlockCount);
    }

    private void ValidatePlan()
    {
        var names = new CommodoreDosNamePolicy();
        if (!names.IsValid(plan.VolumeName)) throw CommodoreDosVolumeWriterExceptions.InvalidEntry("/");
        foreach (var entry in plan.Entries)
        {
            if (entry.Kind != FileSystemEntryKind.File || entry.Content is null || entry.Children.Count != 0 || !names.IsValid(entry.TargetName)) throw CommodoreDosVolumeWriterExceptions.InvalidEntry(entry.SourcePath);
        }
    }

    private void ReserveSystemSectors()
    {
        if (geometry.FormatId == DiskImageFormatIds.Commodore1581)
        {
            Reserve(Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.HeaderSector);
            Reserve(Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.FirstBamSector);
            Reserve(Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.SecondBamSector);
            return;
        }
        Reserve(Commodore1541DosLayout.HeaderTrack, Commodore1541DosLayout.HeaderSector);
        if (geometry.FormatId == DiskImageFormatIds.Commodore1571) Reserve(Commodore1541DosLayout.HeaderTrack + geometry.Cylinders, Commodore1541DosLayout.HeaderSector);
    }

    private void AllocateDirectorySectors()
    {
        var count = Math.Max(1, DivideRoundUp(plan.Entries.Count, CommodoreDosLayout.DirectoryEntryCount));
        for (var index = 0; index < count; index++)
        {
            var preferredSector = (geometry.FormatId == DiskImageFormatIds.Commodore1581 ? Commodore1581DosLayout.DirectorySector : Commodore1541DosLayout.DirectorySector) + index;
            var preferredTrack = geometry.FormatId == DiskImageFormatIds.Commodore1581 ? Commodore1581DosLayout.HeaderTrack : Commodore1541DosLayout.HeaderTrack;
            var logical = TryAllocate(preferredTrack, preferredSector) ?? Allocate(1)[0];
            _directorySectors.Add(logical);
        }
    }

    private void WriteFilesAndDirectory()
    {
        for (var index = 0; index < plan.Entries.Count; index++) WriteFileEntry(plan.Entries[index], index);
        for (var index = 0; index < _directorySectors.Count; index++)
        {
            var directory = _sectors[_directorySectors[index]];
            if (index + 1 < _directorySectors.Count) WriteAddress(directory, 0, _directorySectors[index + 1]);
        }
    }

    private void WriteFileEntry(MigrationEntry entry, int index)
    {
        var rawType = ResolveFileType(entry);
        var content = entry.Content!.ToArray();
        var dataBlocks = Allocate(DivideRoundUp(content.Length, CommodoreDosLayout.DataBytesPerSector));
        WriteDataChain(content, dataBlocks);
        var relative = (rawType & CommodoreDosFileType.BaseTypeMask) == CommodoreDosFileType.Rel;
        var recordLength = relative ? ResolveRelativeRecordLength(entry) : (byte)0;
        var sideBlocks = relative ? WriteRelativeSideSectors(entry.SourcePath, dataBlocks, recordLength) : [];
        var directory = _sectors[_directorySectors[index / CommodoreDosLayout.DirectoryEntryCount]];
        var offset = index % CommodoreDosLayout.DirectoryEntryCount * CommodoreDosLayout.DirectoryEntrySize;
        directory[offset + CommodoreDosLayout.FileTypeOffset] = (byte)rawType;
        if (dataBlocks.Count > 0) WriteAddress(directory, offset + CommodoreDosLayout.FirstDataTrackOffset, dataBlocks[0]);
        PetsciiCodec.Encode(entry.TargetName, CommodoreDosLayout.NameLength).CopyTo(directory, offset + CommodoreDosLayout.FileNameOffset);
        if (sideBlocks.Count > 0) WriteAddress(directory, offset + CommodoreDosLayout.RelativeSideTrackOffset, sideBlocks[0]);
        if (relative) directory[offset + CommodoreDosLayout.RelativeRecordLengthOffset] = recordLength;
        BinaryPrimitives.WriteUInt16LittleEndian(directory.AsSpan(offset + CommodoreDosLayout.DeclaredBlockCountOffset), checked((ushort)(dataBlocks.Count + sideBlocks.Count)));
    }

    private CommodoreDosFileType ResolveFileType(MigrationEntry entry)
    {
        if (!plan.SourceFileSystemId.Equals(FileSystemIds.CommodoreDos, StringComparison.OrdinalIgnoreCase)) return policy.DefaultFileType;
        var rawType = (CommodoreDosFileType)(byte)entry.RawAttributes;
        var baseType = rawType & CommodoreDosFileType.BaseTypeMask;
        if (baseType is not (CommodoreDosFileType.Seq or CommodoreDosFileType.Prg or CommodoreDosFileType.Usr or CommodoreDosFileType.Rel)) throw CommodoreDosVolumeWriterExceptions.InvalidEntry(entry.SourcePath);
        return rawType;
    }

    private byte ResolveRelativeRecordLength(MigrationEntry entry)
    {
        if (!plan.SourceFileSystemId.Equals(FileSystemIds.CommodoreDos, StringComparison.OrdinalIgnoreCase)) return policy.RelativeRecordLength;
        var length = (byte)(entry.RawAttributes >> CommodoreDosLayout.RelativeRecordLengthAttributeShift);
        if (length == 0) throw CommodoreDosVolumeWriterExceptions.InvalidEntry(entry.SourcePath);
        return length;
    }

    private void WriteDataChain(ReadOnlySpan<byte> content, IReadOnlyList<int> blocks)
    {
        for (var index = 0; index < blocks.Count; index++)
        {
            var sector = _sectors[blocks[index]];
            var used = Math.Min(CommodoreDosLayout.DataBytesPerSector, content.Length - index * CommodoreDosLayout.DataBytesPerSector);
            if (index + 1 < blocks.Count) WriteAddress(sector, 0, blocks[index + 1]);
            else sector[CommodoreDosLayout.NextSectorOffset] = checked((byte)(used + 1));
            content.Slice(index * CommodoreDosLayout.DataBytesPerSector, used).CopyTo(sector.AsSpan(CommodoreDosLayout.LinkLength));
        }
    }

    private IReadOnlyList<int> WriteRelativeSideSectors(string path, IReadOnlyList<int> dataBlocks, byte recordLength)
    {
        var count = Math.Max(1, DivideRoundUp(dataBlocks.Count, CommodoreDosLayout.RelativeDataPointersPerSideSector));
        if (count > CommodoreDosLayout.MaximumRelativeSideSectors) throw CommodoreDosVolumeWriterExceptions.RelativeFileTooLarge(path);
        var sideBlocks = Allocate(count);
        for (var index = 0; index < sideBlocks.Count; index++)
        {
            var sector = _sectors[sideBlocks[index]];
            if (index + 1 < sideBlocks.Count) WriteAddress(sector, 0, sideBlocks[index + 1]);
            sector[CommodoreDosLayout.RelativeSideNumberOffset] = checked((byte)index);
            sector[CommodoreDosLayout.RelativeSideRecordLengthOffset] = recordLength;
            for (var side = 0; side < sideBlocks.Count; side++) WriteAddress(sector, CommodoreDosLayout.RelativeSideTableOffset + side * 2, sideBlocks[side]);
            var first = index * CommodoreDosLayout.RelativeDataPointersPerSideSector;
            var pointers = Math.Min(CommodoreDosLayout.RelativeDataPointersPerSideSector, dataBlocks.Count - first);
            for (var pointer = 0; pointer < pointers; pointer++) WriteAddress(sector, CommodoreDosLayout.RelativeDataPointersOffset + pointer * 2, dataBlocks[first + pointer]);
        }
        return sideBlocks;
    }

    private void WriteSystemSectors()
    {
        if (geometry.FormatId == DiskImageFormatIds.Commodore1581) Write1581SystemSectors();
        else Write1541SystemSectors();
    }

    private void Write1541SystemSectors()
    {
        var header = GetSector(Commodore1541DosLayout.HeaderTrack, Commodore1541DosLayout.HeaderSector);
        WriteAddress(header, 0, _directorySectors[0]);
        header[CommodoreDosLayout.DirectoryEntriesOffset] = Commodore1541DosLayout.HeaderSignature;
        PetsciiCodec.Encode(plan.VolumeName, CommodoreDosLayout.NameLength).CopyTo(header, Commodore1541DosLayout.VolumeNameOffset);
        WriteDiskIdentity(header, Commodore1541DosLayout.DiskIdOffset, Commodore1541DosLayout.DosTypeOffset, Commodore1541DosLayout.DosType);
        Write1541Bam(header, 0);
        if (geometry.FormatId == DiskImageFormatIds.Commodore1571)
        {
            var second = GetSector(Commodore1541DosLayout.HeaderTrack + geometry.Cylinders, Commodore1541DosLayout.HeaderSector);
            second[CommodoreDosLayout.DirectoryEntriesOffset] = Commodore1541DosLayout.HeaderSignature;
            Write1541Bam(second, 1);
        }
    }

    private void Write1541Bam(Span<byte> bam, int side)
    {
        for (var track = 1; track <= geometry.Cylinders; track++)
        {
            var globalTrack = track + side * geometry.Cylinders;
            var sectorCount = Geometries.Commodore.Commodore1541Geometry.SectorsPerTrack(track);
            var offset = Commodore1541DosLayout.BamEntriesOffset + (track - 1) * Commodore1541DosLayout.BamEntrySize;
            var freeCount = 0;
            for (var sector = 0; sector < sectorCount; sector++)
            {
                if (!_free[geometry.ToLogicalBlock(globalTrack, sector)]) continue;
                freeCount++;
                bam[offset + 1 + sector / 8] |= checked((byte)(1 << (sector % 8)));
            }
            bam[offset] = checked((byte)freeCount);
        }
    }

    private void Write1581SystemSectors()
    {
        var header = GetSector(Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.HeaderSector);
        WriteAddress(header, 0, _directorySectors[0]);
        header[CommodoreDosLayout.DirectoryEntriesOffset] = Commodore1581DosLayout.HeaderSignature;
        PetsciiCodec.Encode(plan.VolumeName, CommodoreDosLayout.NameLength).CopyTo(header, Commodore1581DosLayout.VolumeNameOffset);
        WriteDiskIdentity(header, Commodore1581DosLayout.DiskIdOffset, Commodore1581DosLayout.DosTypeOffset, Commodore1581DosLayout.DosType);
        for (var side = 0; side < 2; side++)
        {
            var sectorNumber = side == 0 ? Commodore1581DosLayout.FirstBamSector : Commodore1581DosLayout.SecondBamSector;
            var bam = GetSector(Commodore1581DosLayout.HeaderTrack, sectorNumber);
            if (side == 0) WriteAddress(bam, 0, geometry.ToLogicalBlock(Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.SecondBamSector));
            bam[CommodoreDosLayout.DirectoryEntriesOffset] = Commodore1581DosLayout.HeaderSignature;
            WriteDiskIdentity(bam, Commodore1581DosLayout.BamDiskIdOffset, -1, []);
            Write1581Bam(bam, side * Commodore1581DosLayout.BamEntryCount + 1);
        }
    }

    private void Write1581Bam(Span<byte> bam, int firstTrack)
    {
        for (var index = 0; index < Commodore1581DosLayout.BamEntryCount; index++)
        {
            var track = firstTrack + index;
            var offset = Commodore1581DosLayout.BamEntriesOffset + index * Commodore1581DosLayout.BamEntrySize;
            var freeCount = 0;
            for (var sector = 0; sector < geometry.SectorsPerTrack; sector++)
            {
                if (!_free[geometry.ToLogicalBlock(track, sector)]) continue;
                freeCount++;
                bam[offset + 1 + sector / 8] |= checked((byte)(1 << (sector % 8)));
            }
            bam[offset] = checked((byte)freeCount);
        }
    }

    private static void WriteDiskIdentity(Span<byte> sector, int idOffset, int dosTypeOffset, ReadOnlySpan<byte> dosType)
    {
        sector[idOffset] = (byte)'G';
        sector[idOffset + 1] = (byte)'W';
        if (dosTypeOffset >= 0) dosType.CopyTo(sector[dosTypeOffset..]);
    }

    private byte[] GetSector(int track, int sector) => _sectors[geometry.ToLogicalBlock(track, sector)];

    private void Reserve(int track, int sector) => _free[geometry.ToLogicalBlock(track, sector)] = false;

    private int? TryAllocate(int track, int sector)
    {
        try
        {
            var logical = geometry.ToLogicalBlock(track, sector);
            if (!_free[logical]) return null;
            _free[logical] = false;
            return logical;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private IReadOnlyList<int> Allocate(int count)
    {
        if (count == 0) return [];
        var center = geometry.FormatId == DiskImageFormatIds.Commodore1581 ? Commodore1581DosLayout.HeaderTrack : Commodore1541DosLayout.HeaderTrack;
        var selected = Enumerable.Range(0, geometry.BlockCount).Where(logical => _free[logical]).OrderBy(logical => Math.Abs(geometry.FromLogicalBlock(logical).Track - center)).ThenBy(logical => logical).Take(count).ToArray();
        if (selected.Length != count) throw CommodoreDosVolumeWriterExceptions.DiskFull();
        foreach (var logical in selected) _free[logical] = false;
        return selected;
    }

    private void WriteAddress(Span<byte> target, int offset, int logicalBlock)
    {
        var address = geometry.FromLogicalBlock(logicalBlock);
        target[offset] = checked((byte)address.Track);
        target[offset + 1] = checked((byte)address.Sector);
    }

    private static int DivideRoundUp(int value, int divisor) => value == 0 ? 0 : (value + divisor - 1) / divisor;
}
