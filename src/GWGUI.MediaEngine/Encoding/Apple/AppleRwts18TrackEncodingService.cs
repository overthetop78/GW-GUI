using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Encoding.Apple;

/// <summary>Encode les secteurs d'une image RWTS18 en pistes binaires indépendantes de leur conteneur de destination.</summary>
public sealed class AppleRwts18TrackEncodingService
{
    private readonly FluxEncoderRegistry _encoders;

    /// <summary>Crée le service avec le registre d'encodeurs fourni ou le catalogue par défaut.</summary>
    /// <param name="encoders">Registre d'encodeurs optionnel.</param>
    public AppleRwts18TrackEncodingService(FluxEncoderRegistry? encoders = null) => _encoders = encoders ?? new FluxEncoderRegistry();

    /// <summary>Encode toutes les pistes RWTS18 et valide chaque secteur attendu.</summary>
    /// <param name="image">Image sectorielle RWTS18.</param>
    /// <param name="maximumBits">Nombre maximal de bits admis dans une piste de destination.</param>
    /// <param name="cancellationToken">Jeton d'annulation consulté entre les pistes.</param>
    /// <returns>Pistes encodées dans l'ordre des cylindres.</returns>
    public IReadOnlyList<IReadOnlyList<bool>> Encode(SectorImage image, int maximumBits, CancellationToken cancellationToken = default)
    {
        if (!image.FormatId.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase)) throw AppleRwts18EncodingExceptions.UnsupportedSource(image.FormatId);
        var tracks = new List<IReadOnlyList<bool>>(image.Cylinders);
        for (var cylinder = 0; cylinder < image.Cylinders; cylinder++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sectors = new List<TrackSector>(AppleRwts18Format.SectorCount);
            for (var sector = 0; sector < AppleRwts18Format.SectorCount; sector++)
            {
                var logical = cylinder * AppleRwts18Format.SectorCount + sector;
                if (!image.TryGetBlock(logical, out var block) || block.Data.Count != AppleRwts18Format.SectorByteCount) throw AppleRwts18EncodingExceptions.InvalidSector(cylinder, sector, block?.Data.Count ?? 0, AppleRwts18Format.SectorByteCount);
                sectors.Add(new(sector, block.Data));
            }
            var encoded = _encoders.Encode(FluxCodecIds.AppleRwts18, new(cylinder, AppleRwts18Format.LogicalHead, sectors));
            if (encoded.Bits.Count > maximumBits) throw AppleRwts18EncodingExceptions.TrackTooLong(cylinder, encoded.Bits.Count, maximumBits);
            tracks.Add(encoded.Bits);
        }
        return tracks;
    }
}
