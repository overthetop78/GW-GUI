namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Processeur fictif décrit par les profils matériels.</summary>
public enum HardwareCpu
{
    /// <summary>Processeur simulé dont la fréquence décrit un réglage de démonstration.</summary>
    ExampleCpu,
}

/// <summary>Capacités audio exposées par les profils matériels.</summary>
public enum HardwareAudioCapability
{
    /// <summary>Production de trames PCM entrelacées pour la sortie audio commune.</summary>
    Pcm,
}

/// <summary>Capacités vidéo exposées par les profils matériels.</summary>
public enum HardwareVideoCapability
{
    /// <summary>Production d’images de pixels par le cœur simulé.</summary>
    Raster,
}

/// <summary>Choix de région de démonstration indépendants des marques réelles.</summary>
public enum HardwareRegion
{
    /// <summary>Cadence de base correspondant au standard PAL.</summary>
    Pal,
    /// <summary>Cadence correspondant au standard NTSC.</summary>
    Ntsc,
}

/// <summary>Types de ports matériels présentés par les deux machines.</summary>
public enum HardwarePortCapability
{
    /// <summary>Port de clavier.</summary>
    Keyboard,
    /// <summary>Port de pointeur.</summary>
    Mouse,
    /// <summary>Port acceptant les périphériques de jeu configurables.</summary>
    AnalogJoystick,
}

/// <summary>Types de lecteurs déclarés dans les profils matériels.</summary>
public enum HardwareStorageCapability
{
    /// <summary>Lecteur de disquette.</summary>
    Floppy,
    /// <summary>Lecteur de cassette.</summary>
    Cassette,
    /// <summary>Interface de disque dur.</summary>
    HardDisk,
    /// <summary>Emplacement de cartouche.</summary>
    Cartridge,
    /// <summary>Emplacement de carte mémoire.</summary>
    MemoryCard,
}
