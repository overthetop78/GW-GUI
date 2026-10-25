using System.IO;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Constants;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Functions;

internal static partial class EmulationHostProtocolFunctions
{
    /// <summary>Écrit le codec existant des deux moteurs après validation de tous les effets.</summary>
    internal static void WriteControllerFeedback(BinaryWriter writer, IReadOnlyList<ControllerFeedbackState> states)
    {
        ValidateControllerFeedback(states);
        writer.Write(states.Count);
        foreach (var state in states)
        {
            writer.Write(state.Port);
            writer.Write(state.CommandId == EmulationControllerFeedbackCommands.Strong);
            writer.Write(state.Strength);
        }
    }

    /// <summary>Lit une collection bornée puis contrôle ses ports, commandes, intensités et doublons.</summary>
    internal static IReadOnlyList<ControllerFeedbackState> ReadControllerFeedback(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        if (count is < 0 or > EmulationHostProtocolConstants.MaximumControllerFeedbackCount)
            throw new InvalidDataException(EmulationHostProtocolConstants.InvalidControllerFeedbackCount);
        var states = new ControllerFeedbackState[count];
        for (var index = 0; index < count; index++)
            states[index] = new(reader.ReadInt32(), reader.ReadBoolean() ? EmulationControllerFeedbackCommands.Strong : EmulationControllerFeedbackCommands.Weak, reader.ReadSingle());
        ValidateControllerFeedback(states);
        return states;
    }

    private static void ValidateControllerFeedback(IReadOnlyList<ControllerFeedbackState> states)
    {
        if (states.Count > EmulationHostProtocolConstants.MaximumControllerFeedbackCount)
            throw new InvalidDataException(EmulationHostProtocolConstants.InvalidControllerFeedbackCount);
        var seen = new HashSet<(int, string)>();
        foreach (var state in states)
            if (state.Port is < 0 or >= EmulationHostProtocolConstants.MaximumInputControllerCount
                || state.CommandId is not (EmulationControllerFeedbackCommands.Strong or EmulationControllerFeedbackCommands.Weak)
                || !float.IsFinite(state.Strength) || state.Strength is < 0 or > 1
                || !seen.Add((state.Port, state.CommandId)))
                throw new InvalidDataException(EmulationHostProtocolConstants.InvalidControllerFeedbackState);
    }

    /// <summary>Prépare le lancement sans fenêtre de l’hôte ; son lancement et sa destruction restent à la charge de l’appelant.</summary>
    internal static ProcessStartInfo CreateHostStartInfo(string executablePath, string workingDirectory,
        string hostCommand, string pipeName, string videoMapName)
    {
        var startInfo = new ProcessStartInfo(executablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };
        startInfo.ArgumentList.Add(hostCommand);
        startInfo.ArgumentList.Add(pipeName);
        startInfo.ArgumentList.Add(videoMapName);
        return startInfo;
    }

