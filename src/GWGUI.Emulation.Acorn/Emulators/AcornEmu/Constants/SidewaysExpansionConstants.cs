namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Montages documentés des quatre supports ROM BBC et des modules BooBip.</summary>
internal static class SidewaysExpansionConstants
{
    /// <summary>Choix persistant du montage de mémoire sideways.</summary>
    internal const string Option = "acornemu.memory.sideways";
    /// <summary>Absence de module ajouté aux supports d'origine.</summary>
    internal const string None = "none";
    /// <summary>Préfixe des montages RAM/ROM, suivi du nombre de modules.</summary>
    internal const string RamRomPrefix = "boobip-ramrom-";
    /// <summary>Préfixe des montages EPROM, suivi du nombre de modules.</summary>
    internal const string EpromPrefix = "boobip-eprom-";
    /// <summary>Préfixe des chemins explicites des puces physiques.</summary>
    internal const string ChipPrefix = "acornemu.memory.sideways.chip.";
    /// <summary>Nombre de supports ROM physiques du BBC.</summary>
    internal const int SocketCount = 4;
    /// <summary>Premier groupe de banques ROM des modules RAM/ROM.</summary>
    internal const int RamRomFirstRomBank = 8;
    /// <summary>Première banque des supports d'origine.</summary>
    internal const int NativeFirstBank = 12;
    /// <summary>Nombre de banques RAM de chaque module RAM/ROM.</summary>
    internal const int RamBanksPerModule = 2;
    /// <summary>Capacité physique de chaque module RAM/ROM.</summary>
    internal const int RamBytesPerModule = 32 * 1024;
    /// <summary>Capacité d'une banque sideways.</summary>
    internal const int BankBytes = 16 * 1024;
    /// <summary>Plus petite image acceptée dans un module RAM/ROM.</summary>
    internal const int MinimumRamRomImageBytes = 8 * 1024;
    /// <summary>Capacité maximale d'une puce EPROM de l'adaptateur.</summary>
    internal const int EpromBytes = 64 * 1024;
    /// <summary>Masque des commandes logicielles de protection.</summary>
    internal const byte ProtectCommandMask = 0xf0;
    /// <summary>Commande de protection d'une banque RAM sélectionnée.</summary>
    internal const byte ProtectCommand = 0x20;
    /// <summary>Commande retirant la protection d'une banque RAM sélectionnée.</summary>
    internal const byte UnprotectCommand = 0x30;
    /// <summary>Titre du bloc RAM sideways.</summary>
    internal const string Group = "Emulation.Acorn.Memory.Sideways";
    /// <summary>Titre du choix de matériel dans Extensions.</summary>
    internal const string ExtensionGroup = "Emulation.Acorn.Memory.Sideways.Extension";
    /// <summary>Capacité informative de la RAM installée par l'extension.</summary>
    internal const string RamCapacity = "Emulation.Acorn.Memory.Sideways.Capacity";
    /// <summary>Libellé du choix de matériel.</summary>
    internal const string CardLabel = "Emulation.Acorn.Memory.Sideways.Card";
    /// <summary>Explication de la mémoire bankée et des images de puces.</summary>
    internal const string Help = "Emulation.Acorn.Memory.Sideways.Help";
    /// <summary>Titre des supports de puces dans l'onglet ROM.</summary>
    internal const string RomGroup = "Emulation.Acorn.Memory.Sideways.Sockets";
    /// <summary>Nom invariant du module RAM/ROM.</summary>
    internal const string RamRomName = "BooBip 32K RAM / 32K ROM";
    /// <summary>Nom invariant de l'adaptateur EPROM.</summary>
    internal const string EpromName = "BooBip 64K EPROM";
    /// <summary>Format invariant d'un montage de plusieurs modules.</summary>
    internal const string AssemblyDisplayFormat = "{0} × {1}";
    /// <summary>Diagnostic d'une image de puce de taille incompatible.</summary>
    internal const string InvalidImageSize = "Emulation.Acorn.Memory.Sideways.InvalidImageSize";
    /// <summary>Capacités des images RAM/ROM, dans la notation technique commune.</summary>
    internal const string RamRomImageSizes = "8 / 16 / 32 KiB";
    /// <summary>Capacités des images EPROM, dans la notation technique commune.</summary>
    internal const string EpromImageSizes = "32 / 64 KiB";
    /// <summary>Préfixe des puces de la carte B12, indépendant des chemins des modules BooBip.</summary>
    internal const string AriesChipPrefix = "acornemu.memory.sideways.aries.chip.";
    /// <summary>Montage B12 sans puce SRAM.</summary>
    internal const string AriesWithoutRam = "aries-b12-none";
    /// <summary>Montage B12 avec la moitié basse de la banque RAM.</summary>
    internal const string Aries8 = "aries-b12-8k";
    /// <summary>Montage B12 avec les deux puces SRAM.</summary>
    internal const string Aries16 = "aries-b12-16k";
    /// <summary>Nombre de supports ROM de la B12.</summary>
    internal const int AriesRomSockets = 12;
    /// <summary>Décalage des quatre ROM reprises sur les supports 9 à 12.</summary>
    internal const int AriesNativeBankOffset = 3;
    /// <summary>Capacité d'une puce SRAM B12.</summary>
    internal const int AriesHalfRamBytes = 8 * 1024;
    /// <summary>Capacités des images de ROM B12.</summary>
    internal const string AriesImageSizes = "8 / 16 KiB";
    /// <summary>Présentation technique d'un montage B12.</summary>
    internal const string AriesDisplayFormat = "Aries B12 · {0} KiB RAM";
    /// <summary>Montages physiques disponibles pour la banque RAM 0.</summary>
    internal static IReadOnlyDictionary<string, int> AriesModels { get; } = new Dictionary<string, int>
    {
        [AriesWithoutRam] = 0,
        [Aries8] = AriesHalfRamBytes,
        [Aries16] = BankBytes
    };
    /// <summary>Noms invariants des douze supports ROM physiques B12.</summary>
    internal static IReadOnlyList<string> AriesSocketLabels { get; } = Array.AsReadOnly(Enumerable.Range(1, AriesRomSockets).Select(bank => "Emulation.Acorn.Memory.Socket.Bank." + bank.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray());
    /// <summary>Clés communes des noms des quatre supports matériels.</summary>
    internal static IReadOnlyList<string> SocketLabels { get; } = Array.AsReadOnly<string>(["Emulation.Acorn.Memory.Socket.IC52", "Emulation.Acorn.Memory.Socket.IC58", "Emulation.Acorn.Memory.Socket.IC100", "Emulation.Acorn.Memory.Socket.IC101"]);
}
