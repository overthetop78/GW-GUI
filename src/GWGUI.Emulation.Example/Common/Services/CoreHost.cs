using System.IO;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Example.Common.Constants;
using GWGUI.Emulation.Example.Common.Enums;
using GWGUI.Emulation.Example.Common.Exceptions;
using GWGUI.Emulation.Example.Common.Functions;
using GWGUI.Emulation.Example.Common.Interfaces;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Exécute le côté serveur du protocole de cœur dans le processus hôte.</summary>
/// <remarks>Le client ProcessCore envoie les commandes sur un tube nommé. Cet hôte possède le cœur créé par la fabrique
/// et le rédacteur vidéo partagé ; leurs ressources sont libérées à la fin de la session, même après une erreur.</remarks>
internal static class CoreHost
{
    /// <summary>Connecte la session puis traite les commandes jusqu’à sa fermeture.</summary>
    /// <param name="pipeName">Nom du tube nommé créé par le processus appelant.</param>
    /// <param name="videoMapName">Préfixe du segment vidéo partagé servant à publier les images.</param>
    /// <param name="createCore">Fabrique recevant chemin de bibliothèque et identifiant d’émulateur ; le cœur retourné appartient à la session.</param>
    /// <remarks>Les commandes principales sont dispatchées ici, les autres passent par le dispatcher commun.
    /// Une erreur est sérialisée dans une réponse d’échec. Le cœur est libéré dans le finally de la session.</remarks>
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void Run(string pipeName, string videoMapName, Func<string, Emulator, IEmulatorCore> createCore)
    {
        ArgumentNullException.ThrowIfNull(createCore);
        using var video = new SharedVideoWriter(videoMapName);
        EmulationSdk.HostProtocol.RunConnectedSession(pipeName, CoreHostConstants.LocalPipeServerName, CoreHostConstants.ConnectionTimeoutMilliseconds, (reader, transportWriter) =>
        {
            IEmulatorCore? core = null;
            var lastVideoSequence = CoreHostConstants.InitialVideoSequence;
            var lastDiagnosticCount = CoreHostConstants.InitialDiagnosticCount;
            try
            {
                EmulationSdk.HostProtocol.RunCommandLoop<HostCommand>(reader, transportWriter, static commandReader => CoreHostFunctions.ReadRequestHeader(commandReader), HostCommand.Dispose, (command, writer) =>
                {
                    CoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Success);
                    switch (command)
                    {
                        case HostCommand.Initialize:
                            Initialize(reader, writer, createCore, ref core, ref lastDiagnosticCount);
                            lastVideoSequence = CoreHostConstants.InitialVideoSequence;
                            break;
                        case HostCommand.RunFrame:
                            RunFrame(reader, writer, video, EnsureCore(core), ref lastVideoSequence, ref lastDiagnosticCount);
                            break;
                        case HostCommand.SoftReset:
                            EnsureCore(core).SoftReset();
                            break;
                        case HostCommand.SaveMediaChanges:
                            EnsureCore(core).SaveMediaChanges(EmulationMediaSlot.FromProtocolValue(reader.ReadInt32()));
                            break;
                        case HostCommand.HasUnsavedMediaChanges:
                            writer.Write(EnsureCore(core).HasUnsavedMediaChanges(EmulationMediaSlot.FromProtocolValue(reader.ReadInt32())));
                            break;
                        case HostCommand.Dispose:
                            EmulationSdk.HostProtocol.DisposePreviousCore(ref core);
                            break;
                        default:
                            if (!EmulationSdk.HostProtocol.TryExecuteCoreCommand<MediaConfiguration, PeripheralCategory, DiskStatus>(
                                (byte)command, reader, writer, () => EnsureCore(core), ReadMedia,
                                CoreHostFunctions.ReadBytes, CoreHostFunctions.WriteDiskStatus,
                                static value => (PeripheralCategory)value))
                                throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, CoreHostErrors.UnknownCommandFormat, (byte)command));
                            break;
                    }
                }, (writer, error) =>
                {
                    CoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Failure);
                    CoreHostFunctions.WriteError(writer, error);
                });
            }
            finally
            {
                EmulationSdk.HostProtocol.DisposePreviousCore(ref core);
            }
        });
    }

    /// <summary>Lit la requête d’initialisation, remplace l’ancien cœur et écrit ses capacités initiales.</summary>
    /// <param name="reader">Lecteur emprunté de la requête d’initialisation.</param>
    /// <param name="writer">Rédacteur emprunté de la réponse.</param>
    /// <param name="createCore">Fabrique du cœur détenu par la session.</param>
    /// <param name="core">Référence du cœur de session, remplacée par l’initialisation commune.</param>
    /// <param name="lastDiagnosticCount">Nombre de diagnostics déjà transmis, mis à jour par le protocole.</param>
    /// <remarks>Le client et l’hôte partagent les options JSON. La réponse inclut les capacités d’états sauvegardés,
    /// la région, les compteurs audio et les LED. Le statut spécifique commence par la latence minimale
    /// en millisecondes, puis le booléen de prise en charge des états. ProcessCore lit ce même ordre.
    /// Un cœur dont Initialize échoue est immédiatement libéré et retiré de la session dans finally.</remarks>
    private static void Initialize(BinaryReader reader, BinaryWriter writer, Func<string, Emulator, IEmulatorCore> createCore, ref IEmulatorCore? core, ref int lastDiagnosticCount)
    {
        EmulationSdk.HostProtocol.DisposePreviousCore(ref core);
        var (corePath, emulator, session, saves, configuration) = EmulationSdk.HostProtocol.ReadInitializationRequest(reader,
            static requestReader => (Emulator)requestReader.ReadInt32(), CoreHostFunctions.ReadString,
            static requestReader => JsonSerializer.Deserialize<MachineConfiguration>(requestReader.ReadString(), CoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.InvalidConfiguration));
        core = createCore(corePath, emulator);
        var initialized = false;
        try
        {
            core.Initialize(configuration, session, saves);
            initialized = true;
        }
        finally
        {
            if (!initialized)
            {
                try { core.Dispose(); }
                finally { core = null; }
            }
        }
        var initializedCore = EnsureCore(core);
        EmulationSdk.HostProtocol.WriteCoreInitialization<IEmulatorCore, MachineConfiguration, CoreOption>(
            writer, initializedCore, CoreHostFunctions.JsonOptions, ref lastDiagnosticCount, CoreHostConstants.ExtensionListSeparator,
            static (statusWriter, currentCore) =>
            {
                statusWriter.Write(currentCore.MinimumAudioLatencyMilliseconds);
                statusWriter.Write(currentCore.SupportsSaveStates);
                EmulationSdk.HostProtocol.WriteRuntimeStatus(statusWriter, currentCore.Region, currentCore.BufferedAudioFrames, currentCore.AudioOverrunCount, currentCore.AudioUnderrunCount);
            });
        CoreHostFunctions.WriteLedStates(writer, initializedCore.LedStates);
        writer.Write(initializedCore.ShutdownRequested);
        writer.Write(JsonSerializer.Serialize(initializedCore.ControllerFeedback, CoreHostFunctions.JsonOptions));
    }

    /// <summary>Applique les entrées, exécute une image et publie les sorties du cœur.</summary>
    /// <param name="reader">Lecteur emprunté contenant l’instantané des entrées.</param>
    /// <param name="writer">Rédacteur emprunté recevant audio, statut et références vidéo.</param>
    /// <param name="video">Rédacteur emprunté des pixels en mémoire partagée.</param>
    /// <param name="core">Cœur initialisé de la session.</param>
    /// <param name="lastVideoSequence">Dernière séquence vidéo transmise, mise à jour lors de la publication.</param>
    /// <param name="lastDiagnosticCount">Nombre de diagnostics déjà envoyés, mis à jour pour transmettre les changements.</param>
    /// <remarks>La requête contient les entrées puis un booléen de présence du statut audio ; s’il est présent,
    /// quatre valeurs suivent : fréquence, remplissage, capacité et activité. La réponse commune écrit la vidéo,
    /// l’audio, la latence minimale, le statut, le support des états, les diagnostics, les LED, la demande
    /// de fermeture et les retours de contrôleurs sérialisés en JSON.
    /// Le lecteur de ProcessCore doit consommer exactement les mêmes champs, même si le cœur n’émet aucun son.</remarks>
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    private static void RunFrame(BinaryReader reader, BinaryWriter writer, SharedVideoWriter video, IEmulatorCore core, ref long lastVideoSequence, ref int lastDiagnosticCount)
    {
        core.SetInput(CoreHostFunctions.ReadInput(reader));
        core.SetAudioBufferStatus(reader.ReadBoolean() ? new AudioOutputBufferStatus(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean()) : null);
        core.RunFrame();
        EmulationSdk.HostProtocol.WriteNativeCoreFrameResponse<IEmulatorCore, MachineConfiguration, CoreOption>(
            writer, core, (frameWriter, pendingFrame) => CoreHostFunctions.WriteResizableSharedFrame(frameWriter, pendingFrame, video),
            CoreHostFunctions.WriteLedStates, static () => CoreHostFunctions.JsonOptions,
            ref lastVideoSequence, ref lastDiagnosticCount,
            writeStateSupport: static (statusWriter, currentCore) => statusWriter.Write(currentCore.SupportsSaveStates),
            writeAudioStatus: static (statusWriter, currentCore) => statusWriter.Write(currentCore.MinimumAudioLatencyMilliseconds));
        writer.Write(core.ShutdownRequested);
        writer.Write(JsonSerializer.Serialize(core.ControllerFeedback, CoreHostFunctions.JsonOptions));
    }

    /// <summary>Désérialise la description d’un média reçue du client.</summary>
    /// <param name="reader">Lecteur emprunté positionné avant la chaîne JSON.</param>
    /// <returns>Description interne du média.</returns>
    /// <exception cref="InvalidDataException">La désérialisation renvoie null.</exception>
    private static MediaConfiguration ReadMedia(BinaryReader reader) => JsonSerializer.Deserialize<MediaConfiguration>(reader.ReadString(), CoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.InvalidConfiguration);

    /// <summary>Vérifie qu’une commande dispose d’un cœur initialisé.</summary>
    /// <param name="core">Référence courante de la session.</param>
    /// <returns>Référence empruntée du cœur présent.</returns>
    /// <exception cref="InvalidOperationException">Aucun cœur n’est disponible.</exception>
    private static IEmulatorCore EnsureCore(IEmulatorCore? core) => core ?? throw new InvalidOperationException(CoreHostErrors.NotInitialized);
}
