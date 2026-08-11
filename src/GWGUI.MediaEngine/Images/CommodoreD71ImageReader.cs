using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class CommodoreD71ImageReader : ISectorImageReader
{
    private static readonly IReadOnlyDictionary<int, (int Tracks, bool ErrorMap)> Sizes = new Dictionary<int, (int, bool)>
    {
        [349_696] = (35, false), [351_062] = (35, true),
        [393_216] = (40, false), [394_752] = (40, true)
    };

    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.D71, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (!Sizes.TryGetValue(data.Length, out var geometry)) throw new InvalidDataException("The D71 image size is not supported.");
        var count = CommodoreGeometry.BlocksPer1541Side(geometry.Tracks) * 2;
        var blocks = new SectorBlock[count];
        for (var logical = 0; logical < count; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var address = CommodoreGeometry.From1541LogicalBlock(logical, geometry.Tracks, 2);
            var integrity = !geometry.ErrorMap || data[count * 256 + logical] == 1;
            blocks[logical] = new(logical, new(address.Track - 1, address.Side, address.Sector), data.AsSpan(logical * 256, 256).ToArray(), integrity);
        }
        return new(DiskImageFormatIds.Commodore1571, 256, geometry.Tracks, DiskGeometryConstants.DoubleSidedHeadCount, 21, blocks, capacity: count * 256L, logicalBlockCount: count);
    }
}
