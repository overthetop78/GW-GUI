using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Transforme une image sectorielle en pistes encodées en appliquant les politiques communes du moteur.</summary>
public sealed class SectorImageTrackEncoder
{
    private readonly FluxEncoderRegistry encoders;
    private readonly SectorImageVisualizationPolicyRegistry policies;

    /// <summary>Crée un encodeur de disque avec les registres par défaut.</summary>
    public SectorImageTrackEncoder() : this(new FluxEncoderRegistry(), new SectorImageVisualizationPolicyRegistry()) { }

    /// <summary>Crée un encodeur de disque avec des registres injectés.</summary>
    internal SectorImageTrackEncoder(FluxEncoderRegistry encoders, SectorImageVisualizationPolicyRegistry policies)
    {
        this.encoders = encoders;
        this.policies = policies;
    }

    /// <summary>Indique si une politique d'encodage accepte l'image.</summary>
    public bool CanEncode(SectorImage image) => policies.Resolve(image) is not null;

    /// <summary>Encode toutes les pistes disponibles dans l'ordre cylindre puis face.</summary>
    public IReadOnlyList<EncodedDiskTrack> Encode(SectorImage image, CancellationToken cancellationToken = default)
    {
        var policy = policies.Resolve(image) ?? throw SectorImageVisualizationExceptions.MissingPolicy(image.FormatId);
        var tracks = new List<EncodedDiskTrack>();
        foreach (var group in image.AvailableBlocks.Select(block => (Block: block, Address: policy.VisualAddress(image, block.Address))).GroupBy(item => (item.Address.Cylinder, item.Address.Head)).OrderBy(group => group.Key.Cylinder).ThenBy(group => group.Key.Head))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var items = group.OrderBy(item => item.Address.Number).ToArray();
            var sectors = policy.CreateTrackSectors(image, items);
            if (sectors.Count == 0) continue;
            var bitCellTicks = policy.BitCellTicks(image, group.Key.Cylinder);
            EncodedTrack encoded;
            try { encoded = encoders.Encode(policy.EncoderId(image), new TrackEncodeRequest(group.Key.Cylinder, group.Key.Head, sectors, policy.TrackAttributes(image, sectors.Count), bitCellTicks)); }
            catch (KeyNotFoundException) { throw SectorImageVisualizationExceptions.MissingPolicy(image.FormatId); }
            tracks.Add(new(group.Key.Cylinder, group.Key.Head, bitCellTicks, encoded));
        }
        if (tracks.Count == 0) throw SectorImageVisualizationExceptions.NoTrack(image.FormatId);
        return tracks.AsReadOnly();
    }
}
