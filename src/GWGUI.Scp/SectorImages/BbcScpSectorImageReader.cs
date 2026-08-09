using GWGUI.Scp.Decoding;

namespace GWGUI.Scp.SectorImages;

/// <summary>Reads BBC Micro and Acorn DFS sector images reconstructed from ISO FM flux.</summary>
public sealed class BbcScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    public Task<SectorImage> ReadAsync(string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!formatId.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase) &&
            !formatId.StartsWith("acorn.adfs.", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The selected format is not a BBC or Acorn format.", nameof(formatId));
        return reader.ReadAsync(path, formatId, cancellationToken);
    }
}
