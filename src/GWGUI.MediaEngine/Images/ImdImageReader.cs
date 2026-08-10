using System.Buffers.Binary;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

/// <summary>Reads Dave Dunfield ImageDisk (IMD) sector images.</summary>
public sealed class ImdImageReader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Imd, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    internal static SectorImage Read(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default)
    {
        var commentEnd = data.IndexOf((byte)0x1a);
        if (commentEnd < 3 || !data[..3].SequenceEqual("IMD"u8))
            throw new InvalidDataException("The image does not contain an ImageDisk header.");

        var offset = commentEnd + 1;
        var sectors = new List<ImdSector>();
        while (offset < data.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureAvailable(data, offset, 5, "ImageDisk track header");
            var mode = data[offset++];
            var trackCylinder = data[offset++];
            var headFlags = data[offset++];
            var trackHead = headFlags & 1;
            var count = data[offset++];
            var sizeCode = data[offset++];
            if (mode > 5 || count == 0) throw new InvalidDataException("The ImageDisk track header is invalid.");

            EnsureAvailable(data, offset, count, "ImageDisk sector-number map");
            var numbers = data.Slice(offset, count).ToArray();
            offset += count;
            byte[]? cylinders = null;
            byte[]? heads = null;
            if ((headFlags & 0x80) != 0)
            {
                EnsureAvailable(data, offset, count, "ImageDisk cylinder map");
                cylinders = data.Slice(offset, count).ToArray();
                offset += count;
            }
            if ((headFlags & 0x40) != 0)
            {
                EnsureAvailable(data, offset, count, "ImageDisk head map");
                heads = data.Slice(offset, count).ToArray();
                offset += count;
            }

            int[] sizes;
            if (sizeCode == 0xff)
            {
                EnsureAvailable(data, offset, count * 2, "ImageDisk sector-size map");
                sizes = new int[count];
                for (var index = 0; index < count; index++)
                    sizes[index] = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset + index * 2, 2));
                offset += count * 2;
            }
            else
            {
                if (sizeCode > 6) throw new InvalidDataException("The ImageDisk sector-size code is invalid.");
                sizes = Enumerable.Repeat(128 << sizeCode, count).ToArray();
            }

            for (var index = 0; index < count; index++)
            {
                EnsureAvailable(data, offset, 1, "ImageDisk sector record");
                var recordType = data[offset++];
                var size = sizes[index];
                byte[] bytes;
                if (recordType == 0) bytes = new byte[size];
                else if ((recordType & 1) == 0)
                {
                    EnsureAvailable(data, offset, 1, "ImageDisk compressed sector");
                    bytes = Enumerable.Repeat(data[offset++], size).ToArray();
                }
                else
                {
                    EnsureAvailable(data, offset, size, "ImageDisk sector data");
                    bytes = data.Slice(offset, size).ToArray();
                    offset += size;
                }
                if (recordType > 8) throw new InvalidDataException("The ImageDisk sector-record type is invalid.");
                sectors.Add(new(cylinders?[index] ?? trackCylinder, (heads?[index] ?? trackHead) & 1,
                    numbers[index], bytes, recordType != 0,
                    recordType != 0 && recordType is not 5 and not 6 and not 7 and not 8));
            }
        }

        if (sectors.Count == 0) throw new InvalidDataException("The ImageDisk image contains no sectors.");
        var blockSize = sectors.GroupBy(sector => sector.Data.Length).OrderByDescending(group => group.Count()).First().Key;
        var cylindersCount = sectors.Max(sector => sector.Cylinder) + 1;
        var headsCount = sectors.Max(sector => sector.Head) + 1;
        var sectorsPerTrack = sectors.GroupBy(sector => (sector.Cylinder, sector.Head)).Max(group => group.Count());
        var ordered = sectors.OrderBy(sector => sector.Cylinder).ThenBy(sector => sector.Head).ThenBy(sector => sector.Number).ToArray();
        var blocks = ordered.Select((sector, logical) => (sector, logical))
            .Where(item => item.sector.Available)
            .Select(item => new SectorBlock(item.logical,
                new(item.sector.Cylinder, item.sector.Head, item.sector.Number),
                item.sector.Data, item.sector.IntegrityValid)).ToArray();
        // Type 0 records still describe a sector in the image geometry, but no
        // sector data was available. Keep the declared capacity and logical
        // position while exposing the sector through MissingBlocks.
        var capacity = ordered.Sum(sector => (long)sector.Data.Length);
        var formatId = DetectFormat(sectors, blockSize, capacity);
        return new(formatId, blockSize, cylindersCount, headsCount, sectorsPerTrack, blocks,
            sectors.Any(sector => sector.Data.Length != blockSize), capacity, ordered.Length);
    }

    private static string DetectFormat(IReadOnlyList<ImdSector> sectors, int blockSize, long capacity)
    {
        var sectors256 = sectors.Count(sector => sector.Data.Length == 256);
        if (sectors256 >= 64 && blockSize == 512) return "epson.qx10.396";
        // Both the 399 KiB TPM layout and the LOGO layout contain sixteen
        // 256-byte sectors. Capacity distinguishes them reliably.
        if (sectors256 == 16 && blockSize == 512 && capacity == 399 * 1024L) return "epson.qx10.399";
        if (sectors256 == 16 && blockSize == 512) return "epson.qx10.logo";
        return (blockSize, capacity) switch
        {
            (256, 320 * 1024L) => "epson.qx10.320",
            (512, 396 * 1024L) => "epson.qx10.396",
            (512, 399 * 1024L) => "epson.qx10.399",
            (512, 400 * 1024L) => "epson.qx10.logo",
            (1024, 400 * 1024L) => "epson.qx10.400",
            _ => "imd"
        };
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count, string description)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count)
            throw new InvalidDataException($"The {description} is truncated.");
    }

    private sealed record ImdSector(int Cylinder, int Head, int Number, byte[] Data, bool Available, bool IntegrityValid);
}
