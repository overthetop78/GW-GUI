namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décrit une structure technique localisée dans un flux décodé.</summary>
/// <param name="Kind">Type de structure.</param><param name="BitOffset">Position de départ en bits.</param><param name="BitLength">Longueur en bits.</param><param name="Description">Description produite par <see cref="FluxStructureDescriptions"/>.</param>
public sealed record FluxStructure(FluxStructureKind Kind, int BitOffset, int BitLength, string Description);
