using System.IO;
using GWGUI.Emulation.Contracts;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Services;

/// <summary>Relie le contrat public aux traitements binaires internes existants du SDK.</summary>
/// <remarks>Ce service ne conserve aucun état de session et ne modifie ni l’ordre des champs ni les règles de transport.</remarks>
internal sealed class EmulationHostProtocolService : IEmulationHostProtocol
{
    /// <inheritdoc/>
    public void WriteControllerFeedback(BinaryWriter writer, IReadOnlyList<ControllerFeedbackState> states) => EmulationHostProtocolFunctions.WriteControllerFeedback(writer, states);
    /// <inheritdoc/>
    public IReadOnlyList<ControllerFeedbackState> ReadControllerFeedback(BinaryReader reader) => EmulationHostProtocolFunctions.ReadControllerFeedback(reader);

    /// <inheritdoc/>
    public int VideoSlotCapacity => EmulationHostProtocolConstants.VideoSlotCapacity;

    /// <inheritdoc/>
    public long VideoMapCapacity => EmulationHostProtocolConstants.VideoMapCapacity;

    /// <inheritdoc/>
    public int MaximumLedStateCount => EmulationHostProtocolConstants.MaximumLedStateCount;

    /// <inheritdoc/>
    public void WriteString(BinaryWriter writer, string? value)
        => EmulationHostProtocolFunctions.WriteString(writer, value);

    /// <inheritdoc/>
    public void DisposeTransportIgnoringErrors(IDisposable? value)
        => EmulationHostProtocolFunctions.DisposeTransportIgnoringErrors(value);

    /// <inheritdoc/>
    public void WriteSharedFrame(BinaryWriter writer, VideoFrame? frame, MemoryMappedViewAccessor videoMap, string hostName)
        => EmulationHostProtocolFunctions.WriteSharedFrame(writer, frame, videoMap, hostName);

    /// <inheritdoc/>
    public VideoFrame? ReadSharedFrame(BinaryReader reader, MemoryMappedViewAccessor videoMap, string hostName)
        => EmulationHostProtocolFunctions.ReadSharedFrame(reader, videoMap, hostName);

    /// <inheritdoc/>
    public void InitializeCore<TCore, TEmulator, TConfiguration, TOption>(BinaryReader reader, BinaryWriter writer, Func<string, TEmulator, TCore> createCore,
        ref TCore? core, ref int lastDiagnosticCount, Func<BinaryReader, TEmulator> readEmulator, Func<BinaryReader, string?> readSaves,
        Func<BinaryReader, TConfiguration> readConfiguration, Func<JsonSerializerOptions> jsonOptions, char extensionSeparator,
        Action<BinaryWriter, TCore> writeStatus, Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates)
        where TCore : class, IEmulationCoreRuntime<TConfiguration, TOption>
        => EmulationHostProtocolFunctions.InitializeCore<TCore, TEmulator, TConfiguration, TOption>(reader, writer, createCore, ref core, ref lastDiagnosticCount, readEmulator, readSaves, readConfiguration, jsonOptions, extensionSeparator, writeStatus, writeLedStates);

    /// <inheritdoc/>
    public void RunCoreFrame<TCore, TConfiguration, TOption>(BinaryReader reader, BinaryWriter writer, TCore core,
        Func<BinaryReader, EmulationInputSnapshot> readInput, Action<BinaryWriter, VideoFrame?> writeFrame, Action<BinaryWriter, TCore> writeMediaStatus,
        Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates, Func<JsonSerializerOptions> jsonOptions,
        ref long lastVideoSequence, ref int lastDiagnosticCount) where TCore : IEmulationCoreRuntime<TConfiguration, TOption>
        => EmulationHostProtocolFunctions.RunCoreFrame<TCore, TConfiguration, TOption>(reader, writer, core, readInput, writeFrame, writeMediaStatus, writeLedStates, jsonOptions, ref lastVideoSequence, ref lastDiagnosticCount);

    /// <inheritdoc/>
    public void WriteDiskSelection(BinaryWriter writer, int diskCount, int currentDiskIndex)
        => EmulationHostProtocolFunctions.WriteDiskSelection(writer, diskCount, currentDiskIndex);

