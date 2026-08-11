namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Identifie la position physique d'un secteur par son cylindre, sa tête et son numéro.</summary>
/// <param name="Cylinder">Indice de cylindre, généralement compté à partir de zéro.</param>
/// <param name="Head">Indice de tête, compté à partir de zéro.</param>
/// <param name="Number">Numéro de secteur tel qu'il est porté par le format source.</param>
public sealed record SectorAddress(int Cylinder, int Head, int Number);
