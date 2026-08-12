namespace GWGUI.MediaEngine.Geometries.Commodore;

/// <summary>Adresse 1541 utilisant une piste indexée à un, un secteur indexé à zéro et une face indexée à zéro.</summary>
/// <param name="Track">Piste indexée à un.</param>
/// <param name="Sector">Secteur indexé à zéro.</param>
/// <param name="Side">Face indexée à zéro.</param>
public readonly record struct Commodore1541Address(int Track, int Sector, int Side);
