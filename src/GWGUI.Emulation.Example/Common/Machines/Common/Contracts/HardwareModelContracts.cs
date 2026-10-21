namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Description des capacités matérielles d’un modèle, partagée par les catalogues et la validation.</summary>
/// <param name="Model">Identité typée du modèle dans le module.</param>
/// <param name="StableModelId">Identifiant technique stable exposé dans les descriptions de machines.</param>
/// <param name="DisplayNameResourceKey">Clé de ressource du nom de modèle présenté à l’utilisateur.</param>
/// <param name="Emulator">Backend choisi par défaut pour ce modèle.</param>
/// <param name="Cpus">Processeurs proposés ; le premier est le processeur par défaut.</param>
/// <param name="DefaultCpuFrequencyHz">Fréquence d’origine du processeur en hertz.</param>
/// <param name="MainMemoryBytes">Capacité de mémoire principale en octets.</param>
/// <param name="Regions">Régions matérielles proposées ; la première est la région par défaut.</param>
/// <param name="Video">Capacités vidéo présentes dans le modèle.</param>
/// <param name="Audio">Capacités audio présentes dans le modèle.</param>
/// <param name="Storage">Types de stockage disponibles sur la machine.</param>
/// <param name="Ports">Catégories de ports et quantités disponibles pour chacune.</param>
/// <param name="Firmware">Catégories de firmware admises par le modèle.</param>
/// <param name="Media">Catégories de médias admises par le modèle.</param>
/// <remarks>Les collections reçues ne sont pas copiées par le record et doivent rester stables.
/// L’indexation du catalogue exige une identité de modèle unique. Les listes de processeurs et de régions
/// doivent contenir un premier élément pour les propriétés de choix par défaut.</remarks>
public sealed record HardwareModelDefinition(MachineModel Model, string StableModelId, string DisplayNameResourceKey, Emulator Emulator,
    IReadOnlyList<HardwareCpu> Cpus, long DefaultCpuFrequencyHz, long MainMemoryBytes, IReadOnlyList<HardwareRegion> Regions,
    IReadOnlyList<HardwareVideoCapability> Video, IReadOnlyList<HardwareAudioCapability> Audio, IReadOnlyList<HardwareStorageCapability> Storage,
    IReadOnlyList<HardwarePortDefinition> Ports, IReadOnlyList<FirmwareCategory> Firmware, IReadOnlyList<MediaCategory> Media)
{
    /// <summary>Premier processeur de la liste, utilisé comme choix initial du modèle.</summary>
    /// <exception cref="InvalidOperationException">La liste de processeurs est vide.</exception>
    public HardwareCpu DefaultCpu => Cpus.First();
    /// <summary>Première région de la liste, utilisée comme région initiale du modèle.</summary>
    /// <exception cref="InvalidOperationException">La liste de régions est vide.</exception>
    public HardwareRegion DefaultRegion => Regions.First();
}

/// <summary>Capacité d’un ensemble de ports matériels du modèle.</summary>
/// <param name="Capability">Catégorie de connexion ou de périphérique représentée.</param>
/// <param name="Count">Nombre de ports de cette catégorie, et non indice du dernier port.</param>
public sealed record HardwarePortDefinition(HardwarePortCapability Capability, int Count);

