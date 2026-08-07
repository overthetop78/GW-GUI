using System.Buffers.Binary;
using System.Text;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

/// <summary>Reads standard and extended CPCEMU DSK containers without using Greaseweazle.</summary>
public sealed class AmstradDskImageReader
{
    private const int HeaderSize = 0x100;

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (bytes.Length < HeaderSize) throw new InvalidDataException("The Amstrad DSK header is truncated.");
        var signature = System.Text.Encoding.ASCII.GetString(bytes, 0, 34);
        var extended = signature.StartsWith("EXTENDED CPC DSK File", StringComparison.Ordinal);
        if (!extended && !signature.StartsWith("MV - CPC", StringComparison.Ordinal))
            throw new InvalidDataException("The file is not a CPCEMU DSK image.");

        var cylinders = bytes[48];
        var heads = bytes[49];
        if (cylinders is 0 or > 168 || heads is 0 or > 2)
            throw new InvalidDataException("The Amstrad DSK geometry is invalid.");
        var trackCount = checked(cylinders * heads);
        if (extended && 52 + trackCount > HeaderSize)
            throw new InvalidDataException("The extended Amstrad DSK track table is invalid.");

        var blocks = new List<SectorBlock>();
        var position = HeaderSize;
        var logicalBlock = 0;
        var dominantSize = 0;
        var maximumSectors = 0;
        var sectorSizes = new Dictionary<int, int>();
        for (var trackIndex = 0; trackIndex < trackCount; trackIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var trackSize = extended ? bytes[52 + trackIndex] * 256 : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(50, 2));
            if (trackSize == 0) continue;
            if (position + trackSize > bytes.Length || trackSize < HeaderSize)
                throw new InvalidDataException($"Amstrad DSK track {trackIndex} is truncated.");
            if (!System.Text.Encoding.ASCII.GetString(bytes, position, 12).StartsWith("Track-Info", StringComparison.Ordinal))
                throw new InvalidDataException($"Amstrad DSK track {trackIndex} has an invalid header.");

            var cylinder = bytes[position + 16];
            var head = bytes[position + 17];
            var sectorCount = bytes[position + 21];
            maximumSectors = Math.Max(maximumSectors, sectorCount);
            if (position + 24 + sectorCount * 8 > position + HeaderSize)
                throw new InvalidDataException($"Amstrad DSK track {trackIndex} has an invalid sector table.");
            var dataPosition = position + HeaderSize;
            var trackSectors = new List<(int Cylinder, int Head, int Id, byte[] Data, bool Valid)>();
            for (var sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
            {
                var descriptor = position + 24 + sectorIndex * 8;
                var sectorCylinder = bytes[descriptor];
                var sectorHead = bytes[descriptor + 1];
                var sectorId = bytes[descriptor + 2];
                var sizeCode = bytes[descriptor + 3] & 7;
                var nominalSize = 128 << sizeCode;
                var storedSize = extended ? BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(descriptor + 6, 2)) : nominalSize;
                if (storedSize == 0) storedSize = nominalSize;
                if (dataPosition + storedSize > position + trackSize)
                    throw new InvalidDataException($"Amstrad DSK sector {cylinder}:{head}:{sectorId} is truncated.");
                var data = bytes.AsSpan(dataPosition, Math.Min(nominalSize, storedSize)).ToArray();
                var status1 = bytes[descriptor + 4];
                var status2 = bytes[descriptor + 5];
                var crcValid = (status1 & 0x20) == 0;
                trackSectors.Add((sectorCylinder, sectorHead, sectorId, data, crcValid));
                sectorSizes[nominalSize] = sectorSizes.GetValueOrDefault(nominalSize) + 1;
                dataPosition += storedSize;
            }
            foreach (var sector in trackSectors.OrderBy(sector => sector.Id))
                blocks.Add(new(logicalBlock++, new(sector.Cylinder, sector.Head, sector.Id), sector.Data, sector.Valid));
            position += trackSize;
        }
        if (blocks.Count == 0) throw new InvalidDataException("The Amstrad DSK image contains no sectors.");
        dominantSize = sectorSizes.OrderByDescending(item => item.Value).First().Key;
        var formatId = cylinders >= 80 && heads == 2 ? "amstrad.pcw" : "amstrad.cpc";
        return new(formatId, dominantSize, cylinders, heads, Math.Max(1, maximumSectors), blocks,
            allowVariableBlockSize: sectorSizes.Count > 1, capacity: blocks.Sum(block => (long)block.Data.Count), logicalBlockCount: blocks.Count);
    }
}
