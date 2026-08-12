using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Regroupe les données nécessaires pour encoder une piste logique complète.</summary>
public sealed record TrackEncodeRequest
{
    /// <summary>Initialise une requête en copiant ses secteurs et ses attributs.</summary>
    /// <param name="Cylinder">Numéro du cylindre.</param>
    /// <param name="Head">Numéro de la face.</param>
    /// <param name="Sectors">Secteurs à placer sur la piste.</param>
    /// <param name="Attributes">Attributs techniques propres au format.</param>
    /// <param name="BitCellTicks">Durée d'une cellule binaire, en ticks.</param>
    /// <param name="IndexTimeTicks">Durée d'une révolution, en ticks.</param>
    public TrackEncodeRequest(int Cylinder, int Head, IReadOnlyList<TrackSector> Sectors, IReadOnlyDictionary<string, int>? Attributes = null, uint BitCellTicks = TrackEncodingDefaults.BitCellTicks, uint IndexTimeTicks = TrackEncodingDefaults.IndexTimeTicks)
    {
        ArgumentNullException.ThrowIfNull(Sectors);
        this.Cylinder = Cylinder;
        this.Head = Head;
        this.Sectors = Array.AsReadOnly(Sectors.ToArray());
        this.Attributes = Attributes is null ? null : new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(Attributes, StringComparer.Ordinal));
        this.BitCellTicks = BitCellTicks;
        this.IndexTimeTicks = IndexTimeTicks;
    }

    /// <summary>Obtient le numéro du cylindre.</summary>
    public int Cylinder { get; }
    /// <summary>Obtient le numéro de la face.</summary>
    public int Head { get; }
    /// <summary>Obtient une copie non modifiable des secteurs à encoder.</summary>
    public IReadOnlyList<TrackSector> Sectors { get; }
    /// <summary>Obtient une copie non modifiable des attributs propres au format.</summary>
    public IReadOnlyDictionary<string, int>? Attributes { get; }
    /// <summary>Obtient la durée d'une cellule binaire, en ticks.</summary>
    public uint BitCellTicks { get; }
    /// <summary>Obtient la durée d'une révolution complète, en ticks.</summary>
    public uint IndexTimeTicks { get; }
}
