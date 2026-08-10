using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed class CommodoreD81ImageReader : ISectorImageReader
{
    public const int ImageBytes = 819_200;
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.D81, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length != ImageBytes) throw new InvalidDataException("The D81 image must contain exactly 819200 bytes.");
        var blocks = new SectorBlock[3_200];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var track = logical / 40 + 1;
            var sector = logical % 40;
            blocks[logical] = new(logical, new(track - 1, 0, sector), data.AsSpan(logical * 256, 256).ToArray());
        }
        return new("commodore.1581", 256, 80, 1, 40, blocks);
    }
}
