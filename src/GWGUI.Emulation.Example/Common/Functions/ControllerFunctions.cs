using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation;
using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Example.Common.Functions;

/// <summary>Adaptation des périphériques et accès aux contrats communs de traitement des entrées.</summary>
/// <remarks>La projection des associations, les zones mortes et les ports désactivés sont traités dans
/// InputSnapshotFunctions. Les services retournés ici sont empruntés ; les stockages créés appartiennent
/// à leur session. Les lectures natives conservent les unités des commandes déjà projetées.</remarks>
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
    /// <returns>Nombre annoncé par l’adaptateur ; nombre configuré, extension comprise, sans cœur sélectionné.</returns>
    /// <remarks>Le contrat reçoit la configuration entière. Cette consultation ne crée ni cœur ni ressource native.</remarks>
    internal static int PortCount(MachineConfiguration configuration) => new Engine().SelectedAdapter(configuration)?.GetControllerPortCount(configuration) ?? ConfigurationFunctions.ControllerPortCount(configuration);

    /// <summary>Service public de description, validation et projection des retours, emprunté au SDK sans ressource physique possédée.</summary>
    internal static IEmulationControllerFeedbackProcessing Feedback => EmulationSdk.ControllerFeedback;

    /// <summary>Crée un stockage indépendant qui copie les entrées reçues et publie un instantané stable par frame.</summary>
    /// <returns>Contrat possédé par le consommateur, avec des collections copiées et sans ressource native.</returns>
    /// <remarks>Update copie les collections et additionne les quatre composantes relatives avec saturation et échange atomique. Poll les consomme une seule fois et conserve le dernier état absolu ; Polled reste stable jusqu’au Poll suivant. Chaque stockage appartient à son appelant et reste indépendant des autres sessions.</remarks>
    internal static IEmulationInputFrames CreateInputFrames() => EmulationSdk.InputProcessing.CreateInputFrames();

    /// <summary>Associe chaque catégorie générique à la classe native qu’interrogera le cœur.</summary>
    /// <param name="peripheral">Catégorie du port, déjà contrôlée dans la configuration.</param>
    /// <returns>Classe native commune aux trois modes fictifs, indépendante du matériel physique.</returns>
    /// <remarks>Les joysticks analogiques, volants et contrôleurs rotatifs exposent les sticks et les pressions.
    /// Le pistolet et le stylet conservent leurs classes de pointage ; le clavier et la souris utilisent leurs
    /// lecteurs dédiés. Les autres catégories échangent des boutons, et Automatic reprend la manette générique.</remarks>
    internal static uint ResolveControllerDevice(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.None => ExternalCoreApiConstants.NoDevice,
        PeripheralCategory.Mouse => ExternalCoreApiConstants.MouseDevice,
        PeripheralCategory.Keyboard => ExternalCoreApiConstants.KeyboardDevice,
        PeripheralCategory.LightGun => ExternalCoreApiConstants.LightGunDevice,
        PeripheralCategory.Pointer => ExternalCoreApiConstants.PointerDevice,
        PeripheralCategory.AnalogJoystick or PeripheralCategory.DrivingController or PeripheralCategory.Paddle => ExternalCoreApiConstants.AnalogDevice,
        PeripheralCategory.Automatic or PeripheralCategory.RetroPad or PeripheralCategory.Joystick or PeripheralCategory.NumericKeypad or PeripheralCategory.DanceMat => ExternalCoreApiConstants.JoypadDevice,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
    };

    /// <summary>Fournit la table clavier du contrat natif par l’interface commune du SDK.</summary>
    /// <returns>Correspondances dont les identités ne dépendent d’aucune marque de machine.</returns>
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateNativeKeyMap() => EmulationSdk.InputProcessing.CreateKeyMap();

    /// <summary>Lit l’instantané stable de la frame pour un callback d’entrée natif.</summary>
    /// <param name="snapshot">Entrées déjà projetées par l’adaptateur du module.</param>
    /// <param name="port">Indice du port émulé.</param>
    /// <param name="device">Classe native de périphérique demandée par le cœur.</param>
    /// <param name="index">Indice de composante de cette classe.</param>
    /// <param name="id">Identifiant de la commande native à interroger.</param>
    /// <returns>Valeur calculée par les sélecteurs existants du SDK.</returns>
    internal static short ReadNativeInput(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id) => EmulationSdk.InputProcessing.ReadNativeInput(snapshot, port, device, index, id);

    /// <summary>Applique le changement de périphérique d’un port sans perdre ses associations physiques.</summary>
    /// <param name="configuration">Configuration d’origine à conserver immuable.</param>
    /// <param name="port">Indice du port émulé, à partir de zéro.</param>
    /// <param name="peripheral">Catégorie générique à connecter sur ce port.</param>
    /// <returns>Configuration validée dont seul le périphérique du port demandé est remplacé.</returns>
    internal static MachineConfiguration WithControllerType(MachineConfiguration configuration, int port, PeripheralCategory peripheral)
    {
        var controllers = (configuration.Input.Controllers ?? []).Where(binding => binding.Port != port).ToList();
        var previous = configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port);
        controllers.Add(previous is null ? new ControllerBinding(port, peripheral) : previous with { Peripheral = peripheral });
        var updated = configuration with { Input = configuration.Input with { Controllers = controllers.OrderBy(binding => binding.Port).ToArray() } };
        ConfigurationFunctions.Validate(updated);
        return updated;
    }

    /// <summary>Convertit une catégorie publique vers le type de périphérique attendu par les règles existantes du module.</summary>
    /// <param name="peripheral">Catégorie générique fournie par le contrat public des entrées.</param>
    /// <param name="configuration">Configuration de session empruntée, utilisée lorsque la correspondance dépend du modèle ou du choix courant.</param>
    /// <param name="port">Indice du port ; la commande conserve le contrôle des ports et des choix réellement autorisés.</param>
    /// <returns>Catégorie interne attendue par le cœur, sans créer ni modifier de périphérique.</returns>
    /// <remarks>Le tapis de danse et le stylet conservent leur catégorie jusqu’à la sélection de leur classe native.
    /// Cette opération conserve les autres correspondances. La sélection native et la publication
    /// des associations restent dans la commande du propriétaire de la machine, après acceptation du cœur.</remarks>
    internal static PeripheralCategory FromCommon(EmulationPeripheralCategory peripheral, MachineConfiguration configuration, int port)
    {
        if (port < 0 || port >= PortCount(configuration)) throw new ArgumentOutOfRangeException(nameof(port));
        if (!Enum.IsDefined(peripheral)) throw new ArgumentOutOfRangeException(nameof(peripheral));
        return peripheral switch
        {
            EmulationPeripheralCategory.None => PeripheralCategory.None,
            EmulationPeripheralCategory.Automatic => PeripheralCategory.Automatic,
            EmulationPeripheralCategory.Keyboard => PeripheralCategory.Keyboard,
            EmulationPeripheralCategory.Mouse => PeripheralCategory.Mouse,
            EmulationPeripheralCategory.Joystick => PeripheralCategory.Joystick,
            EmulationPeripheralCategory.AnalogJoystick => PeripheralCategory.AnalogJoystick,
            EmulationPeripheralCategory.Paddle => PeripheralCategory.Paddle,
            EmulationPeripheralCategory.LightGun => PeripheralCategory.LightGun,
            EmulationPeripheralCategory.NumericKeypad => PeripheralCategory.NumericKeypad,
            EmulationPeripheralCategory.DrivingController => PeripheralCategory.DrivingController,
            EmulationPeripheralCategory.DanceMat => PeripheralCategory.DanceMat,
            EmulationPeripheralCategory.Pointer => PeripheralCategory.Pointer,
            _ => PeripheralCategory.RetroPad
        };

    }
}
