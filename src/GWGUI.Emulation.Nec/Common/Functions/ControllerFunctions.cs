using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Nec.Common.Functions;

/// <summary>Relais des entrées du module vers les contrats publics du SDK.</summary>
/// <remarks>Les adaptations de cœur passent par ce propriétaire Common ; les algorithmes restent dans le SDK.</remarks>
internal static class ControllerFunctions
{
    /// <summary>Service public de consultation des descriptions et de traitement des entrées, emprunté au SDK.</summary>
    /// <remarks>Le service ne possède aucune machine ni aucun périphérique et ne conserve pas les descriptions reçues.</remarks>
    internal static IEmulationInputProcessing InputProcessing => EmulationSdk.InputProcessing;

    /// <summary>Lit le nombre de ports matériels avant les extensions propres au cœur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée dont le modèle détermine les ports de base.</param>
    /// <returns>Nombre matériel utilisé sans adaptateur ou comme comportement par défaut de son contrat.</returns>
    internal static int ModelPortCount(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).ControllerPortCount;

    /// <summary>Résout les ports de session par le contrat Common du cœur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée, y compris les options de multitap ou d’extension.</param>
    /// <returns>Nombre annoncé par l’adaptateur ; nombre matériel lorsque la machine ne sélectionne aucun cœur.</returns>
    /// <remarks>Le contrat reçoit la configuration entière. Cette consultation ne crée ni cœur ni ressource native.</remarks>
    internal static int PortCount(MachineConfiguration configuration) => new Engine().SelectedAdapter(configuration)?.GetControllerPortCount(configuration) ?? ModelPortCount(configuration);

    /// <summary>Service public de description, validation et projection des retours, emprunté au SDK sans ressource physique possédée.</summary>
    internal static IEmulationControllerFeedbackProcessing Feedback => EmulationSdk.ControllerFeedback;

    /// <summary>Crée un accumulateur indépendant pour conserver les entrées reçues entre deux lectures du cœur.</summary>
    /// <remarks>Ce relais Common demande l’instance au contrat SDK. Les collections restent empruntées et doivent être stables ; Update additionne les mouvements avec saturation, Consume remet seulement les déplacements et molettes à zéro, et Reset efface toutes les entrées. Le service ne possède aucune ressource native.</remarks>
    /// <returns>Contrat appartenant au consommateur ; chaque appel fournit un état indépendant.</returns>
    internal static IEmulationInputAccumulator CreateInputAccumulator() => EmulationSdk.InputProcessing.CreateInputAccumulator();

    /// <summary>Additionne deux déplacements relatifs en saturant aux limites du type Int32.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static int SaturatingAdd(int left, int right) => EmulationSdk.InputProcessing.SaturatingAdd(left, right);

    /// <summary>Calcule les modificateurs clavier par le contrat commun des entrées.</summary>
    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => EmulationSdk.InputProcessing.Modifiers(keys);

