using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décrit un secteur reconstruit à partir d'un flux.</summary>
public sealed record DecodedSector
{
    /// <summary>Initialise un secteur décodé en copiant ses données et son étiquette lorsqu'elles sont fournies.</summary>
    /// <param name="Cylinder">Numéro du cylindre contenant le secteur.</param>
    /// <param name="Head">Numéro de la face contenant le secteur.</param>
    /// <param name="Number">Numéro logique du secteur.</param>
    /// <param name="SizeCode">Code de taille enregistré dans le format.</param>
    /// <param name="SizeBytes">Taille du secteur en octets.</param>
    /// <param name="IntegrityValid">Résultat du contrôle d'intégrité, ou valeur nulle lorsque le format ne permet pas de le déterminer.</param>
    /// <param name="BitOffset">Position du secteur dans le flux décodé, en bits.</param>
    /// <param name="IntegrityKind">Nature du contrôle d'intégrité.</param>
    /// <param name="Data">Données du secteur lorsqu'elles ont été reconstruites.</param>
    /// <param name="Tag">Étiquette associée au secteur lorsqu'elle existe.</param>
    public DecodedSector(byte Cylinder, byte Head, int Number, byte SizeCode, int SizeBytes, bool? IntegrityValid, int BitOffset, SectorIntegrityKind IntegrityKind = SectorIntegrityKind.Crc, IReadOnlyList<byte>? Data = null, IReadOnlyList<byte>? Tag = null)
    {
        this.Cylinder = Cylinder;
        this.Head = Head;
        this.Number = Number;
        this.SizeCode = SizeCode;
        this.SizeBytes = SizeBytes;
        this.IntegrityValid = IntegrityValid;
        this.BitOffset = BitOffset;
        this.IntegrityKind = IntegrityKind;
        this.Data = Data is null ? null : new ReadOnlyCollection<byte>(Data.ToArray());
        this.Tag = Tag is null ? null : new ReadOnlyCollection<byte>(Tag.ToArray());
    }

    /// <summary>Obtient le numéro du cylindre contenant le secteur.</summary>
    public byte Cylinder { get; init; }
    /// <summary>Obtient le numéro de la face contenant le secteur.</summary>
    public byte Head { get; init; }
    /// <summary>Obtient le numéro logique du secteur.</summary>
    public int Number { get; init; }
    /// <summary>Obtient le code de taille enregistré dans le format.</summary>
    public byte SizeCode { get; init; }
    /// <summary>Obtient la taille du secteur en octets.</summary>
    public int SizeBytes { get; init; }
    /// <summary>Obtient le résultat du contrôle d'intégrité, ou une valeur nulle lorsque ce résultat est indéterminé.</summary>
    public bool? IntegrityValid { get; init; }
    /// <summary>Obtient la position du secteur dans le flux décodé, en bits.</summary>
    public int BitOffset { get; init; }
    /// <summary>Obtient la nature du contrôle d'intégrité.</summary>
    public SectorIntegrityKind IntegrityKind { get; init; }
    /// <summary>Obtient la copie non modifiable des données du secteur, ou une valeur nulle lorsque les données sont indisponibles.</summary>
    public IReadOnlyList<byte>? Data { get; }
    /// <summary>Obtient la copie non modifiable de l'étiquette du secteur, ou une valeur nulle lorsqu'elle est absente.</summary>
    public IReadOnlyList<byte>? Tag { get; }
}