    /// <inheritdoc/>
    public EmulationVideoStandard? Region(uint nativeRegion)
        => EmulationHostProtocolFunctions.Region(nativeRegion);

    /// <inheritdoc/>
    [SupportedOSPlatform("windows")]
    public void WithSharedVideoMap(string videoMapName, long viewOffset, Action<MemoryMappedViewAccessor> run)
        => EmulationHostProtocolFunctions.WithSharedVideoMap(videoMapName, viewOffset, run);

    /// <inheritdoc/>
    [SupportedOSPlatform("windows")]
    public void WithSuppressedErrorDialogs(Action run)
        => EmulationHostProtocolFunctions.WithSuppressedErrorDialogs(run);
    /// <inheritdoc/>
    public EmulationVideoStandard? ReadRegion(int value, bool allowAllDefinedStandards) => EmulationHostProtocolFunctions.ReadRegion(value, allowAllDefinedStandards);

    /// <inheritdoc/>
    public HostProcessState GetProcessState(Process? process, bool connectionFailed, bool disposed, int? hostProcessId = null)
        => EmulationHostProtocolFunctions.GetProcessState(process, connectionFailed, disposed, hostProcessId);

    /// <inheritdoc/>
    public Process StartHostProcess(ProcessStartInfo startInfo, Func<Exception> startFailed) => EmulationChildProcessLifetime.Start(startInfo, startFailed);

    /// <inheritdoc/>
    public void WaitForHostExit(Process process, bool graceful, int gracefulTimeoutMilliseconds) => EmulationChildProcessLifetime.WaitForExit(process, graceful, gracefulTimeoutMilliseconds);

    /// <inheritdoc/>
    public void ExecuteRequest(SemaphoreSlim gate, Func<BinaryWriter> getWriter, Func<byte[]> getResponse, Action<BinaryWriter> write, Action<BinaryReader> read, Action<Exception> failConnection)
        => EmulationHostProtocolFunctions.ExecuteRequest(gate, getWriter, getResponse, write, read, failConnection);

    /// <inheritdoc/>
    public Task<byte[]> ReadResponseAsync(Stream pipe, TimeSpan responseTimeout, CancellationToken cancellationToken, Func<int, string> invalidLength)
        => EmulationHostProtocolFunctions.ReadResponseAsync(pipe, responseTimeout, cancellationToken, invalidLength);

    /// <inheritdoc/>
    public string? ReadString(BinaryReader reader)
        => EmulationHostProtocolFunctions.ReadString(reader);

