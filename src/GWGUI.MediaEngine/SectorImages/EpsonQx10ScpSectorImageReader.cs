using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Reconstruction.EpsonQx10;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reconstruit les images Epson QX-10 depuis un flux ISO FM ou MFM.</summary>
public sealed class EpsonQx10ScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    public Task<SectorImage> ReadAsync(string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!formatId.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase))
            throw EpsonQx10Exceptions.InvalidFormat(formatId);
        return reader.ReadAsync(path, formatId, cancellationToken);
    }
}
    /// <summary>Lit une capture SCP selon la disposition Epson QX-10 demandée.</summary>
