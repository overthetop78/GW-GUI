using System.Buffers.Binary;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class IbmPcImageReader : ISectorImageReader
{
    private static readonly IReadOnlyDictionary<int, IbmPcGeometry> Geometries = new Dictionary<int, IbmPcGeometry>
    {
        [160 * 1024] = new(DiskImageFormatIds.Ibm160, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, 8),
        [180 * 1024] = new(DiskImageFormatIds.Ibm180, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, 9),
        [320 * 1024] = new(DiskImageFormatIds.Ibm320, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 8),
        [360 * 1024] = new(DiskImageFormatIds.Ibm360, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 9),
        [720 * 1024] = new(DiskImageFormatIds.Ibm720, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 9),
        [800 * 1024] = new(DiskImageFormatIds.Ibm800, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 10),
        [1200 * 1024] = new(DiskImageFormatIds.Ibm1200, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 15),
        [1440 * 1024] = new(DiskImageFormatIds.Ibm1440, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 18),
        [1680 * 1024] = new(DiskImageFormatIds.Ibm1680, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 21),
        [2880 * 1024] = new(DiskImageFormatIds.Ibm2880, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 36)
    };

    public bool CanRead(string path) => Path.GetExtension(path) is var extension
        && (extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase)
            || extension.Equals(DiskImageFileExtensions.Ima, StringComparison.OrdinalIgnoreCase));

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Create(data, cancellationToken);
    }

    internal static SectorImage Create(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default)
    {
        var geometry = DetectGeometry(data);
        var blocks = new SectorBlock[geometry.Cylinders * geometry.Heads * geometry.SectorsPerTrack];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            var track = logical / geometry.SectorsPerTrack;
            blocks[logical] = new(logical,
                new(track / geometry.Heads, track % geometry.Heads, logical % geometry.SectorsPerTrack + 1),
                data.Slice(logical * 512, 512).ToArray());
        }
        return new(geometry.FormatId, 512, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks);
    }

    internal static IbmPcGeometry DetectGeometry(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0 || data.Length % 512 != 0)
            throw new InvalidDataException("The IBM PC image size is not a multiple of 512 bytes.");

        if (TryReadBpbGeometry(data, out var bpbGeometry)) return bpbGeometry;
        if (Geometries.TryGetValue(data.Length, out var geometry)) return geometry;
        throw new InvalidDataException("The IBM PC image geometry could not be determined from its boot sector or size.");
    }

    internal static bool HasValidBpbGeometry(ReadOnlySpan<byte> data) => TryReadBpbGeometry(data, out _);

    internal static string FormatIdForGeometry(int cylinders, int heads, int sectorsPerTrack, int sectorSize = 512)
    {
        var size = checked(cylinders * heads * sectorsPerTrack * sectorSize);
        return Geometries.TryGetValue(size, out var geometry) ? geometry.FormatId : DiskImageFormatIds.IbmFromCapacity(size);
    }

    internal static bool TryDetectFluxGeometry(ReadOnlySpan<byte> boot, byte fatMedia, out IbmPcGeometry geometry)
    {
        geometry = default;
        if (boot.Length >= 36)
        {
            var bytesPerSector = BinaryPrimitives.ReadUInt16LittleEndian(boot[11..]);
            var totalSectors = (int)BinaryPrimitives.ReadUInt16LittleEndian(boot[19..]);
            if (totalSectors == 0)
            {
                var largeTotal = BinaryPrimitives.ReadUInt32LittleEndian(boot[32..]);
                if (largeTotal <= int.MaxValue) totalSectors = (int)largeTotal;
            }
            var sectorsPerTrack = BinaryPrimitives.ReadUInt16LittleEndian(boot[24..]);
            var heads = BinaryPrimitives.ReadUInt16LittleEndian(boot[26..]);
            if (bytesPerSector == 512 && totalSectors > 0 && sectorsPerTrack is > 0 and <= 63 && heads is > 0 and <= 2
                && totalSectors % (sectorsPerTrack * heads) == 0)
            {
                var cylinders = totalSectors / (sectorsPerTrack * heads);
                geometry = new(FormatIdForGeometry(cylinders, heads, sectorsPerTrack), cylinders, heads, sectorsPerTrack);
                return true;
            }
        }
        geometry = fatMedia switch
        {
            0xfe => Geometries[160 * 1024],
            0xfc => Geometries[180 * 1024],
            0xff => Geometries[320 * 1024],
            0xfd => Geometries[360 * 1024],
            _ => default
        };
        return geometry.Cylinders > 0;
    }

    internal static bool TryIdentifyFluxGeometry(ReadOnlySpan<byte> boot, byte fatMedia, out IbmPcGeometry geometry)
    {
        var hasBpb = boot.Length >= 36 && BinaryPrimitives.ReadUInt16LittleEndian(boot[11..]) == 512;
        var oem = boot.Length >= 11 ? System.Text.Encoding.ASCII.GetString(boot.Slice(3, 8)).Trim('\0', ' ').ToUpperInvariant() : string.Empty;
        var knownDosOem = oem.Contains("IBM", StringComparison.Ordinal)
            || oem.Contains("MSDOS", StringComparison.Ordinal)
            || oem.Contains("MSWIN", StringComparison.Ordinal)
            || oem.Contains("DOS", StringComparison.Ordinal)
            || oem.Contains("FRDOS", StringComparison.Ordinal)
            || oem.Contains("FREEDOS", StringComparison.Ordinal)
            || oem.Contains("COPYDISK", StringComparison.Ordinal);
        var legacyDos = !hasBpb && fatMedia is 0xfe or 0xfc or 0xff or 0xfd;
        if ((knownDosOem || legacyDos) && TryDetectFluxGeometry(boot, fatMedia, out geometry)) return true;
        geometry = default;
        return false;
    }

    private static bool TryReadBpbGeometry(ReadOnlySpan<byte> data, out IbmPcGeometry geometry)
    {
        geometry = default;
        if (data.Length < 36) return false;
        var bytesPerSector = BinaryPrimitives.ReadUInt16LittleEndian(data[11..]);
        var totalSectors = (int)BinaryPrimitives.ReadUInt16LittleEndian(data[19..]);
        if (totalSectors == 0 && data.Length >= 36)
        {
            var largeTotal = BinaryPrimitives.ReadUInt32LittleEndian(data[32..]);
            if (largeTotal <= int.MaxValue) totalSectors = (int)largeTotal;
        }
        var sectorsPerTrack = BinaryPrimitives.ReadUInt16LittleEndian(data[24..]);
        var heads = BinaryPrimitives.ReadUInt16LittleEndian(data[26..]);
        if (bytesPerSector != 512 || totalSectors <= 0 || totalSectors != data.Length / 512
            || sectorsPerTrack is <= 0 or > 63 || heads is <= 0 or > 2
            || totalSectors % (sectorsPerTrack * heads) != 0) return false;
        var cylinders = totalSectors / (sectorsPerTrack * heads);
        if (cylinders is <= 0 or > 255) return false;
        geometry = new(FormatIdForGeometry(cylinders, heads, sectorsPerTrack), cylinders, heads, sectorsPerTrack);
        return true;
    }
}

internal readonly record struct IbmPcGeometry(string FormatId, int Cylinders, int Heads, int SectorsPerTrack);
