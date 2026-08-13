using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Crée des volumes Apple DOS 3.2 ou 3.3 complets à partir d'un plan de migration validé.</summary>
public sealed class AppleDosVolumeWriter
{
    /// <summary>Crée l'image sectorielle contenant le VTOC, le catalogue, les listes T/S et les données.</summary>
    public SectorImage Create(MigrationPlan plan, string formatId)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var sectorsPerTrack = ResolveSectorsPerTrack(formatId);
        if (!AppleDosVolumeNamePolicy.TryParse(plan.VolumeName, out var volumeNumber)) throw new InvalidDataException($"The Apple DOS volume name '{plan.VolumeName}' must use DOS-nnn.");
        if (plan.Entries.Any(entry => entry.Kind != FileSystemEntryKind.File || entry.Children.Count != 0 || entry.Content is null)) throw new InvalidDataException("Apple DOS supports only files with available content in the root catalog.");
        var capacity = checked(AppleDosFileSystemLayout.TrackCount * sectorsPerTrack);
        var sectors = Enumerable.Range(0, capacity).Select(_ => new byte[AppleDosFileSystemLayout.SectorSize]).ToArray();
        var free = Enumerable.Repeat(true, capacity).ToArray();
        ReserveTrack(free, sectorsPerTrack, 0);
        ReserveTrack(free, sectorsPerTrack, AppleDosFileSystemLayout.VtocTrack);
        WriteCatalogChain(sectors, sectorsPerTrack);
        WriteFiles(plan, sectors, free, sectorsPerTrack);
        WriteVtoc(sectors, free, sectorsPerTrack, volumeNumber);
        var blocks = sectors.Select((data, logical) => new SectorBlock(logical, new(logical / sectorsPerTrack, 0, logical % sectorsPerTrack), data));
        return new(formatId, AppleDosFileSystemLayout.SectorSize, AppleDosFileSystemLayout.TrackCount, 1, sectorsPerTrack, blocks);
    }

    private static int ResolveSectorsPerTrack(string formatId)
    {
        if (formatId.Equals(DiskImageFormatIds.AppleIIDos32, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase)) return AppleDosFileSystemLayout.Dos32SectorsPerTrack;
        if (formatId.Equals(DiskImageFormatIds.AppleIIDos33, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase)) return AppleDosFileSystemLayout.Dos33SectorsPerTrack;
        throw AppleDosVolumeWriterExceptions.UnsupportedFormat(formatId);
    }

    private static void ReserveTrack(bool[] free, int sectorsPerTrack, int track)
    {
        for (var sector = 0; sector < sectorsPerTrack; sector++) free[track * sectorsPerTrack + sector] = false;
    }

    private static void WriteCatalogChain(byte[][] sectors, int sectorsPerTrack)
    {
        for (var sector = sectorsPerTrack - 1; sector > 0; sector--)
        {
            var catalog = sectors[AppleDosFileSystemLayout.VtocTrack * sectorsPerTrack + sector];
            catalog[AppleDosFileSystemLayout.NextTrackOffset] = sector == 1 ? (byte)0 : (byte)AppleDosFileSystemLayout.VtocTrack;
            catalog[AppleDosFileSystemLayout.NextSectorOffset] = sector == 1 ? (byte)0 : (byte)(sector - 1);
        }
    }

    private static void WriteFiles(MigrationPlan plan, byte[][] sectors, bool[] free, int sectorsPerTrack)
    {
        var entries = plan.Entries;
        var maximumEntries = (sectorsPerTrack - 1) * AppleDosFileSystemLayout.CatalogEntriesPerSector;
        if (entries.Count > maximumEntries) throw AppleDosVolumeWriterExceptions.DiskFull();
        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];
            var rawType = AppleDosFileTypeValue(entry, plan.SourceFileSystemId);
            var content = EncodeContent(entry, rawType);
            var dataCount = Math.Max(1, (content.Length + AppleDosFileSystemLayout.SectorSize - 1) / AppleDosFileSystemLayout.SectorSize);
            var listCount = (dataCount + AppleDosFileSystemLayout.TrackSectorPairCount - 1) / AppleDosFileSystemLayout.TrackSectorPairCount;
            var allocated = Allocate(free, dataCount + listCount, sectorsPerTrack);
            var lists = allocated.Take(listCount).ToArray();
            var dataSectors = allocated.Skip(listCount).ToArray();
            WriteData(content, dataSectors, sectors);
            WriteLists(lists, dataSectors, sectors, sectorsPerTrack);
            WriteCatalogEntry(entry, rawType, index, lists[0], dataCount + listCount, sectors, sectorsPerTrack);
        }
    }

    private static int[] Allocate(bool[] free, int count, int sectorsPerTrack)
    {
        var candidates = Enumerable.Range(0, AppleDosFileSystemLayout.TrackCount).OrderBy(track => Math.Abs(track - (AppleDosFileSystemLayout.VtocTrack - 1))).ThenBy(track => track).SelectMany(track => Enumerable.Range(0, sectorsPerTrack).Select(sector => track * sectorsPerTrack + sector)).Where(logical => free[logical]).Take(count).ToArray();
        if (candidates.Length != count) throw AppleDosVolumeWriterExceptions.DiskFull();
        foreach (var logical in candidates) free[logical] = false;
        return candidates;
    }

    private static void WriteData(byte[] content, IReadOnlyList<int> dataSectors, byte[][] sectors)
    {
        for (var index = 0; index < dataSectors.Count; index++) content.AsSpan(index * AppleDosFileSystemLayout.SectorSize, Math.Min(AppleDosFileSystemLayout.SectorSize, Math.Max(0, content.Length - index * AppleDosFileSystemLayout.SectorSize))).CopyTo(sectors[dataSectors[index]]);
    }

    private static void WriteLists(IReadOnlyList<int> lists, IReadOnlyList<int> dataSectors, byte[][] sectors, int sectorsPerTrack)
    {
        for (var listIndex = 0; listIndex < lists.Count; listIndex++)
        {
            var list = sectors[lists[listIndex]];
            if (listIndex + 1 < lists.Count) WriteAddress(list, AppleDosFileSystemLayout.NextTrackOffset, lists[listIndex + 1], sectorsPerTrack);
            BinaryPrimitives.WriteUInt16LittleEndian(list.AsSpan(AppleDosFileSystemLayout.TrackSectorListOffsetOffset), checked((ushort)(listIndex * AppleDosFileSystemLayout.TrackSectorPairCount)));
            var first = listIndex * AppleDosFileSystemLayout.TrackSectorPairCount;
            var count = Math.Min(AppleDosFileSystemLayout.TrackSectorPairCount, dataSectors.Count - first);
            for (var pair = 0; pair < count; pair++) WriteAddress(list, AppleDosFileSystemLayout.TrackSectorPairsOffset + pair * AppleDosFileSystemLayout.TrackSectorPairSize, dataSectors[first + pair], sectorsPerTrack);
        }
    }

    private static void WriteCatalogEntry(MigrationEntry entry, byte rawType, int index, int firstList, int sectorCount, byte[][] sectors, int sectorsPerTrack)
    {
        var catalogSector = sectorsPerTrack - 1 - index / AppleDosFileSystemLayout.CatalogEntriesPerSector;
        var catalog = sectors[AppleDosFileSystemLayout.VtocTrack * sectorsPerTrack + catalogSector];
        var offset = AppleDosFileSystemLayout.CatalogFirstEntryOffset + index % AppleDosFileSystemLayout.CatalogEntriesPerSector * AppleDosFileSystemLayout.CatalogEntrySize;
        WriteAddress(catalog, offset, firstList, sectorsPerTrack);
        catalog[offset + AppleDosFileSystemLayout.EntryTypeOffset] = rawType;
        EncodeName(entry.TargetName).CopyTo(catalog, offset + AppleDosFileSystemLayout.EntryNameOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(catalog.AsSpan(offset + AppleDosFileSystemLayout.EntrySectorCountOffset), checked((ushort)sectorCount));
    }

    private static byte AppleDosFileTypeValue(MigrationEntry entry, string sourceFileSystemId) => sourceFileSystemId.Equals(FileSystemIds.AppleDos, StringComparison.OrdinalIgnoreCase) ? (byte)entry.RawAttributes : (byte)AppleDosFileType.Binary;

    private static byte[] EncodeContent(MigrationEntry entry, byte rawType)
    {
        if ((AppleDosFileType)(rawType & AppleDosFileSystemLayout.ValueMask) != AppleDosFileType.Binary) return entry.Content!.ToArray();
        var loadAddress = checked((ushort)(entry.RawAttributes >> AppleDosFileSystemLayout.BinaryLoadAddressAttributeShift));
        return AppleDosBinaryFileCodec.Encode(entry.Content!, loadAddress);
    }

    private static byte[] EncodeName(string name)
    {
        var output = Enumerable.Repeat((byte)0xa0, AppleDosFileSystemLayout.EntryNameLength).ToArray();
        var encoded = System.Text.Encoding.ASCII.GetBytes(name);
        for (var index = 0; index < encoded.Length; index++) output[index] = (byte)(encoded[index] | 0x80);
        return output;
    }

    private static void WriteAddress(byte[] data, int offset, int logical, int sectorsPerTrack)
    {
        data[offset] = checked((byte)(logical / sectorsPerTrack));
        data[offset + 1] = checked((byte)(logical % sectorsPerTrack));
    }

    private static void WriteVtoc(byte[][] sectors, bool[] free, int sectorsPerTrack, byte volumeNumber)
    {
        var vtoc = sectors[AppleDosFileSystemLayout.VtocTrack * sectorsPerTrack];
        vtoc[0] = sectorsPerTrack == AppleDosFileSystemLayout.Dos32SectorsPerTrack ? AppleDosFileSystemLayout.Dos32VtocVersion : AppleDosFileSystemLayout.Dos33VtocVersion;
        vtoc[AppleDosFileSystemLayout.VtocCatalogTrackOffset] = AppleDosFileSystemLayout.VtocTrack;
        vtoc[AppleDosFileSystemLayout.VtocCatalogSectorOffset] = checked((byte)(sectorsPerTrack - 1));
        vtoc[AppleDosFileSystemLayout.VtocVolumeNumberOffset] = volumeNumber;
        vtoc[AppleDosFileSystemLayout.VtocMaximumPairsOffset] = AppleDosFileSystemLayout.TrackSectorPairCount;
        vtoc[AppleDosFileSystemLayout.VtocLastAllocatedTrackOffset] = AppleDosFileSystemLayout.VtocTrack - 1;
        vtoc[AppleDosFileSystemLayout.VtocAllocationDirectionOffset] = AppleDosFileSystemLayout.DescendingAllocationDirection;
        vtoc[AppleDosFileSystemLayout.VtocTrackCountOffset] = AppleDosFileSystemLayout.TrackCount;
        vtoc[AppleDosFileSystemLayout.VtocSectorsPerTrackOffset] = checked((byte)sectorsPerTrack);
        BinaryPrimitives.WriteUInt16LittleEndian(vtoc.AsSpan(AppleDosFileSystemLayout.VtocSectorSizeOffset), AppleDosFileSystemLayout.SectorSize);
        for (var track = 0; track < AppleDosFileSystemLayout.TrackCount; track++)
        {
            uint bits = 0;
            for (var sector = 0; sector < sectorsPerTrack; sector++) if (free[track * sectorsPerTrack + sector]) bits |= 1u << (sizeof(uint) * 8 - sectorsPerTrack + sector);
            BinaryPrimitives.WriteUInt32BigEndian(vtoc.AsSpan(AppleDosFileSystemLayout.VtocFreeBitmapOffset + track * AppleDosFileSystemLayout.VtocTrackBitmapSize), bits);
        }
    }
}
