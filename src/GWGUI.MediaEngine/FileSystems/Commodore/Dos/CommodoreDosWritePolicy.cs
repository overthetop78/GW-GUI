namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Définit explicitement le type appliqué aux fichiers qui ne proviennent pas de Commodore DOS.</summary>
public sealed record CommodoreDosWritePolicy
{
    /// <summary>Crée une politique avec un type de fichier et une longueur d'enregistrement REL explicites.</summary>
    public CommodoreDosWritePolicy(CommodoreDosFileType defaultFileType = CommodoreDosFileType.Prg, byte relativeRecordLength = byte.MaxValue)
    {
        var baseType = defaultFileType & CommodoreDosFileType.BaseTypeMask;
        if (baseType is not (CommodoreDosFileType.Seq or CommodoreDosFileType.Prg or CommodoreDosFileType.Usr or CommodoreDosFileType.Rel)) throw new ArgumentOutOfRangeException(nameof(defaultFileType));
        if (baseType == CommodoreDosFileType.Rel && relativeRecordLength == 0) throw new ArgumentOutOfRangeException(nameof(relativeRecordLength));
        DefaultFileType = defaultFileType | CommodoreDosFileType.Closed;
        RelativeRecordLength = relativeRecordLength;
    }

    /// <summary>Type attribué aux fichiers sans métadonnées Commodore.</summary>
    public CommodoreDosFileType DefaultFileType { get; }

    /// <summary>Longueur d'enregistrement utilisée lorsque le type par défaut est REL.</summary>
    public byte RelativeRecordLength { get; }
}
