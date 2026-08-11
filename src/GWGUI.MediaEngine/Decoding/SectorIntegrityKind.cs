namespace GWGUI.MediaEngine.Decoding;

/// <summary>Identifie le contrôle d'intégrité associé à un secteur décodé.</summary>
public enum SectorIntegrityKind
{
    /// <summary>Contrôle effectué avec un code de redondance cyclique.</summary>
    Crc,
    /// <summary>Contrôle effectué avec une somme de contrôle.</summary>
    Checksum
}
