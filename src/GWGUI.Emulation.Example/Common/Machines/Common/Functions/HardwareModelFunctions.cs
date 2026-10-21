using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Construction des listes, définitions et index utilisés par les catalogues matériels.</summary>
/// <remarks>Ces fonctions manipulent des données de description et ne démarrent aucun cœur.
/// Les collections et objets fournis restent sous la responsabilité des catalogues qui les construisent.</remarks>
internal static class HardwareModelFunctions
{
    /// <summary>Expose un tableau de valeurs sous une vue en lecture seule.</summary>
    /// <typeparam name="T">Type des éléments de description matérielle.</typeparam>
    /// <param name="values">Valeurs dans leur ordre de déclaration.</param>
    /// <returns>Vue du même tableau, sans copie de ses éléments ni de leurs collections.</returns>
    /// <remarks>Le tableau fourni doit rester stable après la création de la vue.</remarks>
    internal static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);

    /// <summary>Construit l’index des définitions matérielles par identité de modèle.</summary>
    /// <param name="definitions">Liste empruntée des définitions à indexer.</param>
    /// <returns>Nouveau dictionnaire conservant les références des définitions.</returns>
    /// <exception cref="ArgumentException">Plusieurs définitions portent la même identité de modèle.</exception>
    /// <exception cref="InvalidOperationException">Le nombre d’éléments indexés diffère du nombre déclaré par la liste.</exception>
    internal static IReadOnlyDictionary<MachineModel, HardwareModelDefinition> Index(IReadOnlyList<HardwareModelDefinition> definitions)
    {
        var result = definitions.ToDictionary(definition => definition.Model);
        if (result.Count != definitions.Count)
            throw new InvalidOperationException(nameof(definitions));
        return result;
    }

    /// <summary>Assemble une définition matérielle à partir des capacités déclarées par un catalogue spécifique.</summary>
    /// <param name="model">Identité typée du modèle.</param>
    /// <param name="stableModelId">Identifiant technique stable présenté par les descriptions de machines.</param>
    /// <param name="displayNameResourceKey">Clé de ressource du nom du modèle.</param>
    /// <param name="emulator">Identité du backend associée à la définition.</param>
    /// <param name="cpuFrequencyHz">Fréquence d’origine en hertz.</param>
    /// <param name="mainMemoryBytes">Capacité de mémoire principale en octets.</param>
    /// <param name="cpus">Processeurs proposés, dont le premier est le choix initial.</param>
    /// <param name="regions">Régions proposées, dont la première est le choix initial.</param>
    /// <param name="video">Composants et capacités vidéo.</param>
    /// <param name="audio">Composants et capacités audio.</param>
    /// <param name="storage">Types de stockage.</param>
    /// <param name="ports">Catégories et quantités de ports.</param>
    /// <param name="firmware">Catégories de firmware du modèle.</param>
    /// <param name="media">Catégories de médias acceptées.</param>
    /// <returns>Définition conservant les références des collections reçues, sans les modifier.</returns>
    internal static HardwareModelDefinition Create(MachineModel model, string stableModelId, string displayNameResourceKey, Emulator emulator, long cpuFrequencyHz,
        long mainMemoryBytes, IReadOnlyList<HardwareCpu> cpus, IReadOnlyList<HardwareRegion> regions, IReadOnlyList<HardwareVideoCapability> video,
        IReadOnlyList<HardwareAudioCapability> audio, IReadOnlyList<HardwareStorageCapability> storage, IReadOnlyList<HardwarePortDefinition> ports,
        IReadOnlyList<FirmwareCategory> firmware, IReadOnlyList<MediaCategory> media) =>
        new(model, stableModelId, displayNameResourceKey, emulator, cpus, cpuFrequencyHz, mainMemoryBytes, regions, video, audio, storage, ports, firmware, media);
}