    /// <summary>Attend la connexion sur le pipe emprunté avec délai et, selon la surcharge, annulation.</summary>
    internal static void WaitForConnection(NamedPipeServerStream pipe, TimeSpan connectionTimeout,
        CancellationToken cancellationToken)
    {
        var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            timeout.CancelAfter(connectionTimeout);
            pipe.WaitForConnectionAsync(timeout.Token).GetAwaiter().GetResult();
        }
        finally { timeout.Dispose(); }
    }

    /// <summary>Attend la connexion sur le pipe emprunté avec délai et, selon la surcharge, annulation.</summary>
    internal static void WaitForConnection(NamedPipeServerStream pipe, TimeSpan connectionTimeout)
    {
        var timeout = new CancellationTokenSource(connectionTimeout);
        try { pipe.WaitForConnectionAsync(timeout.Token).GetAwaiter().GetResult(); }
        finally { timeout.Dispose(); }
    }

    /// <summary>Libère une ressource possédée par l’appelant en conservant la politique des erreurs de transport.</summary>
    internal static void DisposeTransport(IDisposable? resource)
    {
        if (resource is null) return;
        try
        {
            resource.Dispose();
        }
        catch (IOException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }

    /// <summary>Libère une ressource possédée selon la politique existante qui ignore les erreurs de destruction.</summary>
    internal static void DisposeTransportIgnoringErrors(IDisposable? value)
    {
        try { value?.Dispose(); }
        catch (Exception) { }
    }

    /// <summary>Retire la référence au cœur précédent avant sa destruction, y compris si celle-ci échoue.</summary>
    internal static void DisposePreviousCore<TCore>(ref TCore? core) where TCore : class, IDisposable
    {
        var previousCore = core;
        core = null;
        try { previousCore?.Dispose(); }
        finally { core = null; }
    }

    /// <summary>Lit la demande d’initialisation avec les codecs typés du module et l’ordre de protocole commun.</summary>
    internal static (string CorePath, string Session, string? Saves, TConfiguration Configuration)
        ReadInitializationRequest<TConfiguration>(BinaryReader reader,
            Func<BinaryReader, string?> readSaves, Func<BinaryReader, TConfiguration> readConfiguration)
    {
        var corePath = reader.ReadString();
        var session = reader.ReadString();
        var saves = readSaves(reader);
        var configuration = readConfiguration(reader);
        return (corePath, session, saves, configuration);
    }

    /// <summary>Lit la demande d’initialisation avec les codecs typés du module et l’ordre de protocole commun.</summary>
    internal static (string CorePath, TEmulator Emulator, string Session, string? Saves, TConfiguration Configuration)
        ReadInitializationRequest<TEmulator, TConfiguration>(BinaryReader reader,
            Func<BinaryReader, TEmulator> readEmulator, Func<BinaryReader, string?> readSaves,
            Func<BinaryReader, TConfiguration> readConfiguration)
    {
        var corePath = reader.ReadString();
        var emulator = readEmulator(reader);
        var session = reader.ReadString();
        var saves = readSaves(reader);
        var configuration = readConfiguration(reader);
        return (corePath, emulator, session, saves, configuration);
    }

    // La session possède l'emplacement ; le contrat partagé nettoie immédiatement un échec d'initialisation.
    /// <summary>Remplace le cœur, publie sa référence pour le nettoyage de l’hôte puis initialise ses services et écrit la réponse.</summary>
    internal static void InitializeCore<TCore, TEmulator, TConfiguration, TOption>(
        BinaryReader reader, BinaryWriter writer, Func<string, TEmulator, TCore> createCore,
        ref TCore? core, ref int lastDiagnosticCount,
        Func<BinaryReader, TEmulator> readEmulator, Func<BinaryReader, string?> readSaves,
        Func<BinaryReader, TConfiguration> readConfiguration, Func<JsonSerializerOptions> jsonOptions,
        char extensionSeparator, Action<BinaryWriter, TCore> writeStatus,
        Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates)
        where TCore : class, IEmulationCoreRuntime<TConfiguration, TOption>
    {
        DisposePreviousCore(ref core);
        var (corePath, emulator, session, saves, configuration) =
            ReadInitializationRequest(reader, readEmulator, readSaves, readConfiguration);
        core = createCore(corePath, emulator);
        var initializedCore = InitializeCore<TCore, TConfiguration, TOption>(ref core, configuration, session, saves);
        WriteCoreInitialization<TCore, TConfiguration, TOption>(
            writer, initializedCore, jsonOptions(), ref lastDiagnosticCount, extensionSeparator, writeStatus);
        writeLedStates(writer, initializedCore.LedStates);
    }

    /// <summary>Initialise le cœur de session et le libère dans finally si son initialisation échoue.</summary>
    internal static TCore InitializeCore<TCore, TConfiguration, TOption>(ref TCore? core, TConfiguration configuration, string sessionDirectory, string? saveDirectory) where TCore : class, IEmulationCoreRuntime<TConfiguration, TOption>
    {
        var createdCore = core ?? throw new InvalidOperationException();
        var initialized = false;
        try
        {
            createdCore.Initialize(configuration, sessionDirectory, saveDirectory);
            initialized = true;
            return createdCore;
        }
        finally
        {
            if (!initialized) DisposePreviousCore(ref core);
        }
    }

    /// <summary>Écrit les paramètres d’initialisation avec les codecs et les règles de chemins fournis.</summary>
    internal static void WriteInitializationRequest(ref BinaryWriter? writer, string corePath,
        Func<string?> session, Func<string?> saves, Action<BinaryWriter, string?> writeSaves,
        Func<string> serializeConfiguration)
    {
        writer!.Write(corePath);
        writer!.Write(session()!);
        writeSaves(writer!, saves());
        writer!.Write(serializeConfiguration());
    }

    /// <summary>Écrit les paramètres d’initialisation avec les codecs et les règles de chemins fournis.</summary>
    internal static void WriteInitializationRequest<TConfiguration>(BinaryWriter writer,
        string corePath, int emulator, string sessionDirectory, string? saveDirectory,
        TConfiguration configuration, JsonSerializerOptions jsonOptions)
    {
        writer.Write(corePath);
        writer.Write(emulator);
        writer.Write(Path.GetFullPath(sessionDirectory));
        WriteString(writer, saveDirectory is null ? null : Path.GetFullPath(saveDirectory));
        writer.Write(JsonSerializer.Serialize(configuration, jsonOptions));
    }

    /// <summary>Écrit les caractéristiques, options et diagnostics du cœur emprunté dans leur ordre commun.</summary>
    internal static void WriteCoreInitialization<TCore, TConfiguration, TOption>(BinaryWriter writer, TCore core,
        JsonSerializerOptions jsonOptions, ref int lastDiagnosticCount, char extensionSeparator,
        Action<BinaryWriter, TCore>? writeStatus = null)
        where TCore : IEmulationCoreRuntime<TConfiguration, TOption>
    {
        writer.Write(core.CoreSha256);
        writer.Write(core.FramesPerSecond);
        writer.Write(core.SampleRate);
        writeStatus?.Invoke(writer, core);
        writer.Write(JsonSerializer.Serialize(core.Options, jsonOptions));
        writer.Write(JsonSerializer.Serialize(core.Diagnostics, jsonOptions));
        lastDiagnosticCount = core.Diagnostics.Count;
        writer.Write(core.CoreName);
        writer.Write(core.CoreVersion);
        writer.Write(string.Join(extensionSeparator, core.SupportedContentExtensions.Order(StringComparer.OrdinalIgnoreCase)));
    }

    /// <summary>Applique les entrées, exécute une frame puis publie vidéo, audio et statuts dans leur ordre de protocole.</summary>
    internal static void RunCoreFrame<TCore, TConfiguration, TOption>(
        BinaryReader reader, BinaryWriter writer, TCore core,
        Func<BinaryReader, EmulationInputSnapshot> readInput, Action<BinaryWriter, VideoFrame?> writeFrame,
        Action<BinaryWriter, TCore> writeMediaStatus,
        Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates,
        Func<JsonSerializerOptions> jsonOptions, ref long lastVideoSequence, ref int lastDiagnosticCount, Action<BinaryWriter> writeSuccess)
        where TCore : IEmulationCoreRuntime<TConfiguration, TOption>
    {
        core.SetInput(readInput(reader));
        core.SetAudioBufferStatus(ReadAudioBufferStatus(reader));
        core.RunFrame();
        writeSuccess(writer);
        WriteNativeCoreFrameResponse<TCore, TConfiguration, TOption>(writer, core, writeFrame, writeLedStates, jsonOptions,
            ref lastVideoSequence, ref lastDiagnosticCount, writeMediaStatus: writeMediaStatus);
    }

    /// <summary>Écrit la réponse native de frame avec compteurs audio et callbacks de statut facultatifs.</summary>
    internal static void WriteNativeCoreFrameResponse<TCore, TConfiguration, TOption>(
        BinaryWriter writer, TCore core, Action<BinaryWriter, VideoFrame?> writeFrame,
        Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates,
        Func<JsonSerializerOptions> jsonOptions, ref long lastVideoSequence, ref int lastDiagnosticCount,
        Action<BinaryWriter, TCore>? writeStateSupport = null,
        Action<BinaryWriter, TCore>? writeMediaStatus = null)
        where TCore : IEmulationCoreRuntime<TConfiguration, TOption>
    {
        WriteFrameIfChanged(writer, core.LatestVideoFrame, ref lastVideoSequence, writeFrame);
        var bufferedAudioFrames = core.BufferedAudioFrames;
        var audioOverrunCount = core.AudioOverrunCount;
        var audioUnderrunCount = core.AudioUnderrunCount;
        WriteCoreAudio(writer, core);
        writer.Write(core.MinimumAudioLatencyMilliseconds);
        WriteRuntimeStatus(writer, core.Region, bufferedAudioFrames, audioOverrunCount, audioUnderrunCount);
        writeStateSupport?.Invoke(writer, core);
        writeMediaStatus?.Invoke(writer, core);
        var diagnostics = core.Diagnostics;
        WriteChangedDiagnostics(writer, () => diagnostics.Count,
            diagnosticWriter => diagnosticWriter.Write(JsonSerializer.Serialize(diagnostics, jsonOptions())), ref lastDiagnosticCount);
        writeLedStates(writer, core.LedStates);
    }

    /// <summary>Vide la file audio puis écrit cadence vidéo et fréquence audio.</summary>
    internal static void WriteCoreAudio<TConfiguration, TOption>(BinaryWriter writer,
        IEmulationCoreRuntime<TConfiguration, TOption> core)
    {
        WriteQueuedAudio(writer, () => core.TryDequeueAudio(out var chunk) ? chunk : null);
        writer.Write(core.FramesPerSecond);
        writer.Write(core.SampleRate);
    }

    /// <summary>Écrit le nombre d’images et la sélection courante du lecteur.</summary>
    internal static void WriteDiskSelection(BinaryWriter writer, int diskCount, int currentDiskIndex)
    {
        writer.Write(diskCount);
        writer.Write(currentDiskIndex);
    }

    /// <summary>Exécute les commandes communes de reset, arrêt, état, option et sélection de disque.</summary>
    internal static bool TryExecuteCoreStateCommand(byte command, BinaryReader reader, BinaryWriter writer,
        Func<IEmulationCoreStateCommands> ensureCore, Func<BinaryReader, byte[]> readBytes,
        Action<BinaryWriter>? writeSuccess = null)
    {
        switch (command)
        {
            case EmulationHostProtocolConstants.SaveMediaChangesCommand:
                var saveSlot = ReadMediaSlot(reader);
                ensureCore().SaveMediaChanges(saveSlot);
                break;
            case EmulationHostProtocolConstants.HasUnsavedMediaChangesCommand:
                var querySlot = ReadMediaSlot(reader);
                var hasChanges = ensureCore().HasUnsavedMediaChanges(querySlot);
                writeSuccess?.Invoke(writer);
                writer.Write(hasChanges);
                return true;
            case EmulationHostProtocolConstants.SoftResetCommand:
                ensureCore().SoftReset();
                break;
            case EmulationHostProtocolConstants.HardResetCommand:
                ensureCore().HardReset();
                break;
            case EmulationHostProtocolConstants.StopCommand:
                ensureCore().Stop();
                break;
            case EmulationHostProtocolConstants.SaveStateCommand:
                var state = ensureCore().SaveState();
                writeSuccess?.Invoke(writer);
                WriteBytes(writer, state);
                return true;
            case EmulationHostProtocolConstants.LoadStateCommand:
                ensureCore().LoadState(readBytes(reader));
                break;
            case EmulationHostProtocolConstants.SetOptionCommand:
                ensureCore().SetOption(reader.ReadString(), reader.ReadString());
                break;
            case EmulationHostProtocolConstants.SelectDiskCommand:
                ensureCore().SelectDisk(reader.ReadInt32());
                break;
            default:
                return false;
        }
        writeSuccess?.Invoke(writer);
        return true;
    }


    /// <summary>Écrit région vidéo et compteurs audio dans leur ordre commun.</summary>
    internal static void WriteRuntimeStatus(BinaryWriter writer, EmulationVideoStandard? region,
        int bufferedAudioFrames, long audioOverrunCount, long audioUnderrunCount)
    {
        writer.Write(RegionValue(region));
        writer.Write(bufferedAudioFrames);
        writer.Write(audioOverrunCount);
        writer.Write(audioUnderrunCount);
    }

    /// <summary>Lit région vidéo et compteurs audio en appelant les récepteurs fournis.</summary>
    internal static void ReadRuntimeStatus(BinaryReader reader, Action<int> setRegion,
        Action<int> setBufferedAudioFrames, Action<long> setAudioOverrunCount, Action<long> setAudioUnderrunCount)
    {
        setRegion(reader.ReadInt32());
        setBufferedAudioFrames(reader.ReadInt32());
        setAudioOverrunCount(reader.ReadInt64());
        setAudioUnderrunCount(reader.ReadInt64());
    }


    /// <summary>Transmet une image seulement lorsque sa séquence diffère de la dernière publiée.</summary>
    internal static void WriteFrameIfChanged(BinaryWriter writer, VideoFrame? frame,
        ref long lastVideoSequence, Action<BinaryWriter, VideoFrame?> writeFrame)
    {
        writeFrame(writer, frame?.Sequence == lastVideoSequence ? null : frame);
        if (frame is not null) lastVideoSequence = frame.Sequence;
    }

    /// <summary>Vide séquentiellement les blocs audio disponibles dans une réponse.</summary>
    internal static void WriteQueuedAudio(BinaryWriter writer, Func<AudioChunk?> dequeue)
    {
        var audio = new List<AudioChunk>();
        while (dequeue() is { } chunk) audio.Add(chunk);
        WriteAudio(writer, audio);
    }

    /// <summary>Écrit les diagnostics seulement lorsque leur nombre a changé.</summary>
    internal static void WriteChangedDiagnostics(BinaryWriter writer, Func<int> count,
        Action<BinaryWriter> writeDiagnostics, ref int lastDiagnosticCount)
    {
        var diagnosticsChanged = count() != lastDiagnosticCount;
        writer.Write(diagnosticsChanged);
        if (diagnosticsChanged)
        {
            writeDiagnostics(writer);
            lastDiagnosticCount = count();
        }
    }


    // The caller owns the core and codecs. Unknown commands must not acquire a core
    // or consume a payload; initialization, frames and disposal stay in their host.
    /// <summary>Décode les commandes typées communes sans acquérir de cœur ni lire de charge utile pour une commande inconnue.</summary>
    internal static bool TryExecuteCoreCommand<TMedia, TPeripheral>(
        byte command, BinaryReader reader, BinaryWriter writer,
        Func<IEmulationCoreCommands<TMedia, TPeripheral>> ensureCore,
        Func<BinaryReader, TMedia> readMedia, Func<BinaryReader, byte[]> readBytes,
        Action<BinaryWriter, DiskStatus> writeDiskStatus, Func<int, TPeripheral> decodePeripheral, Action<BinaryWriter>? writeSuccess = null)
    {
        switch (command)
        {
            case EmulationHostProtocolConstants.InsertMediaCommand:
                ensureCore().InsertMedia(readMedia(reader));
                break;
            case EmulationHostProtocolConstants.EjectMediaCommand:
                ensureCore().EjectMedia(ReadMediaSlot(reader));
                break;
            case EmulationHostProtocolConstants.GetDiskStatusCommand:
                var status = ensureCore().GetDiskStatus();
                writeSuccess?.Invoke(writer);
                writeDiskStatus(writer, status);
                return true;
            case EmulationHostProtocolConstants.SetControllerPortDeviceCommand:
                ensureCore().SetControllerPortDevice(reader.ReadInt32(), decodePeripheral(reader.ReadInt32()));
                break;
            default:
                return TryExecuteCoreStateCommand(command, reader, writer, ensureCore, readBytes, writeSuccess);
        }
        writeSuccess?.Invoke(writer);
        return true;
    }

    /// <summary>Lit l’état du processus emprunté sans le détruire ni changer son propriétaire.</summary>
    internal static HostProcessState GetProcessState(Process? process, bool connectionFailed, bool disposed, int? hostProcessId = null)
    {
        if (connectionFailed) return HostProcessState.Faulted;
        if (process is null) return hostProcessId.HasValue ? HostProcessState.Exited : HostProcessState.NotStarted;
        if (disposed) return HostProcessState.Exited;
        try { return process.HasExited ? HostProcessState.Exited : HostProcessState.Running; }
        catch (InvalidOperationException) { return HostProcessState.Exited; }
    }
    /// <summary>Convertit la région native en standard vidéo connu ou null.</summary>
    internal static EmulationVideoStandard? Region(uint nativeRegion) => nativeRegion switch
    {
        0 => EmulationVideoStandard.Ntsc,
        1 => EmulationVideoStandard.Pal,
        _ => null
    };

    /// <summary>Convertit le standard vidéo facultatif dans sa représentation de protocole.</summary>
    internal static int RegionValue(EmulationVideoStandard? region) => region is null ? -1 : (int)region.Value;

    /// <summary>Lit le standard vidéo avec la politique de valeurs autorisées fournie.</summary>
    internal static EmulationVideoStandard? ReadRegion(int value, bool allowAllDefinedStandards) =>
        allowAllDefinedStandards
            ? Enum.IsDefined(typeof(EmulationVideoStandard), value) ? (EmulationVideoStandard)value : null
            : value is (int)EmulationVideoStandard.Ntsc or (int)EmulationVideoStandard.Pal
                ? (EmulationVideoStandard)value : null;

    /// <summary>Possède et libère le pipe client et ses codecs autour de la session fournie.</summary>
    internal static void RunConnectedSession(string pipeName, string serverName, int connectionTimeoutMilliseconds,
        Action<BinaryReader, BinaryWriter> run)
    {
        using var pipe = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.None);
        pipe.Connect(connectionTimeoutMilliseconds);
        using var reader = new BinaryReader(pipe, Encoding.UTF8, leaveOpen: true);
        using var transportWriter = new BinaryWriter(pipe, Encoding.UTF8, leaveOpen: true);
        run(reader, transportWriter);
    }

    /// <summary>Exécute les commandes jusqu’à la fermeture du flux ou à la commande de destruction.</summary>
    internal static void RunCommandLoop<TCommand>(BinaryReader reader, BinaryWriter transportWriter,
        Func<BinaryReader, TCommand> readCommand, TCommand disposeCommand,
        Action<TCommand, BinaryWriter> execute, Action<BinaryWriter, Exception> writeFailure)
        where TCommand : struct, Enum
    {
        while (true)
        {
            TCommand command;
            try { command = readCommand(reader); }
            catch (EndOfStreamException) { break; }
            var exit = EqualityComparer<TCommand>.Default.Equals(command, disposeCommand);
            WriteResponse(transportWriter, writer => execute(command, writer), writeFailure);
            if (exit) break;
        }
    }
    /// <summary>Lit le codec de remplissage présent après les entrées de chaque requête de frame.</summary>
    internal static AudioOutputBufferStatus? ReadAudioBufferStatus(BinaryReader reader) => reader.ReadBoolean() ? new AudioOutputBufferStatus(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean()) : null;

    /// <summary>Écrit un instantané emprunté sans conserver ni posséder la sortie audio.</summary>
    internal static void WriteAudioBufferStatus(BinaryWriter writer, AudioOutputBufferStatus? status)
    {
        writer.Write(status.HasValue);
        if (status is not { } value) return;
        writer.Write(value.SampleRate);
        writer.Write(value.BufferedFrames);
        writer.Write(value.CapacityFrames);
        writer.Write(value.IsActive);
    }
    /// <summary>Écrit l’identité logique complète, sans la réduire à un identifiant natif de périphérique.</summary>
    internal static void WriteMediaSlot(BinaryWriter writer, EmulationMediaSlot slot)
    {
        ValidateMediaSlot(slot);
        writer.Write((int)slot.Category);
        writer.Write(slot.Index);
    }
    /// <summary>Lit les deux composantes du lecteur et rejette les valeurs invalides avant son utilisation.</summary>
    internal static EmulationMediaSlot ReadMediaSlot(BinaryReader reader)
    {
        var slot = new EmulationMediaSlot((EmulationMediaCategory)reader.ReadInt32(), reader.ReadInt32());
        ValidateMediaSlot(slot);
        return slot;
    }
    /// <summary>Contrôle uniquement le contrat de transport ; le backend conserve la validation de ses lecteurs.</summary>
    private static void ValidateMediaSlot(EmulationMediaSlot slot)
    {
        if (!Enum.IsDefined(slot.Category) || slot.Index < 0)
            throw new InvalidDataException(EmulationHostProtocolConstants.InvalidMediaSlot);
    }
}
