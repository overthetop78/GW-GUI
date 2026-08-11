using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class MsxImageReader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (!LooksLikeMsx(data)) throw new InvalidDataException("The image does not contain an MSX-DOS boot sector.");
        var (format, cylinders, heads, sectors) = data.Length switch
        {
            184_320 => (DiskImageFormatIds.Msx1D, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, 9),
            368_640 when data[21] == 0xf8 => (DiskImageFormatIds.Msx1Dd, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, 9),
            368_640 => (DiskImageFormatIds.Msx2D, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 9),
            737_280 => (DiskImageFormatIds.Msx2Dd, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 9),
            _ => throw new InvalidDataException("The MSX disk geometry is not supported.")
        };
        var blocks = new SectorBlock[data.Length / 512];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            var track = logical / sectors;
            blocks[logical] = new(logical, new(track / heads, track % heads, logical % sectors + 1),
                data.AsSpan(logical * 512, 512).ToArray());
        }
        return new(format, 512, cylinders, heads, sectors, blocks);
    }

    public static bool LooksLikeMsx(ReadOnlySpan<byte> data)
    {
        if (data.Length < 512 || data.Length % 512 != 0) return false;
        var oem = System.Text.Encoding.ASCII.GetString(data.Slice(3, 8));
        return oem.StartsWith("MSX", StringComparison.OrdinalIgnoreCase)
            && data[11] == 0 && data[12] == 2
            && data[13] > 0 && data[16] > 0;
    }
}
