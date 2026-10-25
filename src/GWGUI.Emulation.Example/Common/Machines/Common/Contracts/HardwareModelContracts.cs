namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Description matérielle commune utilisée pour présenter et valider un profil fictif.</summary>
/// <param name="Model">Identité typée du profil.</param>
/// <param name="StableModelId">Identifiant technique utilisé par les interfaces du module.</param>
/// <param name="DisplayNameResourceKey">Clé du nom présenté par l’hôte.</param>
/// <param name="Emulator">Préférence initiale du profil ; les trois émulateurs restent compatibles.</param>
/// <param name="Cpus">Processeurs proposés dans les réglages.</param>
/// <param name="DefaultCpuFrequencyHz">Fréquence de démonstration exprimée en hertz.</param>
/// <param name="MainMemoryBytes">Capacité de mémoire principale exprimée en octets.</param>
/// <param name="Regions">Régions prises en charge.</param>
/// <param name="Video">Capacités de production vidéo.</param>
/// <param name="Audio">Capacités de production audio.</param>
/// <param name="Storage">Interfaces de lecteurs disponibles.</param>
/// <param name="Ports">Ports et quantités disponibles sur le profil.</param>
/// <param name="Firmware">Rôles de firmware décrits pour le profil.</param>
/// <param name="Media">Types de contenus acceptés par les lecteurs.</param>
public sealed record HardwareModelDefinition(MachineModel Model, string StableModelId, string DisplayNameResourceKey, Emulator Emulator, IReadOnlyList<HardwareCpu> Cpus, long DefaultCpuFrequencyHz, long MainMemoryBytes, IReadOnlyList<HardwareRegion> Regions, IReadOnlyList<HardwareVideoCapability> Video, IReadOnlyList<HardwareAudioCapability> Audio, IReadOnlyList<HardwareStorageCapability> Storage, IReadOnlyList<HardwarePortDefinition> Ports, IReadOnlyList<FirmwareCategory> Firmware, IReadOnlyList<MediaCategory> Media)
{
    /// <summary>Premier processeur proposé par le profil matériel.</summary>
    public HardwareCpu DefaultCpu => Cpus.First();
    /// <summary>Première région proposée par le profil matériel.</summary>
    public HardwareRegion DefaultRegion => Regions.First();
}

/// <summary>Quantité de ports présentant la même capacité matérielle.</summary>
/// <param name="Capability">Capacité du port émulé.</param>
/// <param name="Count">Nombre de ports de cette catégorie, distinct du dernier indice.</param>
public sealed record HardwarePortDefinition(HardwarePortCapability Capability, int Count);
