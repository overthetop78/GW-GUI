using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed class BbcDfsImageReader : ISectorImageReader
{
    private const int SectorSize = 256;
    private const int SectorsPerTrack = 10;
    private const int TrackBytes = SectorSize * SectorsPerTrack;

    public bool CanRead(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".ssd", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".dsd", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var extension = Path.GetExtension(path);
        var heads = extension.Equals(".dsd", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
        if (data.Length == 0 || data.Length % (TrackBytes * heads) != 0)
            throw new InvalidDataException("The BBC DFS image does not contain a whole number of tracks.");
        var cylinders = data.Length / (TrackBytes * heads);
        if (cylinders is not (40 or 80))
            throw new InvalidDataException("The BBC DFS image is not a 40-track or 80-track SSD/DSD image.");

        var blocks = new List<SectorBlock>(data.Length / SectorSize);
        for (var cylinder = 0; cylinder < cylinders; cylinder++)
        for (var head = 0; head < heads; head++)
        for (var sector = 0; sector < SectorsPerTrack; sector++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // DSD stores complete tracks interleaved by side.
            var source = ((cylinder * heads + head) * SectorsPerTrack + sector) * SectorSize;
            var logical = (cylinder * heads + head) * SectorsPerTrack + sector;
            blocks.Add(new(logical, new(cylinder, head, sector), data.AsSpan(source, SectorSize).ToArray()));
        }
        var format = heads == 1
            ? cylinders == 40 ? "acorn.dfs.ss" : "acorn.dfs.ss80"
            : cylinders == 40 ? "acorn.dfs.ds" : "acorn.dfs.ds80";
        return new(format, SectorSize, cylinders, heads, SectorsPerTrack, blocks);
    }
}
