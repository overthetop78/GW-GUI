using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Commodore.D71;

/// <summary>Lit les quatre dispositions de conteneur Commodore D71.</summary>
public sealed class D71Reader
{
    /// <summary>Lit les deux faces successives d'un D71 avec leurs diagnostics sectoriels.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var layout = D71Layout.Find(data.Length) ?? throw D71Exceptions.UnknownLength(data.Length, D71Layout.Supported.Select(candidate => candidate.ImageLength));
        return Commodore1541SectorImageBuilder.Create(data, DiskImageFormatIds.Commodore1571, layout.TracksPerSide, 2, layout.DataBlockCount, layout.ErrorMapOffset, D71Exceptions.InvalidErrorMap, cancellationToken);
    }
}