    /// <inheritdoc/>
    public void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> bytes)
        => EmulationHostProtocolFunctions.WriteBytes(writer, bytes);

    /// <inheritdoc/>
    public byte[] ReadBytes(BinaryReader reader, string hostName)
        => EmulationHostProtocolFunctions.ReadBytes(reader, hostName);

    /// <inheritdoc/>
    public void WriteInput(BinaryWriter writer, EmulationInputSnapshot input)
        => EmulationHostProtocolFunctions.WriteInput(writer, input);

    /// <inheritdoc/>
    public EmulationInputSnapshot ReadInput(BinaryReader reader, string hostName)
        => EmulationHostProtocolFunctions.ReadInput(reader, hostName);

    /// <inheritdoc/>
    public IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader, string hostName)
        => EmulationHostProtocolFunctions.ReadAudio(reader, hostName);

    /// <inheritdoc/>
    public void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states)
        => EmulationHostProtocolFunctions.WriteLedStates(writer, states);

    /// <inheritdoc/>
    public IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader, string hostName)
        => EmulationHostProtocolFunctions.ReadLedStates(reader, hostName);

    /// <inheritdoc/>
    public ProcessStartInfo CreateHostStartInfo(string executablePath, string workingDirectory, string hostCommand, string pipeName, string videoMapName)
        => EmulationHostProtocolFunctions.CreateHostStartInfo(executablePath, workingDirectory, hostCommand, pipeName, videoMapName);

    /// <inheritdoc/>
    public void WaitForConnection(NamedPipeServerStream pipe, TimeSpan connectionTimeout, CancellationToken cancellationToken)
        => EmulationHostProtocolFunctions.WaitForConnection(pipe, connectionTimeout, cancellationToken);

    /// <inheritdoc/>
    public void WaitForConnection(NamedPipeServerStream pipe, TimeSpan connectionTimeout)
        => EmulationHostProtocolFunctions.WaitForConnection(pipe, connectionTimeout);

    /// <inheritdoc/>
    public void DisposeTransport(IDisposable? resource)
        => EmulationHostProtocolFunctions.DisposeTransport(resource);

    /// <inheritdoc/>
    public void DisposePreviousCore<TCore>(ref TCore? core) where TCore : class, IDisposable
        => EmulationHostProtocolFunctions.DisposePreviousCore<TCore>(ref core);

    /// <inheritdoc/>
    public (string CorePath, string Session, string? Saves, TConfiguration Configuration) ReadInitializationRequest<TConfiguration>(BinaryReader reader, Func<BinaryReader, string?> readSaves, Func<BinaryReader, TConfiguration> readConfiguration)
        => EmulationHostProtocolFunctions.ReadInitializationRequest<TConfiguration>(reader, readSaves, readConfiguration);

    /// <inheritdoc/>
    public (string CorePath, TEmulator Emulator, string Session, string? Saves, TConfiguration Configuration) ReadInitializationRequest<TEmulator, TConfiguration>(BinaryReader reader, Func<BinaryReader, TEmulator> readEmulator, Func<BinaryReader, string?> readSaves, Func<BinaryReader, TConfiguration> readConfiguration)
        => EmulationHostProtocolFunctions.ReadInitializationRequest<TEmulator, TConfiguration>(reader, readEmulator, readSaves, readConfiguration);

    /// <inheritdoc/>
    public void WriteInitializationRequest(ref BinaryWriter? writer, string corePath, Func<string?> session, Func<string?> saves, Action<BinaryWriter, string?> writeSaves, Func<string> serializeConfiguration)
        => EmulationHostProtocolFunctions.WriteInitializationRequest(ref writer, corePath, session, saves, writeSaves, serializeConfiguration);

    /// <inheritdoc/>
    public void WriteInitializationRequest<TConfiguration>(BinaryWriter writer, string corePath, int emulator, string sessionDirectory, string? saveDirectory, TConfiguration configuration, JsonSerializerOptions jsonOptions)
        => EmulationHostProtocolFunctions.WriteInitializationRequest<TConfiguration>(writer, corePath, emulator, sessionDirectory, saveDirectory, configuration, jsonOptions);

    /// <inheritdoc/>
    public void WriteCoreInitialization<TCore, TConfiguration, TOption>(BinaryWriter writer, TCore core, JsonSerializerOptions jsonOptions, ref int lastDiagnosticCount, char extensionSeparator, Action<BinaryWriter, TCore>? writeStatus = null) where TCore : IEmulationCoreRuntime<TConfiguration, TOption>
        => EmulationHostProtocolFunctions.WriteCoreInitialization<TCore, TConfiguration, TOption>(writer, core, jsonOptions, ref lastDiagnosticCount, extensionSeparator, writeStatus);

    /// <inheritdoc/>
    public void WriteNativeCoreFrameResponse<TCore, TConfiguration, TOption>( BinaryWriter writer, TCore core, Action<BinaryWriter, VideoFrame?> writeFrame, Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates, Func<JsonSerializerOptions> jsonOptions, ref long lastVideoSequence, ref int lastDiagnosticCount, Action<BinaryWriter, TCore>? writeStateSupport = null, Action<BinaryWriter, TCore>? writeMediaStatus = null) where TCore : IEmulationCoreRuntime<TConfiguration, TOption>
        => EmulationHostProtocolFunctions.WriteNativeCoreFrameResponse<TCore, TConfiguration, TOption>(writer, core, writeFrame, writeLedStates, jsonOptions, ref lastVideoSequence, ref lastDiagnosticCount, writeStateSupport, writeMediaStatus);

    /// <inheritdoc/>
    public void WriteRuntimeStatus(BinaryWriter writer, EmulationVideoStandard? region, int bufferedAudioFrames, long audioOverrunCount, long audioUnderrunCount)
        => EmulationHostProtocolFunctions.WriteRuntimeStatus(writer, region, bufferedAudioFrames, audioOverrunCount, audioUnderrunCount);

    /// <inheritdoc/>
    public void ReadRuntimeStatus(BinaryReader reader, Action<int> setRegion, Action<int> setBufferedAudioFrames, Action<long> setAudioOverrunCount, Action<long> setAudioUnderrunCount)
        => EmulationHostProtocolFunctions.ReadRuntimeStatus(reader, setRegion, setBufferedAudioFrames, setAudioOverrunCount, setAudioUnderrunCount);

    /// <inheritdoc/>
    public bool TryExecuteCoreCommand<TMedia, TPeripheral>( byte command, BinaryReader reader, BinaryWriter writer, Func<IEmulationCoreCommands<TMedia, TPeripheral>> ensureCore, Func<BinaryReader, TMedia> readMedia, Func<BinaryReader, byte[]> readBytes, Action<BinaryWriter, DiskStatus> writeDiskStatus, Func<int, TPeripheral> decodePeripheral, Action<BinaryWriter>? writeSuccess = null)
        => EmulationHostProtocolFunctions.TryExecuteCoreCommand<TMedia, TPeripheral>(command, reader, writer, ensureCore, readMedia, readBytes, writeDiskStatus, decodePeripheral, writeSuccess);

    /// <inheritdoc/>
    public void RunConnectedSession(string pipeName, string serverName, int connectionTimeoutMilliseconds, Action<BinaryReader, BinaryWriter> run)
        => EmulationHostProtocolFunctions.RunConnectedSession(pipeName, serverName, connectionTimeoutMilliseconds, run);

    /// <inheritdoc/>
    public void RunCommandLoop<TCommand>(BinaryReader reader, BinaryWriter transportWriter, Func<BinaryReader, TCommand> readCommand, TCommand disposeCommand, Action<TCommand, BinaryWriter> execute, Action<BinaryWriter, Exception> writeFailure) where TCommand : struct, Enum
        => EmulationHostProtocolFunctions.RunCommandLoop<TCommand>(reader, transportWriter, readCommand, disposeCommand, execute, writeFailure);

    /// <inheritdoc/>
    public int CalculateVideoSlotCapacity(int frameLength, int minimumSlotCapacity, string hostName)
        => EmulationHostProtocolFunctions.CalculateVideoSlotCapacity(frameLength, minimumSlotCapacity, hostName);

    /// <inheritdoc/>
    [SupportedOSPlatform("windows")]
    public void WriteResizableSharedFrame(BinaryWriter writer, VideoFrame? frame, IEmulationSharedVideoWriter video)
        => EmulationHostProtocolFunctions.WriteResizableSharedFrame(writer, frame, video);

    /// <inheritdoc/>
    [SupportedOSPlatform("windows")]
    public VideoFrame? ReadResizableSharedFrame(BinaryReader reader, ref MemoryMappedFile? memory, ref MemoryMappedViewAccessor? view, ref string? activeName, int minimumSlotCapacity)
        => EmulationHostProtocolFunctions.ReadResizableSharedFrame(reader, ref memory, ref view, ref activeName, minimumSlotCapacity);
    /// <inheritdoc/>
    public AudioOutputBufferStatus? ReadAudioBufferStatus(BinaryReader reader) => EmulationHostProtocolFunctions.ReadAudioBufferStatus(reader);
    /// <inheritdoc/>
    public void WriteAudioBufferStatus(BinaryWriter writer, AudioOutputBufferStatus? status) => EmulationHostProtocolFunctions.WriteAudioBufferStatus(writer, status);
    /// <inheritdoc/>
    public void WriteMediaSlot(BinaryWriter writer, EmulationMediaSlot slot) => EmulationHostProtocolFunctions.WriteMediaSlot(writer, slot);
    /// <inheritdoc/>
    public EmulationMediaSlot ReadMediaSlot(BinaryReader reader) => EmulationHostProtocolFunctions.ReadMediaSlot(reader);
}
