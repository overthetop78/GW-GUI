namespace GWGUI.MediaEngine.Containers.I86f;

/// <summary>Définit les drapeaux portés par l'en-tête d'un fichier 86F.</summary>
[Flags]
public enum I86fFileFlags : ushort
{
    /// <summary>Aucun drapeau.</summary>
    None = 0,
    /// <summary>Le fichier contient deux faces.</summary>
    TwoSided = 0x0008,
    /// <summary>Masque du décalage de vitesse encodé dans les bits 5 et 6.</summary>
    SpeedShiftMask = 0x0060,
    /// <summary>Chaque piste possède le champ supplémentaire de quatre octets.</summary>
    ExtraBitCellCount = 0x0080,
    /// <summary>Les deux octets de chaque mot de piste sont stockés dans l'ordre inverse.</summary>
    ReverseByteOrder = 0x0800,
    /// <summary>Indique une accélération, ou un nombre total explicite de cellules lorsque le décalage de vitesse vaut zéro.</summary>
    SpeedupOrExplicitBitCount = 0x1000
}
