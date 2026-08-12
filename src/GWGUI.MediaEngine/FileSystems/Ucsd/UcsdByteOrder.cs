namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Décrit l'ordre des octets d'un répertoire UCSD.</summary>
public enum UcsdByteOrder
{
    /// <summary>Octet faible en premier.</summary>
    LittleEndian,
    /// <summary>Octet fort en premier.</summary>
    BigEndian
}

/// <summary>Résultat de la détection de l'ordre des octets.</summary>
public readonly record struct UcsdByteOrderDetection(bool Success, UcsdByteOrder ByteOrder);
