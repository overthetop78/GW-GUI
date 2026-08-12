using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Visualization;

/// <summary>Convertit une image sectorielle en image SCP synthétique destinée à la visualisation.</summary>
public sealed class SectorImageFluxVisualizer
{
    private readonly FluxEncoderRegistry _encoders;
    private readonly SectorImageVisualizationPolicyRegistry _policies;

    /// <summary>Crée un visualiseur avec le registre d'encodeurs fourni ou le registre par défaut.</summary>
    /// <param name="encoders">Registre d'encodeurs optionnel.</param>
    public SectorImageFluxVisualizer(FluxEncoderRegistry? encoders = null) : this(encoders, new()) { }
    /// <summary>Crée un visualiseur avec ses deux registres injectés.</summary>
    internal SectorImageFluxVisualizer(FluxEncoderRegistry? encoders, SectorImageVisualizationPolicyRegistry policies)
    {
        _encoders = encoders ?? new FluxEncoderRegistry();
        _policies = policies;
    }

    /// <summary>Indique si une politique permet de visualiser l'image.</summary>
    public bool CanVisualize(SectorImage image) => _policies.Resolve(image) is not null;

    /// <summary>Crée l'image SCP synthétique en conservant l'ordre cylindre puis face.</summary>
    /// <param name="image">Image sectorielle source.</param>
    /// <param name="cancellationToken">Jeton d'annulation consulté entre les pistes.</param>
    /// <returns>Image SCP utilisable par les composants de visualisation.</returns>
    /// <exception cref="NotSupportedException">Aucune politique ou aucun encodeur ne prend le format en charge.</exception>
    /// <exception cref="InvalidDataException">L'image ne produit aucune piste.</exception>
    public ScpImage Create(SectorImage image, CancellationToken cancellationToken = default)
    {
        var policy = _policies.Resolve(image) ?? throw SectorImageVisualizationExceptions.MissingPolicy(image.FormatId);
        var tracks = new List<ScpTrack>();
        foreach (var group in image.AvailableBlocks
                     .Select(block => (Block: block, Address: policy.VisualAddress(image, block.Address)))
                     .GroupBy(item => (item.Address.Cylinder, item.Address.Head))
                     .OrderBy(group => group.Key.Cylinder).ThenBy(group => group.Key.Head))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var track = CreateTrack(image, policy, group.Key.Cylinder, group.Key.Head, group.OrderBy(item => item.Address.Number).ToArray());
            if (track is not null) tracks.Add(track);
        }
        if (tracks.Count == 0) throw SectorImageVisualizationExceptions.NoTrack(image.FormatId);
        var start = tracks.Min(track => track.TrackNumber);
        var end = tracks.Max(track => track.TrackNumber);
        var heads = tracks.Select(track => track.Head).Distinct().Count() == 1 ? tracks[0].Head == 0 ? ScpHeadSelection.Side0 : ScpHeadSelection.Side1 : ScpHeadSelection.Both;
        var header = new ScpHeader(ScpVisualizationDefaults.Version, ScpVisualizationDefaults.DiskType, ScpVisualizationDefaults.RevolutionCount, start, end, ScpFlags.IndexAligned | ScpFlags.Writable, ScpBitCellEncoding.Default16Bit, heads, ScpVisualizationDefaults.Resolution, ScpVisualizationDefaults.Checksum);
        return new(header, tracks, true, image.Capacity);
    }

    /// <summary>Construit et encode une piste, ou ne produit rien lorsque la politique ne retourne aucun secteur.</summary>
    /// <param name="image">Image sectorielle source.</param>
    /// <param name="policy">Politique compatible avec l'image.</param>
    /// <param name="cylinder">Cylindre de la piste.</param>
    /// <param name="head">Face de la piste.</param>
    /// <param name="items">Blocs de la piste classés par numéro de secteur.</param>
    /// <returns>Piste SCP encodée, ou <see langword="null"/> si aucun secteur n'est produit.</returns>
    private ScpTrack? CreateTrack(SectorImage image, ISectorImageVisualizationPolicy policy, int cylinder, int head, IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items)
    {
        var sectors = policy.CreateTrackSectors(image, items);
        if (sectors.Count == 0) return null;
        EncodedTrack encoded;
        try
        {
            encoded = _encoders.Encode(policy.EncoderId(image), new TrackEncodeRequest(cylinder, head, sectors, policy.TrackAttributes(image, sectors.Count), policy.BitCellTicks(image, cylinder)));
        }
        catch (KeyNotFoundException)
        {
            throw SectorImageVisualizationExceptions.MissingPolicy(image.FormatId);
        }
        var trackNumber = ScpFormatConstants.ToTrackNumber(cylinder, head);
        return new(trackNumber, cylinder, head, [new ScpRevolution(encoded.Revolution, (uint)encoded.Revolution.FluxIntervals.Count)]);
    }
}
