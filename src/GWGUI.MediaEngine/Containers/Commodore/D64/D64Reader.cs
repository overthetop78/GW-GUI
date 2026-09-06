using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Commodore.D64;

/// <summary>Lit les quatre dispositions de conteneur Commodore D64.</summary>
public sealed class D64Reader
{
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public D64Reader() : this(File.ReadAllBytesAsync) { }

    internal D64Reader(Func<string, CancellationToken, Task<byte[]>> readBytes)
    {
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));
    }

    /// <summary>Lit un conteneur D64 et reconstruit ses secteurs avec leurs diagnostics.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        var layout = D64Layout.Find(data.Length) ?? throw D64Exceptions.UnknownLength(data.Length, D64Layout.Supported.Select(candidate => candidate.ImageLength));
        return Commodore1541SectorImageBuilder.Create(data, DiskImageFormatIds.Commodore1541, layout.TrackCount, 1, layout.DataBlockCount, layout.ErrorMapOffset, D64Exceptions.InvalidErrorMap, cancellationToken);
    }
}
