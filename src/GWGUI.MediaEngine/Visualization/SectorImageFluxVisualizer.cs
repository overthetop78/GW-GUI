using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Visualization;

/// <summary>Convertit une image sectorielle en image SCP synthétique destinée à la visualisation.</summary>
public sealed class SectorImageFluxVisualizer
{
    private readonly SectorImageTrackEncoder trackEncoder;

    /// <summary>Crée un visualiseur avec le registre d'encodeurs fourni ou le registre par défaut.</summary>
    /// <param name="encoders">Registre d'encodeurs optionnel.</param>
    public SectorImageFluxVisualizer(FluxEncoderRegistry? encoders = null) : this(encoders, new()) { }
    /// <summary>Crée un visualiseur avec ses deux registres injectés.</summary>
    internal SectorImageFluxVisualizer(FluxEncoderRegistry? encoders, SectorImageVisualizationPolicyRegistry policies)
    {
        trackEncoder = new(encoders ?? new FluxEncoderRegistry(), policies);
    }

    /// <summary>Indique si une politique permet de visualiser l'image.</summary>
    public bool CanVisualize(SectorImage image) => trackEncoder.CanEncode(image);

    /// <summary>Crée l'image SCP synthétique en conservant l'ordre cylindre puis face.</summary>
    /// <param name="image">Image sectorielle source.</param>
    /// <param name="cancellationToken">Jeton d'annulation consulté entre les pistes.</param>
    /// <returns>Image SCP utilisable par les composants de visualisation.</returns>
    /// <exception cref="NotSupportedException">Aucune politique ou aucun encodeur ne prend le format en charge.</exception>
    /// <exception cref="InvalidDataException">L'image ne produit aucune piste.</exception>
    public ScpImage Create(SectorImage image, CancellationToken cancellationToken = default)
    {
        var tracks = trackEncoder.Encode(image, cancellationToken).Select(item => new ScpTrack(ScpFormatConstants.ToTrackNumber(item.Cylinder, item.Head), item.Cylinder, item.Head, [new ScpRevolution(item.Track.Revolution, (uint)item.Track.Revolution.FluxIntervals.Count)])).ToArray();
        var start = tracks.Min(track => track.TrackNumber);
        var end = tracks.Max(track => track.TrackNumber);
        var heads = tracks.Select(track => track.Head).Distinct().Count() == 1 ? tracks[0].Head == 0 ? ScpHeadSelection.Side0 : ScpHeadSelection.Side1 : ScpHeadSelection.Both;
        var header = new ScpHeader(ScpVisualizationDefaults.Version, ScpVisualizationDefaults.DiskType, ScpVisualizationDefaults.RevolutionCount, start, end, ScpFlags.IndexAligned | ScpFlags.Writable, ScpBitCellEncoding.Default16Bit, heads, ScpVisualizationDefaults.Resolution, ScpVisualizationDefaults.Checksum);
        return new(header, tracks, true, image.Capacity);
    }

}
