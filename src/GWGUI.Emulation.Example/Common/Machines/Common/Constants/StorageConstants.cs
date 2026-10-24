namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;

/// <summary>Choix de capacité et de présentation des images de disque dur du profil fictif.</summary>
/// <remarks>Ces valeurs décrivent les images proposées par le module. Le registre commun complète ensuite
/// leurs capacités avec les contraintes réelles du conteneur choisi.</remarks>
internal static class StorageSettingsFunctionsConstants
{
    /// <summary>Taille d'un secteur logique du profil, exprimée en octets.</summary>
    internal const int SectorBytes = 512;
    /// <summary>Capacité initialement proposée, exprimée en octets : 32 Mio.</summary>
    internal const long DefaultBytes = 32L * 1024 * 1024;
    /// <summary>Capacité maximale choisie pour ce profil, alignée sur un secteur et représentable sur 32 bits.</summary>
    /// <remarks>uint.MaxValue est la borne de System.UInt32. La division puis multiplication conserve
    /// le plus grand nombre entier de secteurs contenu dans cette borne ; le résultat est exprimé en octets.</remarks>
    internal const long MaximumBytes = uint.MaxValue / SectorBytes * (long)SectorBytes;
    /// <summary>Nom technique de l'interface présentée pour les images de ce profil.</summary>
    internal const string InterfaceName = "IDE";
    /// <summary>Préfixe des identifiants de formats, complété par l'extension sans son point initial.</summary>
    internal const string FormatIdPrefix = "example-";
}
