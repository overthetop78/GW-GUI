using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Reads Dave Dunfield ImageDisk (IMD) sector images.</summary>
public sealed class ImdReader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Imd, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    internal static SectorImage Read(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default)
    {
        var commentEnd = data.IndexOf(ImdFormat.CommentTerminator);
        if (commentEnd < ImdFormat.SignatureLength || !data[..ImdFormat.SignatureLength].SequenceEqual(ImdFormat.Signature))
            throw new InvalidDataException("The image does not contain an ImageDisk header.");

        var offset = commentEnd + 1;
        var sectors = new List<ImdSector>();
        while (offset < data.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureAvailable(data, offset, ImdLayout.TrackHeaderSize, "ImageDisk track header");
            var header = data.Slice(offset, ImdLayout.TrackHeaderSize);
            var mode = header[ImdLayout.ModeOffset];
            var trackCylinder = header[ImdLayout.CylinderOffset];
            var headFlags = header[ImdLayout.HeadFlagsOffset];
            var trackHead = headFlags & 1;
            var count = header[ImdLayout.SectorCountOffset];
            var sizeCode = header[ImdLayout.SectorSizeCodeOffset];
            offset += ImdLayout.TrackHeaderSize;
            if (mode > 5 || count == 0) throw new InvalidDataException("The ImageDisk track header is invalid.");

            var mapLength = count * ImdLayout.MapEntrySize;
            EnsureAvailable(data, offset, mapLength, "ImageDisk sector-number map");
            var numbers = data.Slice(offset, mapLength).ToArray();
            offset += mapLength;
            byte[]? cylinders = null;
            byte[]? heads = null;
            if ((headFlags & 0x80) != 0)
            {
                EnsureAvailable(data, offset, mapLength, "ImageDisk cylinder map");
                cylinders = data.Slice(offset, mapLength).ToArray();
                offset += mapLength;
            }
            if ((headFlags & 0x40) != 0)
            {
                EnsureAvailable(data, offset, mapLength, "ImageDisk head map");
                heads = data.Slice(offset, mapLength).ToArray();
                offset += mapLength;
            }

            int[] sizes;
            if (sizeCode == ImdLayout.ExplicitSectorSizeCode)
            {
                var sizeMapLength = count * ImdLayout.SectorSizeMapEntrySize;
                EnsureAvailable(data, offset, sizeMapLength, "ImageDisk sector-size map");
                sizes = new int[count];
                for (var index = 0; index < count; index++)
                    sizes[index] = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset + index * ImdLayout.SectorSizeMapEntrySize, ImdLayout.SectorSizeMapEntrySize));
                offset += sizeMapLength;
            }
            else
            {
                if (sizeCode > ImdLayout.MaximumExponentialSizeCode) throw new InvalidDataException("The ImageDisk sector-size code is invalid.");
                sizes = Enumerable.Repeat(ImdLayout.BaseSectorSize << sizeCode, count).ToArray();
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
        if (sectors256 >= 64 && blockSize == 512) return DiskImageFormatIds.EpsonQx10_396;
        // Both the 399 KiB TPM layout and the LOGO layout contain sixteen
        // 256-byte sectors. Capacity distinguishes them reliably.
        if (sectors256 == 16 && blockSize == 512 && capacity == 399 * DataSizeConstants.BytesPerKibibyte) return DiskImageFormatIds.EpsonQx10_399;
        if (sectors256 == 16 && blockSize == 512) return DiskImageFormatIds.EpsonQx10Logo;
        return (blockSize, capacity) switch
        {
            (256, 320 * DataSizeConstants.BytesPerKibibyte) => DiskImageFormatIds.EpsonQx10_320,
            (512, 396 * DataSizeConstants.BytesPerKibibyte) => DiskImageFormatIds.EpsonQx10_396,
            (512, 399 * DataSizeConstants.BytesPerKibibyte) => DiskImageFormatIds.EpsonQx10_399,
            (512, 400 * DataSizeConstants.BytesPerKibibyte) => DiskImageFormatIds.EpsonQx10Logo,
            (1024, 400 * DataSizeConstants.BytesPerKibibyte) => DiskImageFormatIds.EpsonQx10_400,
            _ => DiskImageFormatIds.Imd
        };
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count, string description)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count)
            throw new InvalidDataException($"The {description} is truncated.");
    }

    private sealed record ImdSector(int Cylinder, int Head, int Number, byte[] Data, bool Available, bool IntegrityValid);
}

