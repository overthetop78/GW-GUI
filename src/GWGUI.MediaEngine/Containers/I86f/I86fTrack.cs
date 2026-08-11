namespace GWGUI.MediaEngine.Containers.I86f;

/// <summary>Représente une piste 86F dont l'ordre des octets a été normalisé.</summary>
/// <param name="LogicalIndex">Index de la piste dans la table du conteneur.</param>
/// <param name="Flags">Drapeaux propres à la piste.</param>
/// <param name="BitCount">Nombre utile de cellules de bits.</param>
/// <param name="Bits">Cellules de bits normalisées, du bit de poids fort au bit de poids faible de chaque octet.</param>
public sealed record I86fTrack(int LogicalIndex, I86fTrackFlags Flags, int BitCount, IReadOnlyList<bool> Bits);
