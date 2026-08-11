using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reads BBC Micro and Acorn DFS sector images reconstructed from ISO FM flux.</summary>
public sealed class BbcScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    public Task<SectorImage> ReadAsync(string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!formatId.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase) &&
            !formatId.StartsWith(DiskImageFormatIds.AcornAdfsPrefix, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The selected format is not a BBC or Acorn format.", nameof(formatId));
        return reader.ReadAsync(path, formatId, cancellationToken);
    }
}
