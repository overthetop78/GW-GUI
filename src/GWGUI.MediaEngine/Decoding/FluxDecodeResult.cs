namespace GWGUI.MediaEngine.Decoding;

/// <summary>Regroupe le résultat complet produit par un décodeur de flux.</summary>
/// <param name="DecoderId">Identifiant technique du décodeur.</param><param name="DisplayName">Nom affiché du décodeur.</param><param name="Confidence">Confiance normalisée entre zéro et un.</param><param name="EstimatedBitCellTicks">Durée estimée d'une cellule en ticks.</param><param name="Structures">Structures reconnues.</param><param name="DecodedBytes">Octets décodés.</param><param name="Sectors">Secteurs reconstruits lorsqu'ils sont disponibles.</param>
public sealed record FluxDecodeResult(string DecoderId, string DisplayName, double Confidence, double EstimatedBitCellTicks, IReadOnlyList<FluxStructure> Structures, IReadOnlyList<byte> DecodedBytes, IReadOnlyList<DecodedSector>? Sectors = null);