    /// <summary>Calcule le caractère natif par le contrat commun des entrées.</summary>
    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys) => EmulationSdk.InputProcessing.Character(code, keys);

    /// <summary>Publie les transitions sans interrompre les références réentrantes de l’état et du callback.</summary>
    internal static void PublishTransitions(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<EmulationKey, uint> keyMap, ushort modifiers,
        ref IReadOnlySet<EmulationKey> previous, ref ExternalCoreApi.KeyboardEvent? keyboardEvent,
        Func<EmulationKey, bool> isModifier, Func<uint, IReadOnlySet<EmulationKey>, uint> characterFor)
        => EmulationSdk.InputProcessing.PublishTransitions(keys, keyMap, modifiers, ref previous, ref keyboardEvent, isModifier, characterFor);


    /// <summary>Fournit la table clavier native par le contrat commun des entrées.</summary>
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateNativeKeyMap() => EmulationSdk.InputProcessing.CreateKeyMap();

    /// <summary>Convertit la catégorie publique vers un choix réellement accepté sur le port du modèle courant.</summary>
    /// <param name="peripheral">Catégorie demandée par le contrat public des entrées.</param>
    /// <param name="configuration">Configuration empruntée pour consulter le modèle et les choix de son adaptation.</param>
    /// <param name="port">Indice matériel du port, à partir de zéro.</param>
    /// <returns>Type interne existant ; Automatic reprend le périphérique par défaut du modèle.</returns>
    /// <remarks>Les descriptions restent dans leur catalogue existant. Une catégorie incompatible est rejetée
    /// avant tout appel au cœur ; cette conversion ne crée aucun périphérique ni ressource native.</remarks>
    internal static PeripheralCategory FromCommon(EmulationPeripheralCategory peripheral, MachineConfiguration configuration, int port)
    {
        if (port < 0 || port >= PortCount(configuration)) throw new ArgumentOutOfRangeException(nameof(port));
        var model = ModelCatalog.Get(configuration.Model);
        var supported = InputProcessing.GetPeripherals<PeripheralCategory>(InputSettingsFunctions.Describe(configuration), port);
        var selected = peripheral switch
        {
            EmulationPeripheralCategory.Automatic => ControllerCatalog.Default(model),
            EmulationPeripheralCategory.Mouse => supported.FirstOrDefault(ControllerCatalog.IsMouse, PeripheralCategory.Automatic),
            EmulationPeripheralCategory.RetroPad or EmulationPeripheralCategory.Joystick => Enum.TryParse<PeripheralCategory>(peripheral.ToString(), out var named) && supported.Contains(named) ? named : ControllerCatalog.Default(model),
            _ => Enum.TryParse<PeripheralCategory>(peripheral.ToString(), out var named) && Enum.IsDefined(named) ? named : PeripheralCategory.Automatic
        };
        if (!Enum.IsDefined(peripheral) || !supported.Contains(selected)) throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null);
        return selected;
    }

    /// <summary>Remplace le type du port demandé en conservant ses associations et les autres réglages de session.</summary>
    /// <param name="configuration">Configuration source, laissée immuable.</param>
    /// <param name="port">Indice du port à modifier.</param>
    /// <param name="type">Choix interne accepté par la description du port.</param>
    /// <returns>Nouvelle configuration à publier après la réussite de la commande native.</returns>
    /// <remarks>Le périphérique physique, les boutons, les visuels et les sorties du port sont conservés.
    /// Lorsque la liste explicite des contrôleurs est présente, sa sélection est actualisée également :
    /// elle ne doit pas rétablir un ancien type lors du prochain démarrage du cœur.</remarks>
    internal static MachineConfiguration WithControllerType(MachineConfiguration configuration, int port, PeripheralCategory type)
    {
        if (port < 0 || port >= PortCount(configuration)) throw new ArgumentOutOfRangeException(nameof(port));
        if (!InputProcessing.GetPeripherals<PeripheralCategory>(InputSettingsFunctions.Describe(configuration), port).Contains(type)) throw new ArgumentOutOfRangeException(nameof(type), type, null);
        var input = configuration.Input ?? new InputConfiguration();
        var bindings = input.Controllers ?? [];
        var binding = bindings.FirstOrDefault(item => item.Port == port) ?? new ControllerBinding(port, type);
        var controllers = configuration.Controllers is null ? null : Enumerable.Range(0, Math.Max(configuration.Controllers.Count, port + 1))
            .Select(index => index == port ? type : index < configuration.Controllers.Count ? configuration.Controllers[index] : bindings.FirstOrDefault(item => item.Port == index)?.Peripheral ?? ControllerCatalog.Default(ModelCatalog.Get(configuration.Model))).ToArray();
        return configuration with
        {
            Controllers = controllers,
            Input = input with { Controllers = bindings.Where(item => item.Port != port).Append(binding with { Peripheral = type }).OrderBy(item => item.Port).ToArray() }
        };
    }
}
