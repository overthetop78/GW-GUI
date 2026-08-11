using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Definitions;

/// <summary>Définit et construit les identifiants des images de disquettes IBM PC.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Préfixe des formats IBM PC.</summary>
    public const string IbmPrefix = "ibm.";
    /// <summary>Image IBM PC de 160 Kio.</summary>
    public const string Ibm160 = "ibm.160";
    /// <summary>Image IBM PC de 180 Kio.</summary>
    public const string Ibm180 = "ibm.180";
    /// <summary>Image IBM PC de 320 Kio.</summary>
    public const string Ibm320 = "ibm.320";
    /// <summary>Image IBM PC de 360 Kio.</summary>
    public const string Ibm360 = "ibm.360";
    /// <summary>Image IBM PC de 720 Kio.</summary>
    public const string Ibm720 = "ibm.720";
    /// <summary>Image IBM PC de 800 Kio.</summary>
    public const string Ibm800 = "ibm.800";
    /// <summary>Image IBM PC de 1 200 Kio.</summary>
    public const string Ibm1200 = "ibm.1200";
    /// <summary>Image IBM PC de 1 440 Kio.</summary>
    public const string Ibm1440 = "ibm.1440";
    /// <summary>Image IBM PC de 1 680 Kio.</summary>
    public const string Ibm1680 = "ibm.1680";
    /// <summary>Image IBM PC au format DMF.</summary>
    public const string IbmDmf = "ibm.dmf";
    /// <summary>Image IBM PC de 2 880 Kio.</summary>
    public const string Ibm2880 = "ibm.2880";
    /// <summary>Image IBM PC dont la géométrie doit être déterminée par analyse.</summary>
    public const string IbmScan = "ibm.scan";

    /// <summary>Construit l'identifiant IBM PC en tronquant au kibioctet inférieur une capacité non alignée.</summary>
    /// <param name="capacityBytes">Capacité positive ou nulle de l'image, en octets.</param>
    /// <returns>Identifiant IBM PC contenant la capacité entière en kibioctets.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La capacité est négative.</exception>
    public static string IbmFromCapacity(long capacityBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacityBytes);
        return $"{IbmPrefix}{capacityBytes / DataSizeConstants.BytesPerKibibyte}";
    }
}
