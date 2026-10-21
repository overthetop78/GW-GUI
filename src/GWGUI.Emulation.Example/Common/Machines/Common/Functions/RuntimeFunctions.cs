using System.Diagnostics;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Construit les informations de fonctionnement présentées par la machine.</summary>
/// <remarks>Ces fonctions consultent un cœur ou un processus emprunté ; elles n’en prennent pas possession.</remarks>
internal static class RuntimeFunctions
{
    /// <summary>Interprète le code de région annoncé par le protocole du cœur.</summary>
    /// <param name="value">Code entier du standard vidéo.</param>
    /// <returns>Standard PAL ou NTSC reconnu, sinon aucune valeur.</returns>
    internal static EmulationVideoStandard? ReadRegion(int value) => EmulationHostProtocolFunctions.ReadRegion(value, allowAllDefinedStandards: false);

    /// <summary>Traduit l’état du processus et de sa connexion vers les états internes de la machine.</summary>
    /// <param name="process">Processus emprunté, ou null avant son lancement.</param>
    /// <param name="connectionFailed">Indique un échec de communication, prioritaire sur les autres états.</param>
    /// <param name="disposed">Indique que l’hôte a déjà été libéré.</param>
    /// <returns>Faulted en cas d’échec, NotStarted sans processus, Exited après libération ou sortie, sinon Running.</returns>
    internal static HostProcessState ProcessState(Process? process, bool connectionFailed, bool disposed)
        =>
            EmulationHostProtocolFunctions.GetProcessState(process, connectionFailed, disposed,
                HostProcessState.Faulted, HostProcessState.NotStarted, HostProcessState.Exited, HostProcessState.Running);

    /// <summary>Assemble un instantané du statut à partir des dernières valeurs disponibles.</summary>
    /// <param name="configuration">Configuration fournissant l’identité du modèle.</param>
    /// <param name="core">Cœur emprunté fournissant les mesures, les LED et la dernière image.</param>
    /// <returns>Statut contenant une géométrie si une image existe, une copie des LED et les compteurs audio.</returns>
    /// <remarks>La géométrie exprime largeur et hauteur en pixels, pas mémoire en octets et rapport d’aspect.
    /// La fréquence audio est en Hz et la cadence vidéo en images par seconde. Le dictionnaire d’activité des médias est vide ici.</remarks>
    internal static RuntimeStatus Status(MachineConfiguration configuration, IEmulatorCore core)
    {
        var frame = core.LatestVideoFrame;
        var geometry = frame is null ? null : new RuntimeGeometry(frame.Width, frame.Height, frame.Pitch, frame.AspectRatio);
        // Les LED et l’activité des lecteurs restent deux informations distinctes : aucune activité n’est déduite des LED.
        return new RuntimeStatus(configuration.Model, core.Region, core.FramesPerSecond, core.SampleRate,
            geometry, core.CoreName, new Dictionary<EmulationMediaSlot, bool>(),
            new Dictionary<int, bool>(core.LedStates), core.BufferedAudioFrames, core.AudioOverrunCount,
            core.AudioUnderrunCount, core.HostProcessState, core.HostProcessId);
    }
}

/// <summary>Consulte les contraintes de redémarrage déclarées par l’adaptateur.</summary>
internal static class RuntimeOptionFunctions
{
    /// <summary>Recherche une option exigeant un redémarrage du cœur.</summary>
    /// <param name="emulator">Adaptateur sélectionné, éventuellement None.</param>
    /// <param name="key">Identifiant exact de l’option.</param>
    /// <returns>Vrai si l’adaptateur expose les réglages et déclare cette contrainte pour la clé ; faux sinon.</returns>
    internal static bool RequiresRestart(Emulator emulator, string key) => emulator != Emulator.None && new Engine().Adapter(emulator) is IEmulatorSettings settings && settings.GetSettingsOptions().Any(option => option.Key == key && option.RequiresRestart);
}

/// <summary>Adapte les erreurs internes aux messages publics contextualisés pour l’application.</summary>
internal static class MessageFunctions
{
    /// <summary>Traduit une erreur en conservant son origine et en lui associant la machine concernée.</summary>
    /// <param name="error">Erreur originale, conservée par le service de traduction.</param>
    /// <param name="configuration">Configuration fournissant l’identifiant de machine.</param>
    /// <returns>Exception adaptée au contrat de messages de l’application.</returns>
    /// <remarks>Une erreur déjà localisée conserve son texte. Une demande de contenu est ici traduite en demande de disquette.
    /// Une exception extérieure au modèle interne devient une erreur de démarrage.</remarks>
    internal static Exception Translate(Exception error, MachineConfiguration configuration)
    {
        var context = new EmulationMachineMessageContext(configuration.MachineId);
        if (error is not EmulationException coreError)
            return EmulationErrorService.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, context);
        if (coreError.IsLocalized)
            return EmulationErrorService.TranslateLocalized(error, Category(coreError.Category), context);
        if (coreError.Code == ErrorCode.ContentRequired)
            return EmulationErrorService.RequiredMediaMissing(configuration.MachineId, [EmulationMediaCategory.FloppyDrive], error);
        return EmulationErrorService.Translate(error, Category(coreError.Category), MessageCode(coreError.Code), context);
    }

    /// <summary>Associe une catégorie interne à celle du contrat de messages.</summary>
    /// <param name="category">Catégorie de l’erreur interne.</param>
    /// <returns>Catégorie publique correspondante, ou Machine pour les autres valeurs.</returns>
    private static EmulationMessageCategory Category(ErrorCategory category) => category switch
    {
        ErrorCategory.Core or ErrorCategory.Host => EmulationMessageCategory.Emulator,
        ErrorCategory.Firmware => EmulationMessageCategory.Firmware,
        ErrorCategory.Content => EmulationMessageCategory.Media,
        ErrorCategory.State => EmulationMessageCategory.SavedState,
        _ => EmulationMessageCategory.Machine
    };

    /// <summary>Associe un code interne à un message compréhensible par l’application.</summary>
    /// <param name="code">Code décrivant l’échec du cœur, du contenu, du firmware ou de l’état.</param>
    /// <returns>Code public correspondant, ou MachineStartFailed en l’absence de correspondance.</returns>
    private static EmulationMessageCode MessageCode(ErrorCode code) => code switch
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
