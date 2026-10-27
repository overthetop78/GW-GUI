namespace GWGUI.MediaEngine.Constants;

/// <summary>Profils matériels Winchester Acorn utilisant le format DAT/DSC du contrôleur ACB-4000.</summary>
public static class AcornWinchesterModelConstants
{
    /// <summary>Famille des unités Winchester 110 et 130.</summary>
    public const string Family = "acorn-winchester";
    /// <summary>Identifiant du Winchester 110.</summary>
    public const string Model110 = "acorn-winchester-110";
    /// <summary>Identifiant du Winchester 130.</summary>
    public const string Model130 = "acorn-winchester-130";
    /// <summary>Nombre de cylindres de l'unité de 10 Mo.</summary>
    internal const int Cylinders110 = 306;
    /// <summary>Nombre de têtes de l'unité de 10 Mo.</summary>
    internal const int Heads110 = 4;
    /// <summary>Nombre de cylindres du mécanisme RO203E.</summary>
    internal const int Cylinders130 = 640;
    /// <summary>Nombre de têtes du mécanisme RO203E.</summary>
    internal const int Heads130 = 6;
    /// <summary>Profil compatible pour une géométrie personnalisée DAT/DSC.</summary>
    public const string Custom = "acorn-winchester-custom";
    /// <summary>Adressage des commandes à six octets utilisées par la ROM ADFS.</summary>
    internal const int AddressBits = 21;
    /// <summary>Taille des secteurs Winchester Acorn.</summary>
    internal const int SectorBytes = 256;
    /// <summary>Capacité adressable avec ces commandes, avant limites du mécanisme choisi.</summary>
    public const long MaximumAddressableBytes = (1L << AddressBits) * SectorBytes;
}
