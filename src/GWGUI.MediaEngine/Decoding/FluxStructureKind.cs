namespace GWGUI.MediaEngine.Decoding;

/// <summary>Identifie la nature d'une structure repérée dans un flux décodé.</summary>
public enum FluxStructureKind
{
    /// <summary>Zone de synchronisation générique.</summary>
    Sync,
    /// <summary>Marque d'adresse d'identification ISO.</summary>
    IdAddressMark,
    /// <summary>Marque d'adresse de données ISO.</summary>
    DataAddressMark,
    /// <summary>Marque d'adresse de données supprimées ISO.</summary>
    DeletedDataAddressMark,
    /// <summary>Mot de synchronisation Amiga.</summary>
    AmigaSync,
    /// <summary>Champ d'adresse Apple.</summary>
    AppleAddress,
    /// <summary>Champ de données Apple.</summary>
    AppleData,
    /// <summary>Zone de synchronisation Commodore.</summary>
    CommodoreSync,
    /// <summary>En-tête de secteur Commodore.</summary>
    CommodoreHeader,
    /// <summary>En-tête propre à un format spécialisé.</summary>
    FormatHeader,
    /// <summary>Données propres à un format spécialisé.</summary>
    FormatData,
    /// <summary>Anomalie détectée dans la temporisation du flux.</summary>
    TimingAnomaly
}
