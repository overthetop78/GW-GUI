using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Regroupe le résultat complet produit par un décodeur de flux.</summary>
public sealed record FluxDecodeResult
{
    /// <summary>Initialise un résultat de décodage en copiant toutes les collections fournies.</summary>
    public FluxDecodeResult(string DecoderId, string DisplayName, double Confidence, double EstimatedBitCellTicks, IReadOnlyList<FluxStructure> Structures, IReadOnlyList<byte> DecodedBytes, IReadOnlyList<DecodedSector>? Sectors = null)
    {
        this.DecoderId = DecoderId;
        this.DisplayName = DisplayName;
        this.Confidence = Confidence;
        this.EstimatedBitCellTicks = EstimatedBitCellTicks;
        this.Structures = new ReadOnlyCollection<FluxStructure>(Structures.ToArray());
        this.DecodedBytes = new ReadOnlyCollection<byte>(DecodedBytes.ToArray());
        this.Sectors = new ReadOnlyCollection<DecodedSector>((Sectors ?? []).ToArray());
    }

    public string DecoderId { get; init; }
    public string DisplayName { get; init; }
    public double Confidence { get; init; }
    public double EstimatedBitCellTicks { get; init; }
    public IReadOnlyList<FluxStructure> Structures { get; }
    public IReadOnlyList<byte> DecodedBytes { get; }
    public IReadOnlyList<DecodedSector> Sectors { get; }
}
