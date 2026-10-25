using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Apple.Common.Functions;

internal static class ControllerFunctions
{
    /// <summary>Service public de consultation des descriptions et de traitement des entrées, emprunté au SDK.</summary>
    /// <remarks>Le service ne possède aucune machine ni aucun périphérique et ne conserve pas les descriptions reçues.</remarks>
    internal static IEmulationInputProcessing InputProcessing => EmulationSdk.InputProcessing;

    /// <summary>Lit le nombre de ports matériels avant les extensions propres au cœur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée dont le modèle détermine les ports de base.</param>
    /// <returns>Nombre matériel utilisé sans adaptateur ou comme comportement par défaut de son contrat.</returns>
    internal static int ModelPortCount(MachineConfiguration configuration) => ConfigurationFunctions.ControllerPortCount(configuration.Model);

    /// <summary>Résout les ports de session par le contrat Common du cœur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée, y compris les options de multitap ou d’extension.</param>
    /// <returns>Nombre annoncé par l’adaptateur ; nombre matériel lorsque la machine ne sélectionne aucun cœur.</returns>
    /// <remarks>Le contrat reçoit la configuration entière. Cette consultation ne crée ni cœur ni ressource native.</remarks>
    internal static int PortCount(MachineConfiguration configuration) => new Engine().SelectedAdapter(configuration)?.GetControllerPortCount(configuration) ?? ModelPortCount(configuration);

    /// <summary>Service public de description, validation et projection des retours, emprunté au SDK sans ressource physique possédée.</summary>
    internal static IEmulationControllerFeedbackProcessing Feedback => EmulationSdk.ControllerFeedback;

    /// <summary>Valide la syntaxe commune des associations clavier, souris et contrôleur.</summary>
    /// <param name="binding">Association empruntée ; une valeur nulle, vide ou blanche est acceptée.</param>
    /// <returns>True si chaque composante respecte la syntaxe commune, sans modifier les libellés ni les correspondances du module.</returns>
    internal static bool IsValidBinding(string? binding) => EmulationSdk.InputProcessing.IsValidBinding(binding);

    /// <summary>Crée un accumulateur indépendant pour conserver les entrées reçues entre deux lectures du cœur.</summary>
    /// <remarks>Ce relais Common demande l’instance au contrat SDK. Les collections restent empruntées et doivent être stables ; Update additionne les mouvements avec saturation, Consume remet seulement les déplacements et molettes à zéro, et Reset efface toutes les entrées. Le service ne possède aucune ressource native.</remarks>
    /// <returns>Contrat appartenant au consommateur ; chaque appel fournit un état indépendant.</returns>
    internal static IEmulationInputAccumulator CreateInputAccumulator() => EmulationSdk.InputProcessing.CreateInputAccumulator();

    /// <summary>Crée un stockage indépendant qui copie les entrées reçues et publie un instantané stable par frame.</summary>
    /// <returns>Contrat possédé par le consommateur, avec des collections copiées et sans ressource native.</returns>
    /// <remarks>Update copie les collections et additionne les quatre composantes relatives avec saturation et échange atomique. Poll les consomme une seule fois et conserve le dernier état absolu ; Polled reste stable jusqu’au Poll suivant. Chaque stockage appartient à son appelant et reste indépendant des autres sessions.</remarks>
    internal static IEmulationInputFrames CreateInputFrames() => EmulationSdk.InputProcessing.CreateInputFrames();


