namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Décrit un bloc logique, son adresse physique, ses données et les métadonnées issues de sa lecture.</summary>
/// <param name="LogicalBlock">Indice logique du bloc, compté à partir de zéro.</param>
/// <param name="Address">Adresse physique associée au bloc.</param>
/// <param name="Data">Données du bloc, exprimées en octets.</param>
/// <param name="IntegrityValid"><see langword="true"/> si l'intégrité est valide, <see langword="false"/> si elle est invalide, ou <see langword="null"/> si elle n'a pas pu être déterminée.</param>
/// <param name="Revolution">Indice de la révolution source, compté à partir de zéro.</param>
/// <param name="Tag">Métadonnées sectorielles facultatives, exprimées en octets.</param>
/// <param name="FormatCode">Octet de format sectoriel facultatif.</param>
public sealed record SectorBlock(int LogicalBlock, SectorAddress Address, IReadOnlyList<byte> Data, bool? IntegrityValid = true, int Revolution = 0, IReadOnlyList<byte>? Tag = null, byte? FormatCode = null);
