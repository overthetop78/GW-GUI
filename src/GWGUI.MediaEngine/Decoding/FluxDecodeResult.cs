using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Regroupe le résultat complet produit par un décodeur de flux.</summary>
public sealed record FluxDecodeResult
{
    /// <summary>Initialise un résultat de décodage en copiant toutes les collections fournies.</summary>
    /// <param name="DecoderId">Identifiant technique du décodeur.</param>
    /// <param name="DisplayName">Nom affichable du décodeur.</param>
    /// <param name="Confidence">Indice de confiance normalisé entre zéro et un.</param>
    /// <param name="EstimatedBitCellTicks">Durée estimée d'une cellule de bit, en pas temporels SCP.</param>
    /// <param name="Structures">Structures reconnues dans le flux.</param>
    /// <param name="DecodedBytes">Octets reconstruits depuis le flux.</param>
    /// <param name="Sectors">Secteurs reconstruits, ou valeur nulle lorsqu'aucun secteur n'est disponible.</param>
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

    /// <summary>Obtient l'identifiant technique du décodeur.</summary>
    public string DecoderId { get; init; }
    /// <summary>Obtient le nom affichable du décodeur.</summary>
    public string DisplayName { get; init; }
    /// <summary>Obtient l'indice de confiance normalisé entre zéro et un.</summary>
    public double Confidence { get; init; }
    /// <summary>Obtient la durée estimée d'une cellule de bit, en pas temporels SCP.</summary>
    public double EstimatedBitCellTicks { get; init; }
    /// <summary>Obtient la copie non modifiable des structures reconnues.</summary>
    public IReadOnlyList<FluxStructure> Structures { get; }
    /// <summary>Obtient la copie non modifiable des octets reconstruits.</summary>
    public IReadOnlyList<byte> DecodedBytes { get; }
    /// <summary>Obtient la copie non modifiable des secteurs reconstruits ; la collection est vide lorsqu'aucun secteur n'est disponible.</summary>
    public IReadOnlyList<DecodedSector> Sectors { get; }
}
