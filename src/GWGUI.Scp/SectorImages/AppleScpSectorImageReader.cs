using GWGUI.Scp.Decoding;
using GWGUI.Scp.Containers.Scp;

namespace GWGUI.Scp.SectorImages;

/// <summary>Routes Apple SCP images to the Apple II, Macintosh/Lisa or RWTS18 reconstructor.</summary>
public sealed class AppleScpSectorImageReader
{
    private readonly IScpReader _scpReader;
    private readonly AppleIIScpSectorReconstructor _appleII;
    private readonly AppleMacScpSectorReconstructor _macintosh;
    private readonly AppleRwts18ScpSectorReconstructor _rwts18;

    public AppleScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
    {
        _scpReader = scpReader;
        var sectorDecoder = new AppleScpSectorDecoder(decoders);
        _appleII = new(sectorDecoder);
        _macintosh = new(sectorDecoder);
        _rwts18 = new(sectorDecoder);
    }

    public async Task<SectorImage> ReadAsync(string path, string? formatId = null,
        CancellationToken cancellationToken = default)
    {
        var scp = await _scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith("apple2.rwts18", StringComparison.OrdinalIgnoreCase) == true)
            return _rwts18.Decode(scp, cancellationToken);
        if (formatId?.StartsWith("apple2.appledos", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("apple2.nofs", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("apple2.dos", StringComparison.OrdinalIgnoreCase) == true)
            return _appleII.Decode(scp, false, cancellationToken);
        if (formatId?.StartsWith("apple2.prodos.140", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("apple3.sos", StringComparison.OrdinalIgnoreCase) == true)
            return _appleII.Decode(scp, true, cancellationToken);
        if (formatId?.StartsWith("apple2.prodos.800", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("mac.", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("applemac", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith("applelisa", StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.Equals("apple2.prodos", StringComparison.OrdinalIgnoreCase) == true)
            return _macintosh.Decode(scp, formatId, cancellationToken);

        return DetectAutomatically(scp, cancellationToken);
    }

    private SectorImage DetectAutomatically(ScpImage scp, CancellationToken cancellationToken)
    {
        var candidates = new List<SectorImage>(3);
        TryAdd(candidates, () => _macintosh.Decode(scp, null, cancellationToken));
        TryAdd(candidates, () => _appleII.Decode(scp, false, cancellationToken));
        TryAdd(candidates, () => _rwts18.Decode(scp, cancellationToken));
        if (candidates.Count == 0)
            throw new InvalidDataException("No Apple GCR sectors could be decoded from the SCP image.");
        return candidates.OrderByDescending(image =>
                image.AvailableBlocks.Count / (double)Math.Max(1, image.BlockCount))
            .ThenByDescending(image => image.AvailableBlocks.Count)
            .First();
    }

    private static void TryAdd(ICollection<SectorImage> candidates, Func<SectorImage> decode)
    {
        try
        {
            candidates.Add(decode());
        }
        catch (InvalidDataException)
        {
            // This Apple encoding is not present; automatic detection evaluates the others.
        }
    }
}
