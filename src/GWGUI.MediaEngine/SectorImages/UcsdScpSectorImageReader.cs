using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reads UCSD p-System sector images reconstructed from IBM MFM flux.</summary>
public sealed class UcsdScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    public Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) =>
        reader.ReadAsync(path, DiskImageFormatIds.UcsdIbmMfm, cancellationToken);
}
