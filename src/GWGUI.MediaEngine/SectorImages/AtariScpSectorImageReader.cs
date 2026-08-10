using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reads Atari 8-bit and Atari ST sector images reconstructed from ISO FM/MFM flux.</summary>
public sealed class AtariScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly IsoScpSectorImageReader reader = new(scpReader, decoders);

    public Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        if (formatId is not null &&
            !formatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) &&
            !formatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The selected format is not an Atari format.", nameof(formatId));

        return reader.ReadAsync(path, formatId, cancellationToken);
    }
}
