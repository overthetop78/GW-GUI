using GWGUI.Scp.Decoding;

namespace GWGUI.Scp.SectorImages;

/// <summary>Reads Amstrad CPC and PCW sector images reconstructed from ISO FM/MFM flux.</summary>
public sealed class AmstradScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    public Task<SectorImage> ReadAsync(string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!formatId.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The selected format is not an Amstrad format.", nameof(formatId));
        return reader.ReadAsync(path, formatId, cancellationToken);
    }
}
