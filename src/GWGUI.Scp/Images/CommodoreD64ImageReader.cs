using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed class CommodoreD64ImageReader : ISectorImageReader
{
    private static readonly IReadOnlyDictionary<int, (int Tracks, bool ErrorMap)> Sizes = new Dictionary<int, (int, bool)>
    {
        [174_848] = (35, false), [175_531] = (35, true),
        [196_608] = (40, false), [197_376] = (40, true)
    };

    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.D64, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (!Sizes.TryGetValue(data.Length, out var geometry)) throw new InvalidDataException("The D64 image size is not supported.");
        var count = CommodoreGeometry.BlocksPer1541Side(geometry.Tracks);
        var blocks = new SectorBlock[count];
        for (var logical = 0; logical < count; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var address = CommodoreGeometry.From1541LogicalBlock(logical, geometry.Tracks, 1);
            var integrity = !geometry.ErrorMap || data[count * 256 + logical] == 1;
            blocks[logical] = new(logical, new(address.Track - 1, 0, address.Sector), data.AsSpan(logical * 256, 256).ToArray(), integrity);
        }
        return new("commodore.1541", 256, geometry.Tracks, 1, 21, blocks, capacity: count * 256L, logicalBlockCount: count);
    }
}
