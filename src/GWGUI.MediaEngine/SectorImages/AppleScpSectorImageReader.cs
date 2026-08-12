using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Reconstruction.Apple;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Oriente les captures SCP Apple vers le reconstructeur Apple II, Macintosh/Lisa ou RWTS18.</summary>
public sealed class AppleScpSectorImageReader
{
    private readonly IScpReader _scpReader;
    private readonly AppleIIScpSectorReconstructor _appleII;
    private readonly AppleMacScpSectorReconstructor _macintosh;
    private readonly AppleRwts18ScpSectorReconstructor _rwts18;

    /// <summary>Crée le lecteur et ses reconstructeurs Apple spécialisés.</summary>
    public AppleScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
    {
        _scpReader = scpReader;
        var sectorDecoder = new AppleScpSectorDecoder(decoders);
        _appleII = new(sectorDecoder);
        _macintosh = new(sectorDecoder);
        _rwts18 = new(sectorDecoder);
    }

    /// <summary>Lit puis reconstruit une capture SCP Apple dans le format demandé ou détecté.</summary>
    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var scp = await _scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId?.StartsWith(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase) == true)
            return _rwts18.Decode(scp, cancellationToken);
        if (formatId?.StartsWith(DiskImageFormatIds.AppleIIAppleDosPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleIINoFileSystemPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleIIDosPrefix, StringComparison.OrdinalIgnoreCase) == true)
            return _appleII.Decode(scp, false, cancellationToken);
        if (formatId?.StartsWith(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase) == true)
            return _appleII.Decode(scp, true, cancellationToken);
        if (formatId?.StartsWith(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) == true ||
            formatId?.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) == true)
            return _macintosh.Decode(scp, formatId, cancellationToken);

        return DetectAutomatically(scp, cancellationToken);
    }

    /// <summary>Essaie chaque famille Apple et conserve l'image la plus complète.</summary>
    private SectorImage DetectAutomatically(ScpImage scp, CancellationToken cancellationToken)
    {
        var candidates = new List<SectorImage>(3);
        TryAdd(candidates, () => _macintosh.Decode(scp, null, cancellationToken));
        TryAdd(candidates, () => _appleII.Decode(scp, false, cancellationToken));
        TryAdd(candidates, () => _rwts18.Decode(scp, cancellationToken));
        if (candidates.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors("Apple GCR");
        return candidates.OrderByDescending(image =>
                image.AvailableBlocks.Count / (double)Math.Max(1, image.BlockCount))
            .ThenByDescending(image => image.AvailableBlocks.Count)
            .First();
    }

    /// <summary>Ajoute une reconstruction réussie sans interrompre la détection lorsqu'un encodage est absent.</summary>
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
