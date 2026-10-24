using System.Diagnostics;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Projection des mesures du cœur vers le statut public de la machine.</summary>
internal static class RuntimeFunctions
{
    /// <summary>Décode le standard vidéo reçu par le protocole commun.</summary>
    /// <param name="value">Valeur numérique du champ de région.</param>
    /// <returns>Standard reconnu, ou null pour une valeur non définie.</returns>
    internal static EmulationVideoStandard? ReadRegion(int value) => EmulationSdk.HostProtocol.ReadRegion(value, allowAllDefinedStandards: true);

/// <summary>Construit l’instantané des sorties et activités actuellement annoncées par le cœur.</summary>
    /// <param name="configuration">Configuration portant le modèle et les lecteurs à décrire.</param>
    /// <param name="core">Cœur emprunté sur son thread propriétaire ou après publication de ses données.</param>
    /// <returns>Mesures et copies des dictionnaires de statut ; aucune ressource native n’est transférée.</returns>
    /// <param name="mediaCapabilities">Facette média empruntée à la session, ou null pour consulter le choix du catalogue dans un appel indépendant.</param>
    internal static RuntimeStatus Status(MachineConfiguration configuration, IEmulatorCore core, IEmulatorMediaCapabilities? mediaCapabilities = null)
    {
        var frame = core.LatestVideoFrame;
        var geometry = frame is null ? null : new RuntimeGeometry(frame.Width, frame.Height, frame.Pitch, frame.AspectRatio);
        var activity = (mediaCapabilities ?? new Engine().SelectedAdapter(configuration))?.MediaActivity(configuration, core.LedStates) ?? new Dictionary<EmulationMediaSlot, bool>();
        return new(configuration.Model, core.Region, core.FramesPerSecond, core.SampleRate, geometry, core.CoreName,
            new Dictionary<EmulationMediaSlot, bool>(activity), new Dictionary<int, bool>(core.LedStates), core.BufferedAudioFrames,
            core.AudioOverrunCount, core.AudioUnderrunCount, core.HostProcessState, core.HostProcessId);
    }
}

/// <summary>Consultation des options dont l’adaptateur exige la recréation du cœur.</summary>
internal static class RuntimeOptionFunctions
{
    /// <summary>Consulte les descriptions de réglages du cœur sélectionné.</summary>
    /// <param name="emulator">Émulateur dont les règles doivent être consultées.</param>
    /// <param name="key">Identifiant technique de l’option.</param>
    /// <returns>True si la description courante de l’option exige un redémarrage.</returns>
    internal static bool RequiresRestart(Emulator emulator, string key) => emulator != Emulator.None && new Engine().Adapter(emulator).GetSettingsOptions().Any(option => option.Key == key && option.RequiresRestart);
}

/// <summary>Adaptation des erreurs locales aux messages publics reconnus par l’application.</summary>
internal static class MessageFunctions
{
    /// <summary>Conserve l’annulation ou contextualise la cause avec l’identité du profil.</summary>
    /// <param name="error">Cause à transmettre sans perdre son exception interne.</param>
    /// <param name="configuration">Configuration du profil concerné.</param>
    /// <returns>Erreur conforme au contrat commun de messages.</returns>
    internal static Exception Translate(Exception error, MachineConfiguration configuration)
    {
        var context = new EmulationMachineMessageContext(configuration.MachineId);
        if (error is not EmulationException coreError) return EmulationSdk.Messages.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, context);
        if (coreError.IsLocalized) return EmulationSdk.Messages.TranslateLocalized(error, Category(coreError.Category), context);
        if (coreError.Code == ErrorCode.ContentRequired) return EmulationSdk.Messages.RequiredMediaMissing(configuration.MachineId, StorageSettingsFunctions.DescribeDevices(configuration.Model).Select(device => device.Slot.Category).Distinct().ToArray(), error);
        return EmulationSdk.Messages.Translate(error, Category(coreError.Category), Code(coreError.Code), context);
    }

    /// <summary>Convertit la catégorie interne vers son domaine public de diagnostic.</summary>
    /// <param name="category">Catégorie conservée par l’exception du module.</param>
    /// <returns>Domaine public employé par la traduction de l’hôte.</returns>
    private static EmulationMessageCategory Category(ErrorCategory category) => category switch
    {
        ErrorCategory.Core or ErrorCategory.Host => EmulationMessageCategory.Emulator,
        ErrorCategory.Firmware => EmulationMessageCategory.Firmware,
        ErrorCategory.Content => EmulationMessageCategory.Media,
        ErrorCategory.State => EmulationMessageCategory.SavedState,
        _ => EmulationMessageCategory.Machine
    };

    /// <summary>Convertit le motif interne vers le code public de message correspondant.</summary>
    /// <param name="code">Motif de l’échec décrit par le module.</param>
    /// <returns>Code public conservant la distinction entre contenu, firmware, état et protocole.</returns>
    private static EmulationMessageCode Code(ErrorCode code) => code switch
    {
        ErrorCode.CoreNotFound => EmulationMessageCode.EmulatorNotInstalled,
        ErrorCode.CoreRejected => EmulationMessageCode.EmulatorRejected,
        ErrorCode.FirmwareMissing => EmulationMessageCode.FirmwareMissing,
        ErrorCode.FirmwareInvalid => EmulationMessageCode.FirmwareIncompatible,
        ErrorCode.ContentNotFound => EmulationMessageCode.MediaNotFound,
        ErrorCode.ContentUnsupported => EmulationMessageCode.MediaUnsupported,
        ErrorCode.OptionInvalid => EmulationMessageCode.OptionInvalid,
        ErrorCode.HostProtocolFailure => EmulationMessageCode.HostCommunicationFailed,
        ErrorCode.StateInvalid => EmulationMessageCode.SavedStateInvalid,
        ErrorCode.StateIncompatible => EmulationMessageCode.SavedStateIncompatible,
        _ => EmulationMessageCode.MachineStartFailed
    };
}
