using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Décrit un secteur logique à encoder dans une piste.</summary>
public sealed record TrackSector
{
    /// <summary>Initialise un secteur en copiant sa charge utile et ses attributs.</summary>
    /// <param name="Number">Numéro logique du secteur.</param>
    /// <param name="Data">Octets de la charge utile.</param>
    /// <param name="Deleted">Indique si une marque de données supprimées doit être utilisée.</param>
    /// <param name="SizeCode">Code de taille imposé par le format, ou <see langword="null"/> pour le déduire.</param>
    /// <param name="Attributes">Attributs techniques propres au format.</param>
    public TrackSector(int Number, IReadOnlyList<byte> Data, bool Deleted = false, byte? SizeCode = null, IReadOnlyDictionary<string, int>? Attributes = null)
    {
        ArgumentNullException.ThrowIfNull(Data);
        this.Number = Number;
        this.Data = Array.AsReadOnly(Data.ToArray());
        this.Deleted = Deleted;
        this.SizeCode = SizeCode;
        this.Attributes = Attributes is null ? null : new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(Attributes, StringComparer.Ordinal));
    }

    /// <summary>Obtient le numéro logique du secteur.</summary>
    public int Number { get; }
    /// <summary>Obtient une copie non modifiable des octets de la charge utile.</summary>
    public IReadOnlyList<byte> Data { get; }
    /// <summary>Indique si une marque de données supprimées doit être utilisée.</summary>
    public bool Deleted { get; }
    /// <summary>Obtient le code de taille imposé, ou <see langword="null"/> pour le déduire.</summary>
    public byte? SizeCode { get; }
    /// <summary>Obtient une copie non modifiable des attributs propres au format.</summary>
    public IReadOnlyDictionary<string, int>? Attributes { get; }
}
