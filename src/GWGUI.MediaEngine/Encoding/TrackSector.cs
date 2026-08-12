namespace GWGUI.MediaEngine.Encoding;

/// <summary>Décrit un secteur logique à encoder dans une piste.</summary>
/// <param name="Number">Numéro logique du secteur.</param>
/// <param name="Data">Octets de la charge utile.</param>
/// <param name="Deleted">Indique si une marque de données supprimées doit être utilisée.</param>
/// <param name="SizeCode">Code de taille imposé par le format, ou <see langword="null"/> pour le déduire.</param>
/// <param name="Attributes">Attributs techniques propres au format.</param>
public sealed record TrackSector(int Number, IReadOnlyList<byte> Data, bool Deleted = false, byte? SizeCode = null, IReadOnlyDictionary<string, int>? Attributes = null);
