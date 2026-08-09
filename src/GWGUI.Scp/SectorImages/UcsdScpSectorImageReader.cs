using GWGUI.Scp.Decoding;

namespace GWGUI.Scp.SectorImages;

/// <summary>Reads UCSD p-System sector images reconstructed from IBM MFM flux.</summary>
public sealed class UcsdScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    public Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) =>
        reader.ReadAsync(path, "ucsd.ibm.mfm", cancellationToken);
}
