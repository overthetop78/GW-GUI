using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Sony.Common.Functions;

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

    internal static IReadOnlyList<PeripheralCategory> Supported(MachineConfiguration configuration) => WithCompatibleDevices(configuration,
        new Engine().Adapter(configuration) is IEmulatorControllerAdapter adapter ? adapter.GetPeripherals(configuration) : []);
    internal static IReadOnlyList<PeripheralCategory> Supported(MachineConfiguration configuration, int port) => WithCompatibleDevices(configuration,
        new Engine().Adapter(configuration) is IEmulatorControllerAdapter adapter ? adapter.GetPeripherals(configuration, port) : []);

    private static IReadOnlyList<PeripheralCategory> WithCompatibleDevices(MachineConfiguration configuration, IReadOnlyList<PeripheralCategory> types) =>
        configuration.Model is ModelConstants.PlayStation or ModelConstants.PlayStation2 && types.Contains(PeripheralCategory.Joystick)
            ? types.Concat(new[] { PeripheralCategory.DanceMat, PeripheralCategory.ArcadeStick }).Distinct().ToArray() : types;

    internal static PeripheralCategory Selected(MachineConfiguration configuration, int port)
    {
        var binding = configuration.Input?.Controllers?.FirstOrDefault(item => item.Port == port);
        var type = binding?.Peripheral ?? configuration.Controllers?.ElementAtOrDefault(port) ?? PeripheralCategory.Automatic;
        return type == PeripheralCategory.Automatic ? Supported(configuration, port).FirstOrDefault() : type;
    }

    internal static PeripheralCategory Resolve(MachineConfiguration configuration, int port)
    {
        ValidatePort(configuration, port);
        var binding = configuration.Input?.Controllers?.FirstOrDefault(item => item.Port == port);
        var type = binding?.Peripheral ?? configuration.Controllers?.ElementAtOrDefault(port) ?? PeripheralCategory.Automatic;
        if (type == PeripheralCategory.Automatic || type == PeripheralCategory.None && !Supported(configuration, port).Contains(PeripheralCategory.None)) type = Supported(configuration, port).FirstOrDefault();
        Validate(configuration, port, type);
        return type;
    }

    internal static void ValidatePort(MachineConfiguration configuration, int port)
    {
        if (port < ControllerPortConstants.MinimumControllerPort
            || port >= PortCount(configuration))
            throw new ArgumentOutOfRangeException(nameof(port), port, null);
    }

    internal static void Validate(MachineConfiguration configuration, int port, PeripheralCategory type)
    {
        ValidatePort(configuration, port);
        if (!Supported(configuration, port).Contains(type))
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }

    internal static MachineConfiguration WithControllerType(MachineConfiguration configuration, int port, PeripheralCategory type)
    {
        Validate(configuration, port, type);
        var input = configuration.Input ?? new InputConfiguration();
        var bindings = input.Controllers ?? [];
        var binding = bindings.FirstOrDefault(item => item.Port == port) ?? new ControllerBinding(port, type);
        return configuration with
        {
            Input = input with
            {
                Controllers = bindings.Where(item => item.Port != port).Append(binding with { Peripheral = type }).ToArray()
            }
        };
    }

    internal static PeripheralCategory FromCommon(EmulationPeripheralCategory peripheral, MachineConfiguration configuration, int port)
    {
        ValidatePort(configuration, port);
        var types = Supported(configuration);
        var type = peripheral switch
        {
            EmulationPeripheralCategory.None => PeripheralCategory.None,
            EmulationPeripheralCategory.Automatic => ControllerCatalog.Default(configuration),
            EmulationPeripheralCategory.RetroPad or EmulationPeripheralCategory.Joystick => PeripheralCategory.Joystick,
            EmulationPeripheralCategory.AnalogJoystick => types.Contains(PeripheralCategory.AnalogJoystick) ? PeripheralCategory.AnalogJoystick : PeripheralCategory.Joystick,
            EmulationPeripheralCategory.Keyboard => PeripheralCategory.Keyboard,
            EmulationPeripheralCategory.Mouse => PeripheralCategory.Mouse,
            EmulationPeripheralCategory.LightGun => types.Contains(PeripheralCategory.GunCon2) ? PeripheralCategory.GunCon2 : types.Contains(PeripheralCategory.GunCon) ? PeripheralCategory.GunCon : PeripheralCategory.Justifier,
            EmulationPeripheralCategory.DrivingController => PeripheralCategory.NeGcon,
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
        };
        if (!types.Contains(type))
            throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null);
        return type;
    }

    /// <summary>Lit la composante native d’un stick ou d’une gâchette.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short NativeAnalogState(EmulationControllerState controller, uint index, uint id) => EmulationSdk.InputProcessing.NativeAnalogState(controller, index, id);

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

}