    internal static EmulationInputSnapshot ApplyDeadZones(EmulationInputSnapshot snapshot, IReadOnlyList<ControllerBinding>? bindings)
    {
        if (bindings is null || bindings.Count == BufferConstants.EmptyCollectionCount) return snapshot;
        var controllers = snapshot.Controllers.ToArray();
        foreach (var binding in bindings)
        {
            if (binding.Port < ControllerPortConstants.MinimumControllerPort || binding.Port >= controllers.Length) continue;
            if (binding.Peripheral == PeripheralCategory.None)
            {
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
            var controller = controllers[binding.Port];
            controllers[binding.Port] = EmulationSdk.InputProcessing.ApplyDeadZones(controller, binding.DeadZonePercent);
        }
        return snapshot with { Controllers = controllers };
    }

    internal static IReadOnlySet<PeripheralCategory> Peripherals(MachineModel model)
    {
        return new HashSet<PeripheralCategory>(HardwareModelCatalog.Get(model).Ports.Where(port => port.Capability != HardwarePortCapability.Keyboard).Select(port => FromCapability(port.Capability)).Append(PeripheralCategory.None)
            .Append(PeripheralCategory.Automatic));
    }

    private static PeripheralCategory FromCapability(HardwarePortCapability capability) => capability switch
    {
        HardwarePortCapability.Keyboard => PeripheralCategory.Keyboard,
        HardwarePortCapability.Mouse => PeripheralCategory.Mouse,
        HardwarePortCapability.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };
    /// <summary>Lit une commande native de souris à partir du pointeur de la frame.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short NativeMouseState(EmulationPointerState pointer, uint port, uint index, uint id) => EmulationSdk.InputProcessing.NativeMouseState(pointer, port, index, id);

    /// <summary>Lit un bouton ou le masque natif de la manette, avec la capacité fournie par l’adaptation.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short NativeJoypadState(uint buttons, uint id, uint maximumButtonCount) => EmulationSdk.InputProcessing.NativeJoypadState(buttons, id, maximumButtonCount);

    /// <summary>Lit la composante native d’un stick ou d’une gâchette.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short NativeAnalogState(EmulationControllerState controller, uint index, uint id) => EmulationSdk.InputProcessing.NativeAnalogState(controller, index, id);

    /// <summary>Calcule le caractère natif par le contrat commun des entrées.</summary>
    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys) => EmulationSdk.InputProcessing.Character(code, keys);

    /// <summary>Complète une plage de touches dans la table appartenant à l’adaptateur.</summary>
    internal static void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count) => EmulationSdk.InputProcessing.AddRange(map, firstKey, firstCode, count);

    /// <summary>Publie les codes ordonnés ; le propriétaire conserve la responsabilité de mémoriser l’état suivant.</summary>
    internal static void PublishCodeTransitions<TSet>(TSet current, ref TSet previous, ExternalCoreApi.KeyboardEvent keyboardEvent, ushort modifiers,
        Func<IEnumerable<uint>, IEnumerable<uint>> orderReleased, Func<IEnumerable<uint>, IEnumerable<uint>> orderPressed, Func<uint, uint> characterFor) where TSet : IReadOnlySet<uint>
        => EmulationSdk.InputProcessing.PublishCodeTransitions(current, ref previous, keyboardEvent, modifiers, orderReleased, orderPressed, characterFor);



    /// <summary>Convertit une catégorie publique vers le type de périphérique attendu par les règles existantes du module.</summary>
    /// <param name="peripheral">Catégorie générique fournie par le contrat public des entrées.</param>
    /// <param name="configuration">Configuration de session empruntée, utilisée lorsque la correspondance dépend du modèle ou du choix courant.</param>
    /// <param name="port">Indice du port ; la commande conserve le contrôle des ports et des choix réellement autorisés.</param>
    /// <returns>Catégorie interne attendue par le cœur, sans créer ni modifier de périphérique.</returns>
    /// <remarks>Cette opération conserve les correspondances antérieures. La sélection native et la publication
    /// des associations restent dans la commande du propriétaire de la machine, après acceptation du cœur.</remarks>
    internal static PeripheralCategory FromCommon(EmulationPeripheralCategory peripheral, MachineConfiguration configuration, int port)
    {
        return peripheral switch
    {
        EmulationPeripheralCategory.None => PeripheralCategory.None,
        EmulationPeripheralCategory.Automatic => PeripheralCategory.Automatic,
        EmulationPeripheralCategory.Keyboard => PeripheralCategory.Keyboard,
        EmulationPeripheralCategory.Mouse => PeripheralCategory.Mouse,
        EmulationPeripheralCategory.Joystick or EmulationPeripheralCategory.RetroPad
            or EmulationPeripheralCategory.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
    };
    }
}
