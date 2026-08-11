using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Recognition.TeleDisk;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Reading;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Reads ordinary (uppercase TD signature) TeleDisk images.</summary>
public sealed class Td0Reader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Td0, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Read(data);
    }

    internal static SectorImage Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < 12 || data[0] != (byte)'T' || data[1] != (byte)'D')
            throw new InvalidDataException("The image is not an uncompressed TeleDisk image.");

        var offset = 12;
        var stepping = data[7];
        if ((stepping & 0x80) != 0)
        {
            EnsureAvailable(data, offset, 10, "TeleDisk comment header");
            var commentLength = ReadUInt16(data, offset + 2);
            offset += 10;
            EnsureAvailable(data, offset, commentLength, "TeleDisk comment");
            offset += commentLength;
        }

        var sectors = new List<Td0Sector>();
        while (true)
        {
            EnsureAvailable(data, offset, 1, "TeleDisk track header");
            var sectorCount = data[offset];
            if (sectorCount == 0xff) break;
            EnsureAvailable(data, offset, 4, "TeleDisk track header");
            var trackCylinder = data[offset + 1];
            var trackHead = data[offset + 2] & 1;
            offset += 4;

            for (var index = 0; index < sectorCount; index++)
            {
                EnsureAvailable(data, offset, 6, "TeleDisk sector header");
                var cylinder = data[offset];
                var head = data[offset + 1] & 1;
                var number = data[offset + 2];
                var sizeCode = data[offset + 3];
                var flags = data[offset + 4];
                offset += 6;

                if (sizeCode > 6) throw new InvalidDataException($"TeleDisk sector {cylinder}/{head}/{number} has an invalid size code.");
                var expectedLength = 128 << sizeCode;
                byte[] sectorData;
                if ((flags & 0x30) != 0)
                {
                    sectorData = new byte[expectedLength];
                }
                else
                {
                    EnsureAvailable(data, offset, 3, "TeleDisk sector data header");
                    var encodedLength = ReadUInt16(data, offset);
                    var encoding = data[offset + 2];
                    offset += 3;
                    if (encodedLength == 0) throw new InvalidDataException($"TeleDisk sector {cylinder}/{head}/{number} has no encoded data.");
                    EnsureAvailable(data, offset, encodedLength - 1, "TeleDisk sector data");
                    sectorData = Td0SectorDecoder.Decode(data.Slice(offset, encodedLength - 1), encoding, expectedLength);
                    offset += encodedLength - 1;
                }

                sectors.Add(new(cylinder, head, number, sectorData, (flags & 0x02) == 0));
            }

            if (sectorCount != 0 && sectors[^1].Cylinder != trackCylinder)
                throw new InvalidDataException("A TeleDisk track contains an inconsistent cylinder number.");
            if (sectorCount != 0 && sectors[^1].Head != trackHead)
                throw new InvalidDataException("A TeleDisk track contains an inconsistent head number.");
        }

        if (sectors.Count == 0) throw new InvalidDataException("The TeleDisk image contains no sectors.");
        var blockSize = sectors.GroupBy(sector => sector.Data.Length).OrderByDescending(group => group.Count()).First().Key;
        // Copy-protected TeleDisk images may add a few deliberately unusual sectors.
        // Reconstruct the normal logical image from the dominant sector size rather
        // than rejecting the complete disk.
        var logicalSectors = sectors.Where(sector => sector.Data.Length == blockSize).ToArray();

        var cylinders = logicalSectors.Max(sector => sector.Cylinder) + 1;
        var heads = logicalSectors.Max(sector => sector.Head) + 1;
        var sectorsPerTrack = logicalSectors.GroupBy(sector => (sector.Cylinder, sector.Head)).Max(group => group.Count());
        var blocks = logicalSectors
            .OrderBy(sector => sector.Cylinder).ThenBy(sector => sector.Head).ThenBy(sector => sector.Number)
            .Select((sector, logical) => new SectorBlock(logical,
                new SectorAddress(sector.Cylinder, sector.Head, sector.Number), sector.Data, sector.IntegrityValid))
            .ToArray();
        var formatId = Td0SectorImageClassifier.Detect(blocks, blockSize, cylinders, heads, sectorsPerTrack);
        return new SectorImage(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks, capacity: blocks.LongLength * blockSize, logicalBlockCount: blocks.Length);
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count, string description)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count) throw new InvalidDataException($"The {description} is truncated.");
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => (ushort)(data[offset] | data[offset + 1] << 8);
    private sealed record Td0Sector(int Cylinder, int Head, int Number, byte[] Data, bool IntegrityValid);
}
