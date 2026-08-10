using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

/// <summary>Reads ordinary (uppercase TD signature) TeleDisk images.</summary>
public sealed class Td0ImageReader : ISectorImageReader
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
                    sectorData = DecodeSector(data.Slice(offset, encodedLength - 1), encoding, expectedLength);
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
        var formatId = DetectFormat(blocks, blockSize, cylinders, heads, sectorsPerTrack);
        return new SectorImage(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks,
            capacity: blocks.LongLength * blockSize, logicalBlockCount: blocks.Length);
    }

    private static string DetectFormat(IReadOnlyList<SectorBlock> blocks, int blockSize, int cylinders, int heads, int sectorsPerTrack)
    {
        var boot = blocks.FirstOrDefault(block => block.Address.Cylinder == 0 && block.Address.Head == 0 && block.Address.Number == 1)?.Data;
        var hasFatBpb = boot is { Count: >= 36 }
            && (boot[11] | boot[12] << 8) == blockSize
            && boot[13] > 0
            && (boot[24] | boot[25] << 8) is > 0 and <= 64
            && (boot[26] | boot[27] << 8) is > 0 and <= 8;
        var hasDosBootJump = boot is { Count: >= 3 } && boot[0] is 0xeb or 0xe9;
        if ((hasFatBpb || hasDosBootJump) && blockSize == 512)
        {
            return (cylinders, heads, sectorsPerTrack) switch
            {
                (40, 1, 8) => DiskImageFormatIds.Ibm160,
                (40, 1, 9) => DiskImageFormatIds.Ibm180,
                (40, 2, 8) => DiskImageFormatIds.Ibm320,
                (40, 2, 9) => DiskImageFormatIds.Ibm360,
                (80, 2, 9) => DiskImageFormatIds.Ibm720,
                (80, 2, 15) => DiskImageFormatIds.Ibm1200,
                (80, 2, 18) => DiskImageFormatIds.Ibm1440,
                _ => DiskImageFormatIds.IbmScan
            };
        }
        return DiskImageFormatIds.UcsdIbmMfm;
    }

    private static byte[] DecodeSector(ReadOnlySpan<byte> encoded, byte encoding, int expectedLength)
    {
        var output = new List<byte>(expectedLength);
        switch (encoding)
        {
            case 0:
                output.AddRange(encoded.ToArray());
                break;
            case 1:
                if (encoded.Length != 4) throw new InvalidDataException("A TeleDisk repeated sector has an invalid payload.");
                var repetitions = ReadUInt16(encoded, 0);
                for (var index = 0; index < repetitions; index++) { output.Add(encoded[2]); output.Add(encoded[3]); }
                break;
            case 2:
                for (var offset = 0; offset < encoded.Length;)
                {
                    if (offset + 2 > encoded.Length) throw new InvalidDataException("A TeleDisk RLE sector is truncated.");
                    var patternWords = encoded[offset++];
                    var count = encoded[offset++];
                    if (patternWords == 0)
                    {
                        if (offset + count > encoded.Length) throw new InvalidDataException("A TeleDisk literal run is truncated.");
                        output.AddRange(encoded.Slice(offset, count).ToArray());
                        offset += count;
                    }
                    else
                    {
                        var patternLength = patternWords * 2;
                        if (offset + patternLength > encoded.Length) throw new InvalidDataException("A TeleDisk repeated run is truncated.");
                        var pattern = encoded.Slice(offset, patternLength).ToArray();
                        offset += patternLength;
                        for (var repeat = 0; repeat < count; repeat++) output.AddRange(pattern);
                    }
                }
                break;
            default:
                throw new InvalidDataException($"TeleDisk sector encoding {encoding} is not supported.");
        }

        if (output.Count != expectedLength)
            throw new InvalidDataException($"A TeleDisk sector expands to {output.Count} bytes instead of {expectedLength}.");
        return output.ToArray();
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count, string description)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count) throw new InvalidDataException($"The {description} is truncated.");
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => (ushort)(data[offset] | data[offset + 1] << 8);
    private sealed record Td0Sector(int Cylinder, int Head, int Number, byte[] Data, bool IntegrityValid);
}
