namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Disponibilité d’un contenu lors de la préparation de la configuration.</summary>
public enum MediaAvailability
{
    /// <summary>Le contenu demandé est accessible.</summary>
    Available,
    /// <summary>Le contenu demandé n’est pas accessible.</summary>
    Unavailable,
}

/// <summary>Catégories de contenus représentées par Common ; chaque profil déclare celles que ses lecteurs acceptent.</summary>
public enum MediaCategory
{
    /// <summary>Image de disquette montée dans un lecteur.</summary>
    Floppy,
    /// <summary>Image de cassette utilisée par le transport de bande.</summary>
    Cassette,
    /// <summary>Image de disque dur.</summary>
    HardDisk,
    /// <summary>Image de cartouche.</summary>
    Cartridge,
    /// <summary>Image ou répertoire de carte mémoire.</summary>
    MemoryCard,
    /// <summary>Image de disque optique destinée à un profil possédant un lecteur compatible.</summary>
    CompactDisc,
    /// <summary>Répertoire présenté comme un volume par une interface de stockage de dossiers.</summary>
    Directory,
    /// <summary>Contenu initial regroupant un instantané de machine, distinct du format des états natifs sauvegardés.</summary>
    Snapshot,
}

/// <summary>Deux variantes de format de cartouche que le même profil peut déclarer.</summary>
/// <remarks>Ces identifiants sont fictifs. Ils décrivent le format du contenu, tandis que MachineModel
/// identifie la machine et Slot son connecteur. Un adaptateur réel conserve ici ses formats reconnus.</remarks>
public enum CartridgePlatform
{
    /// <summary>Première variante de format de cartouche du profil.</summary>
    Standard = 0,
    /// <summary>Seconde variante, distinguée avant la préparation du contenu par le backend.</summary>
    Alternate = 1
}