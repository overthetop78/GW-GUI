namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Circuits audio pouvant être déclarés par une définition matérielle.</summary>
public enum HardwareAudioCapability
{
    /// <summary>Générateur de son SN76489.</summary>
    Sn76489
}

/// <summary>Composants vidéo décrits par les profils de machines.</summary>
public enum HardwareVideoCapability
{
    /// <summary>Contrôleur d’affichage MC6845.</summary>
    Mc6845,
    /// <summary>Circuit de génération vidéo ULA.</summary>
    VideoUla,
    /// <summary>Générateur de caractères de télétexte SAA5050.</summary>
    Saa5050
}

/// <summary>Identités de processeurs décrites dans les capacités matérielles.</summary>
/// <remarks>La fréquence est fournie séparément en hertz dans HardwareModelDefinition.</remarks>
public enum HardwareCpu
{
    /// <summary>Processeur MOS 6502A.</summary>
    Mos6502A,
    /// <summary>Processeur Rockwell 65C12.</summary>
    Rockwell65C12,
    /// <summary>Processeur Rockwell 6512.</summary>
    Rockwell6512
}

/// <summary>Régions ou standards matériels proposés par les profils de machines.</summary>
public enum HardwareRegion
{
    /// <summary>Profil matériel utilisant le standard vidéo PAL.</summary>
    Pal
}

/// <summary>Catégories de ports matériels décrites dans HardwarePortDefinition.</summary>
public enum HardwarePortCapability
{
    /// <summary>Connexion du clavier de la machine.</summary>
    Keyboard,
    /// <summary>Connexion de joystick à axes analogiques.</summary>
    AnalogJoystick
}

/// <summary>Types de stockage que la définition matérielle rend disponibles.</summary>
public enum HardwareStorageCapability
{
    /// <summary>Stockage sur disquette.</summary>
    Floppy,
    /// <summary>Stockage sur cassette.</summary>
    Cassette,
    /// <summary>Contenu fourni sous forme de cartouche.</summary>
    Cartridge
}
