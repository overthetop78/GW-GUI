namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Interfaces de stockage déjà représentées par les descriptions de montage communes.</summary>
/// <remarks>Le bus précise comment le backend présente le contenu. Slot reste l’identité du lecteur ;
/// MountPoint désigne son point de montage et MountOrder l’ordre d’affectation de ses contenus.</remarks>
public enum StorageBus
{
    /// <summary>Interface ACSI utilisée pour présenter une image de disque comme périphérique de stockage.</summary>
    Acsi = 0,
    /// <summary>Interface IDE utilisée pour présenter une image de disque.</summary>
    Ide = 1,
    /// <summary>Interface GEMDOS présentant un dossier hôte comme volume de la machine.</summary>
    Gemdos = 2
}