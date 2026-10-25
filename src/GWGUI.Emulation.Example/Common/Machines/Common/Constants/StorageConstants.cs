namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;

/// <summary>Choix d’images et conventions des paramètres persistants des lecteurs fictifs.</summary>
/// <remarks>Les capacités décrivent les images proposées par le module ; le registre commun complète
/// les contraintes des conteneurs. Les préfixes identifient séparément les paramètres de chaque lecteur.</remarks>
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
    /// <summary>Préfixe du modèle de lecteur, complété par l’identité textuelle de son emplacement.</summary>
    internal const string StorageModel = "storage.model.";
    /// <summary>Préfixe de la vitesse du lecteur, en pourcentage ; zéro représente la vitesse maximale.</summary>
    internal const string StorageSpeed = "storage.speed.";
    /// <summary>Préfixe du booléen de protection en écriture du lecteur.</summary>
    internal const string StorageWriteProtected = "storage.writeProtected.";
    /// <summary>Préfixe du booléen de redirection des écritures vers une copie de travail.</summary>
    internal const string StorageRedirectWrites = "storage.redirectWrites.";
    /// <summary>Préfixe de l’interface de disque sélectionnée, indépendante du chemin de l’image.</summary>
    internal const string StorageInterface = "storage.interface.";
    /// <summary>Préfixe du booléen illustrant une option propre à un emplacement de cartouche.</summary>
    internal const string CartridgeOptionPrefix = "storage.cartridgeOption.";
    /// <summary>Préfixe du chemin de carte mémoire, complété par l’indice du lecteur.</summary>
    internal const string MemoryCardPathOptionPrefix = "storage.memoryCardPath";
    /// <summary>Préfixe des options supplémentaires : emplacement, point, puis clé native inchangée.</summary>
    internal const string DeviceOptionsPrefix = "storage.options.";
    /// <summary>Identité du modèle de lecteur générique ; sa présentation utilise les ressources communes.</summary>
    internal const string FloppyModel = nameof(EmulationMediaType.Floppy);
    /// <summary>Vitesse initiale du lecteur, en pourcentage du rythme de base.</summary>
    internal const string DefaultFloppySpeed = "100";
    /// <summary>Vitesses prises en charge par le dialogue commun : pourcentages et zéro pour le maximum.</summary>
    internal static IReadOnlyList<string> FloppySpeeds { get; } = Array.AsReadOnly<string>([DefaultFloppySpeed, "200", "400", "800", "0"]);
    /// <summary>Capacité fixe de l’image de carte mémoire simulée : 128 Kio, exprimés en octets.</summary>
    internal const int MemoryCardSize = 128 * 1024;
    /// <summary>Identité technique de l’interface de carte mémoire utilisée dans les descriptions d’images.</summary>
    internal const string MemoryCardModel = nameof(EmulationMediaType.MemoryCard);
}
