using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reads IBM PC sector images reconstructed from ISO FM/MFM flux.</summary>
public sealed class IbmPcScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    public Task<SectorImage> ReadAsync(string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!formatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) &&
            !formatId.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The selected format is not an IBM-compatible ISO format.", nameof(formatId));
        return reader.ReadAsync(path, formatId, cancellationToken);
    }
}
