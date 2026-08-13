using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Lit les images sectorielles Dave Dunfield ImageDisk.</summary>
public sealed class ImdReader
{
    /// <summary>Lit un fichier ImageDisk et construit son image sectorielle.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) => (await ReadDetailedAsync(path, cancellationToken).ConfigureAwait(false)).SectorImage;

    /// <summary>Lit un fichier ImageDisk en conservant ses pistes, modes, cartes et états.</summary>
    public async Task<ImdImage> ReadDetailedAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return ReadDetailed(data, cancellationToken);
    }

    /// <summary>Analyse une séquence ImageDisk et construit son image sectorielle.</summary>
    internal static SectorImage Read(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default) => ReadDetailed(data, cancellationToken).SectorImage;

    /// <summary>Analyse une séquence ImageDisk en conservant les informations réinscriptibles.</summary>
    public static ImdImage ReadDetailed(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default)
    {
        var offset = FindTrackDataOffset(data, out var comment);
        var tracks = new List<ImdTrack>();
        while (offset < data.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var header = ReadTrackHeader(data, ref offset);
            var numbers = ReadByteMap(data, ref offset, header.SectorCount, ImdSection.SectorNumberMap);
            var cylinders = header.HeadFlags.HasFlag(ImdHeadFlags.HasCylinderMap) ? ReadByteMap(data, ref offset, header.SectorCount, ImdSection.CylinderMap) : null;
            var heads = header.HeadFlags.HasFlag(ImdHeadFlags.HasHeadMap) ? ReadByteMap(data, ref offset, header.SectorCount, ImdSection.HeadMap) : null;
            var sizes = ReadSectorSizes(data, ref offset, header.SectorCount, header.SectorSizeCode);
            var sectors = new List<ImdSector>(header.SectorCount);
            for (var index = 0; index < header.SectorCount; index++)
            {
                EnsureAvailable(data, offset, ImdLayout.MapEntrySize, ImdSection.SectorRecord);
                var recordType = (ImdSectorRecordType)data[offset++];
                if (!Enum.IsDefined(recordType)) throw ImdExceptions.InvalidRecordType((byte)recordType);
                var bytes = ReadSectorRecord(data, ref offset, recordType, sizes[index]);
                sectors.Add(new(cylinders?[index] ?? header.Cylinder, checked((byte)((heads?[index] ?? header.Head) & (int)ImdHeadFlags.HeadMask)), numbers[index], sizes[index], recordType, bytes));
            }
            tracks.Add(new(header.Mode, header.Cylinder, checked((byte)header.Head), sectors));
        }
        return new(comment, tracks, BuildImage(tracks.SelectMany(track => track.Sectors).ToArray()));
    }

    private static int FindTrackDataOffset(ReadOnlySpan<byte> data, out string comment)
    {
        var commentEnd = data.IndexOf(ImdFormat.CommentTerminator);
        if (commentEnd < ImdFormat.SignatureLength || !data[..ImdFormat.SignatureLength].SequenceEqual(ImdFormat.Signature)) throw ImdExceptions.MissingSignature(commentEnd, data.Length);
        comment = System.Text.Encoding.ASCII.GetString(data[..commentEnd]);
        return commentEnd + ImdLayout.MapEntrySize;
    }

    private static ImdTrackHeader ReadTrackHeader(ReadOnlySpan<byte> data, ref int offset)
    {
        EnsureAvailable(data, offset, ImdLayout.TrackHeaderSize, ImdSection.TrackHeader);
        var header = data.Slice(offset, ImdLayout.TrackHeaderSize);
        offset += ImdLayout.TrackHeaderSize;
        var mode = (ImdMode)header[ImdLayout.ModeOffset];
        var sectorCount = header[ImdLayout.SectorCountOffset];
        if (!Enum.IsDefined(mode) || sectorCount == 0) throw ImdExceptions.InvalidTrackHeader(mode, sectorCount);
        var flags = (ImdHeadFlags)header[ImdLayout.HeadFlagsOffset];
        return new(mode, header[ImdLayout.CylinderOffset], flags, (int)(flags & ImdHeadFlags.HeadMask), sectorCount, header[ImdLayout.SectorSizeCodeOffset]);
    }

    private static byte[] ReadByteMap(ReadOnlySpan<byte> data, ref int offset, int count, ImdSection section)
    {
        EnsureAvailable(data, offset, count, section);
        var map = data.Slice(offset, count).ToArray();
        offset += count;
        return map;
    }

    private static int[] ReadSectorSizes(ReadOnlySpan<byte> data, ref int offset, int count, byte sizeCode)
    {
        if (sizeCode != ImdLayout.ExplicitSectorSizeCode)
        {
            if (sizeCode > ImdLayout.MaximumExponentialSizeCode) throw ImdExceptions.InvalidSizeCode(sizeCode);
            return Enumerable.Repeat(ImdLayout.BaseSectorSize << sizeCode, count).ToArray();
        }
        var length = count * ImdLayout.SectorSizeMapEntrySize;
        EnsureAvailable(data, offset, length, ImdSection.SectorSizeMap);
        var sizes = new int[count];
        for (var index = 0; index < count; index++) sizes[index] = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset + index * ImdLayout.SectorSizeMapEntrySize, ImdLayout.SectorSizeMapEntrySize));
        offset += length;
        return sizes;
    }

    private static byte[] ReadSectorRecord(ReadOnlySpan<byte> data, ref int offset, ImdSectorRecordType type, int size)
    {
        if (!type.HasData()) return new byte[size];
        if (type.IsCompressed())
        {
            EnsureAvailable(data, offset, 1, ImdSection.CompressedValue);
            return Enumerable.Repeat(data[offset++], size).ToArray();
        }
        EnsureAvailable(data, offset, size, ImdSection.SectorData);
        var bytes = data.Slice(offset, size).ToArray();
        offset += size;
        return bytes;
    }

    private static SectorImage BuildImage(IReadOnlyList<ImdSector> sectors)
    {
        if (sectors.Count == 0) throw ImdExceptions.NoSectors();
        var blockSize = sectors.GroupBy(sector => sector.Size).OrderByDescending(group => group.Count()).First().Key;
        var cylinders = sectors.Max(sector => sector.Cylinder) + 1;
        var heads = sectors.Max(sector => sector.Head) + 1;
        var sectorsPerTrack = sectors.GroupBy(sector => (sector.Cylinder, sector.Head)).Max(group => group.Count());
        var ordered = sectors.OrderBy(sector => sector.Cylinder).ThenBy(sector => sector.Head).ThenBy(sector => sector.Number).ToArray();
        var blocks = ordered.Select((sector, logical) => (sector, logical)).Where(item => item.sector.RecordType.HasData()).Select(item => new SectorBlock(item.logical, new(item.sector.Cylinder, item.sector.Head, item.sector.Number), item.sector.Data, item.sector.RecordType.IsIntegrityValid(), DiagnosticCode: (byte)item.sector.RecordType)).ToArray();
        var descriptors = sectors.Select(sector => new EpsonQx10SectorDescriptor(sector.Cylinder, sector.Head, sector.Number, sector.Size)).ToArray();
        var formatId = EpsonQx10FormatDetector.TryDetect(descriptors, out var detected) ? detected : DiskImageFormatIds.Imd;
        return new(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks, sectors.Any(sector => sector.Size != blockSize), ordered.Sum(sector => (long)sector.Size), ordered.Length);
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count, ImdSection section)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count) throw ImdExceptions.TruncatedSection(section, offset, count, Math.Max(0, data.Length - offset));
    }

    private readonly record struct ImdTrackHeader(ImdMode Mode, byte Cylinder, ImdHeadFlags HeadFlags, int Head, int SectorCount, byte SectorSizeCode);
}
