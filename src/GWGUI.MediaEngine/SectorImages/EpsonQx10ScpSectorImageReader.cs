using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reads Epson QX-10 sector images reconstructed from ISO FM/MFM flux.</summary>
public sealed class EpsonQx10ScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    public Task<SectorImage> ReadAsync(string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!formatId.StartsWith("epson.qx10.", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The selected format is not an Epson QX-10 format.", nameof(formatId));
        return reader.ReadAsync(path, formatId, cancellationToken);
    }
}
