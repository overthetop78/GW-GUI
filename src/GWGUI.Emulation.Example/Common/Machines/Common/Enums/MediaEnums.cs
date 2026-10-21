namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Disponibilité d’un contenu lors de la préparation de la configuration.</summary>
public enum MediaAvailability
{
    /// <summary>Le contenu demandé est accessible.</summary>
    Available,
    /// <summary>Le contenu demandé n’est pas accessible.</summary>
    Unavailable,
}

/// <summary>Types de contenus montables par les deux machines fictives.</summary>
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
}
