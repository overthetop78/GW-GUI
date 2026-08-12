namespace GWGUI.MediaEngine.Encoding;

/// <summary>Regroupe les données nécessaires pour encoder une piste logique complète.</summary>
/// <param name="Cylinder">Numéro du cylindre.</param>
/// <param name="Head">Numéro de la face.</param>
/// <param name="Sectors">Secteurs à placer sur la piste.</param>
/// <param name="Attributes">Attributs techniques propres au format.</param>
/// <param name="BitCellTicks">Durée d'une cellule binaire, en ticks.</param>
/// <param name="IndexTimeTicks">Durée d'une révolution, en ticks.</param>
public sealed record TrackEncodeRequest(int Cylinder, int Head, IReadOnlyList<TrackSector> Sectors, IReadOnlyDictionary<string, int>? Attributes = null, uint BitCellTicks = 40, uint IndexTimeTicks = 8_000_000);
