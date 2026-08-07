using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public interface ISectorImageReader
{
    bool CanRead(string path);
    Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default);
}

public sealed class AdfImageReader : ISectorImageReader
{
    public const int DoubleDensityBytes = 901_120;
    public const int HighDensityBytes = 1_802_240;

    public bool CanRead(string path) => Path.GetExtension(path).Equals(".adf", StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var sectorsPerTrack = data.Length switch
        {
            DoubleDensityBytes => 11,
            HighDensityBytes => 22,
            _ => throw new InvalidDataException("The ADF image is not an Amiga DD or HD sector image.")
        };
        var blocks = new SectorBlock[data.Length / 512];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var track = logical / sectorsPerTrack;
            blocks[logical] = new(logical, new(track / 2, track % 2, logical % sectorsPerTrack), data.AsSpan(logical * 512, 512).ToArray());
        }
        return new("amiga.amigados", 512, 80, 2, sectorsPerTrack, blocks);
    }
}
