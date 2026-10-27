namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Identifiants persistants et décodage des cartes mémoire du Model A.</summary>
internal static class MemoryExtensionConstants
{
    /// <summary>Choix de la carte Shadow RAM installée.</summary>
    internal const string CardOption = "acornemu.memory.shadow";
    /// <summary>Chemin de la ROM de gestion ; conservé lorsque la carte est retirée.</summary>
    internal const string Firmware = "acornemu.memory.shadow.rom";
    /// <summary>Absence d'extension mémoire.</summary>
    internal const string None = "none";
    /// <summary>Carte Aries B20 de 20 Kio.</summary>
    internal const string AriesB20 = "aries-b20";
    /// <summary>Carte Watford de 32 Kio.</summary>
    internal const string Watford32 = "watford-32k";
    /// <summary>Capacité de la carte Aries B20.</summary>
    internal const int AriesBytes = 20 * 1024;
    /// <summary>Capacité de la carte Watford.</summary>
    internal const int WatfordBytes = 32 * 1024;
    /// <summary>Début de la zone haute commutable avec la mémoire écran.</summary>
    internal const ushort ShadowStart = 0x3000;
    /// <summary>Fin exclusive de la RAM principale BBC.</summary>
    internal const ushort RamEnd = 0x8000;
    /// <summary>Écriture sélectionnant toute la RAM Watford.</summary>
    internal const ushort FullRamSelect = 0xfffc;
    /// <summary>Écriture sélectionnant la RAM BBC.</summary>
    internal const ushort MainRamSelect = 0xfffe;
    /// <summary>Écriture sélectionnant la zone haute de la carte.</summary>
    internal const ushort ShadowRamSelect = 0xffff;
    /// <summary>Sélection de la RAM BBC au démarrage.</summary>
    internal const int MainSelection = 2;
    /// <summary>Sélection de la zone haute de la carte.</summary>
    internal const int ShadowSelection = 3;
    /// <summary>Sélection de toute la carte Watford.</summary>
    internal const int FullSelection = 0;
    /// <summary>Sélection sans RAM répondante sur Watford.</summary>
    internal const int UnmappedSelection = 1;
    /// <summary>Valeur d'un accès sans RAM répondante.</summary>
    internal const byte UnmappedValue = 0xff;
    /// <summary>Banque de gestion libre sous les banques NFS, DFS et BASIC.</summary>
    internal const int FirmwareBank = 12;
    /// <summary>Titre du bloc de configuration RAM.</summary>
    internal const string Group = "Emulation.Acorn.Memory.Shadow";
    /// <summary>Libellé du choix de carte.</summary>
    internal const string CardLabel = "Emulation.Acorn.Memory.Card";
    /// <summary>Libellé de l'absence de carte.</summary>
    internal const string NoneLabel = "Emulation.Acorn.Memory.None";
    /// <summary>Nom invariant de la carte Aries.</summary>
    internal const string AriesName = "Aries B20";
    /// <summary>Nom invariant de la carte Watford.</summary>
    internal const string WatfordName = "Watford Electronics 32K RAM";
    /// <summary>Clé commune du nom Aries et de sa capacité.</summary>
    internal const string AriesLabel = "Emulation.Acorn.Memory.AriesB20";
    /// <summary>Clé commune du nom Watford et de sa capacité.</summary>
    internal const string WatfordLabel = "Emulation.Acorn.Memory.Watford32";
    /// <summary>Libellé du fichier ROM de gestion.</summary>
    internal const string FirmwareLabel = "Emulation.Acorn.Memory.Rom";
    /// <summary>Aide expliquant la sélection explicite de la ROM de gestion.</summary>
    internal const string FirmwareHelp = "Emulation.Acorn.Memory.Rom.Help";
    /// <summary>Explication des prérequis et de l'usage de la ROM de gestion.</summary>
    internal const string Help = "Emulation.Acorn.Memory.Help";
    /// <summary>Diagnostic de conflit avec une capacité principale insuffisante.</summary>
    internal const string RequiresMainRam = "Emulation.Acorn.Memory.RequiresMainRam";
    /// <summary>Diagnostic d'une ROM MOS connue trop ancienne.</summary>
    internal const string RequiresMos = "Emulation.Acorn.Memory.RequiresMos";
    /// <summary>Diagnostic de ROM de gestion absente, avec son emplacement dans RAM.</summary>
    internal const string MissingFirmware = "Emulation.Acorn.Memory.RomMissing";
    /// <summary>Version MOS requise par les ROM de gestion retenues.</summary>
    internal const string RequiredMosVersion = "1.20";
}
