namespace GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Dictionaries;

/// <summary>Décrit cinq profils matériels d’exemple appartenant au même groupe de machines.</summary>
/// <remarks>Chaque définition réunit identité, processeur, mémoire, ports, stockage, firmware et médias acceptés.
/// Le catalogue Machines/Common agrège ces définitions pour les exposer aux services du module.
/// Ces données décrivent le matériel ; elles ne lancent aucun cœur et ne montent aucun média.</remarks>
public static class ExampleMachineAModelCatalog
{
    /// <summary>Standard vidéo PAL commun aux cinq profils.</summary>
    private static readonly IReadOnlyList<HardwareRegion> Regions = HardwareModelFunctions.Values(HardwareRegion.Pal);
    /// <summary>Composants vidéo décrits pour ces profils : contrôleur d’affichage, circuit pixels et générateur télétexte.</summary>
    private static readonly IReadOnlyList<HardwareVideoCapability> Video = HardwareModelFunctions.Values(HardwareVideoCapability.Mc6845, HardwareVideoCapability.VideoUla, HardwareVideoCapability.Saa5050);
    /// <summary>Circuit sonore commun aux profils.</summary>
    private static readonly IReadOnlyList<HardwareAudioCapability> Audio = HardwareModelFunctions.Values(HardwareAudioCapability.Sn76489);
    /// <summary>Un port clavier pour le profil A, dépourvu de ports joystick dans cette description.</summary>
    private static readonly IReadOnlyList<HardwarePortDefinition> KeyboardPorts = HardwareModelFunctions.Values(new HardwarePortDefinition(HardwarePortCapability.Keyboard, ExampleMachineAModelConstants.OnePort));
    /// <summary>Un port clavier et deux ports joystick analogiques pour les profils B, C, D et E.</summary>
    private static readonly IReadOnlyList<HardwarePortDefinition> ComputerPorts = HardwareModelFunctions.Values(new HardwarePortDefinition(HardwarePortCapability.Keyboard, ExampleMachineAModelConstants.OnePort), new HardwarePortDefinition(HardwarePortCapability.AnalogJoystick, ExampleMachineAModelConstants.TwoPorts));

    /// <summary>Définitions des profils, conservées dans l’ordre A, B, D, E, C du catalogue.</summary>
    /// <remarks>A illustre une machine à cassette et 16 Kio ; B ajoute les disquettes et les joysticks avec 32 Kio.
    /// D et E présentent un processeur 6512 avec respectivement 64 et 128 Kio.
    /// C présente un processeur 65C12 avec 128 Kio, un firmware distinct et le support des cartouches.
    /// Les listes de firmware indiquent les catégories matérielles ; les règles de fourniture des fichiers restent celles de la configuration et de l’adaptateur.</remarks>
    public static IReadOnlyList<HardwareModelDefinition> All { get; } = HardwareModelFunctions.Values(
        Create(MachineModel.ExampleMachineA, ExampleMachineAModelConstants.ModelAId,
            ExampleMachineAModelConstants.ModelADisplayNameResource, ExampleMachineAModelConstants.SixteenKibibytes,
            HardwareCpu.Mos6502A, KeyboardPorts,
            HardwareModelFunctions.Values(HardwareStorageCapability.Cassette),
            HardwareModelFunctions.Values(FirmwareCategory.OperatingSystem, FirmwareCategory.Basic),
            HardwareModelFunctions.Values(MediaCategory.Cassette)),
        Create(MachineModel.ExampleMachineB, ExampleMachineAModelConstants.ModelBId,
            ExampleMachineAModelConstants.ModelBDisplayNameResource, ExampleMachineAModelConstants.ThirtyTwoKibibytes,
            HardwareCpu.Mos6502A, ComputerPorts,
            HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette),
            HardwareModelFunctions.Values(FirmwareCategory.OperatingSystem, FirmwareCategory.Basic, FirmwareCategory.DiscFilingSystem),
            HardwareModelFunctions.Values(MediaCategory.Floppy, MediaCategory.Cassette)),
        Create(MachineModel.ExampleMachineD, ExampleMachineAModelConstants.ModelDId,
            ExampleMachineAModelConstants.ModelDDisplayNameResource, ExampleMachineAModelConstants.SixtyFourKibibytes,
            HardwareCpu.Rockwell6512, ComputerPorts,
            HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette),
            HardwareModelFunctions.Values(FirmwareCategory.OperatingSystem, FirmwareCategory.Basic, FirmwareCategory.DiscFilingSystem),
            HardwareModelFunctions.Values(MediaCategory.Floppy, MediaCategory.Cassette)),
        Create(MachineModel.ExampleMachineE, ExampleMachineAModelConstants.ModelEId,
            ExampleMachineAModelConstants.ModelEDisplayNameResource, ExampleMachineAModelConstants.OneHundredTwentyEightKibibytes,
            HardwareCpu.Rockwell6512, ComputerPorts,
            HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette),
            HardwareModelFunctions.Values(FirmwareCategory.OperatingSystem, FirmwareCategory.Basic, FirmwareCategory.DiscFilingSystem),
            HardwareModelFunctions.Values(MediaCategory.Floppy, MediaCategory.Cassette)),
        Create(MachineModel.ExampleMachineC, ExampleMachineAModelConstants.ModelCId,
            ExampleMachineAModelConstants.ModelCDisplayNameResource, ExampleMachineAModelConstants.OneHundredTwentyEightKibibytes,
            HardwareCpu.Rockwell65C12, ComputerPorts,
            HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette, HardwareStorageCapability.Cartridge),
            HardwareModelFunctions.Values(FirmwareCategory.MasterMos, FirmwareCategory.Basic, FirmwareCategory.DiscFilingSystem),
            HardwareModelFunctions.Values(MediaCategory.Floppy, MediaCategory.Cassette, MediaCategory.Cartridge)));

    /// <summary>Assemble une définition avec les caractéristiques communes du groupe et les différences du profil.</summary>
    /// <param name="model">Valeur typée identifiant le profil.</param>
    /// <param name="modelId">Identifiant textuel stable correspondant au modèle.</param>
    /// <param name="resourceKey">Clé de traduction de son nom affiché.</param>
    /// <param name="memoryBytes">Capacité de mémoire principale, en octets.</param>
    /// <param name="cpu">Processeur unique proposé pour ce profil.</param>
    /// <param name="ports">Types et nombres de ports d’entrée.</param>
    /// <param name="storage">Capacités matérielles de stockage.</param>
    /// <param name="firmware">Catégories de firmware décrivant le matériel.</param>
    /// <param name="media">Catégories de médias prises en charge par le profil.</param>
    /// <returns>Définition associée au cœur du catalogue, à la fréquence commune et aux circuits audio et vidéo communs.</returns>
    private static HardwareModelDefinition Create(MachineModel model, string modelId, string resourceKey, long memoryBytes, HardwareCpu cpu,
        IReadOnlyList<HardwarePortDefinition> ports, IReadOnlyList<HardwareStorageCapability> storage, IReadOnlyList<FirmwareCategory> firmware, IReadOnlyList<MediaCategory> media) =>
        HardwareModelFunctions.Create(model, modelId, resourceKey, Emulator.B2, ExampleMachineAModelConstants.CpuFrequencyHz, memoryBytes, HardwareModelFunctions.Values(cpu), Regions, Video, Audio, storage, ports, firmware, media);
}
