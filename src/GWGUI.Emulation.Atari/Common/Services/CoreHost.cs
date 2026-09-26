using System.Globalization;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

public static class CoreHost
{
    [SupportedOSPlatform(CoreHostValues.Windows)]
    public static void Run(string pipeName, string videoMapName)
    {
        using var video = new SharedVideoWriter(videoMapName);
        using var pipe = new NamedPipeClientStream(CoreHostConstants.LocalPipeServerName,
            pipeName, PipeDirection.InOut, PipeOptions.None);
        pipe.Connect(CoreHostConstants.ConnectionTimeoutMilliseconds);
        using var reader = new BinaryReader(pipe, Encoding.UTF8, leaveOpen: true);
        using var transportWriter = new BinaryWriter(pipe, Encoding.UTF8, leaveOpen: true);
        ExternalCore? core = null;
        var lastVideoSequence = CoreHostConstants.InitialVideoSequence;
        var lastDiagnosticCount = CoreHostConstants.InitialDiagnosticCount;
        try
        {
            while (TryReadCommand(reader, out var command))
            {
                var exit = command == HostCommand.Dispose;
                using var responseStream = new MemoryStream();
                using var writer = new BinaryWriter(responseStream, Encoding.UTF8, leaveOpen: true);
                try
                {
                    CoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Success);
                    switch (command)
                    {
                        case HostCommand.Initialize:
                            Initialize(reader, writer, ref core, ref lastDiagnosticCount);
                            break;
                        case HostCommand.RunFrame:
                            RunFrame(reader, writer, video, EnsureCore(core), ref lastVideoSequence,
                                ref lastDiagnosticCount);
                            break;
                        case HostCommand.HardReset:
                            EnsureCore(core).HardReset();
                            break;
                        case HostCommand.Stop:
                            EnsureCore(core).Stop();
                            break;
                        case HostCommand.InsertMedia:
                            EnsureCore(core).InsertMedia(ReadMedia(reader));
                            break;
                        case HostCommand.EjectMedia:
                            EnsureCore(core).EjectMedia(EmulationMediaSlot.FromProtocolValue(reader.ReadInt32()));
                            break;
                        case HostCommand.SaveState:
                            CoreHostFunctions.WriteBytes(writer, EnsureCore(core).SaveState());
                            break;
                        case HostCommand.LoadState:
                            EnsureCore(core).LoadState(CoreHostFunctions.ReadBytes(reader));
                            break;
                        case HostCommand.SetOption:
                            EnsureCore(core).SetOption(reader.ReadString(), reader.ReadString());
                            break;
                        case HostCommand.SelectDisk:
                            EnsureCore(core).SelectDisk(reader.ReadInt32());
                            break;
                        case HostCommand.SaveMediaChanges:
                            EnsureCore(core).SaveMediaChanges(EmulationMediaSlot.FromProtocolValue(reader.ReadInt32()));
                            break;
                        case HostCommand.GetDiskStatus:
                            CoreHostFunctions.WriteDiskStatus(writer, EnsureCore(core).GetDiskStatus());
                            break;
                        case HostCommand.HasUnsavedMediaChanges:
                            writer.Write(EnsureCore(core).HasUnsavedMediaChanges(
                                EmulationMediaSlot.FromProtocolValue(reader.ReadInt32())));
                            break;
                        case HostCommand.SetControllerPortDevice:
                            EnsureCore(core).SetControllerPortDevice(reader.ReadInt32(),
                                (PeripheralCategory)reader.ReadInt32());
                            break;
                        case HostCommand.Dispose:
                            core?.Dispose();
                            core = null;
                            break;
                        default:
                            throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
                                CoreHostErrors.UnknownCommandFormat, (byte)command));
                    }
                }
                catch (Exception error)
                {
                    responseStream.SetLength(0);
                    responseStream.Position = BufferConstants.FirstBufferIndex;
                    CoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Failure);
                    CoreHostFunctions.WriteError(writer, error);
                }
                writer.Flush();
                CoreHostFunctions.WriteBytes(transportWriter,
                    responseStream.GetBuffer().AsSpan(BufferConstants.FirstBufferIndex,
                        checked((int)responseStream.Length)));
                if (exit) break;
            }
        }
        finally
        {
            core?.Dispose();
        }
    }

    private static bool TryReadCommand(BinaryReader reader, out HostCommand command)
    {
        try
        {
            command = CoreHostFunctions.ReadRequestHeader(reader);
            return true;
        }
        catch (EndOfStreamException)
        {
            command = default;
            return false;
        }
    }

    private static void Initialize(BinaryReader reader, BinaryWriter writer, ref ExternalCore? core,
        ref int lastDiagnosticCount)
    {
        core?.Dispose();
        var corePath = reader.ReadString();
        var emulator = (Emulator)reader.ReadInt32();
        var session = reader.ReadString();
        var saves = CoreHostFunctions.ReadString(reader);
        var configuration = JsonSerializer.Deserialize<MachineConfiguration>(reader.ReadString(),
            CoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.InvalidConfiguration);
        core = new ExternalCore(corePath, emulator);
        core.Initialize(configuration, session, saves);
        writer.Write(core.CoreSha256);
        writer.Write(core.FramesPerSecond);
        writer.Write(core.SampleRate);
        writer.Write(core.SupportsSaveStates);
        WriteRuntimeStatus(writer, core.Region, core.BufferedAudioFrames, core.AudioOverrunCount,
            core.AudioUnderrunCount);
        writer.Write(JsonSerializer.Serialize(core.Options, CoreHostFunctions.JsonOptions));
        writer.Write(JsonSerializer.Serialize(core.Diagnostics, CoreHostFunctions.JsonOptions));
        lastDiagnosticCount = core.Diagnostics.Count;
        writer.Write(core.CoreName);
        writer.Write(core.CoreVersion);
        writer.Write(string.Join(CoreHostConstants.ExtensionListSeparator,
            core.SupportedContentExtensions.Order(StringComparer.OrdinalIgnoreCase)));
        CoreHostFunctions.WriteLedStates(writer, core.LedStates);
    }

    [SupportedOSPlatform(CoreHostValues.Windows)]
    private static void RunFrame(BinaryReader reader, BinaryWriter writer, SharedVideoWriter video,
        ExternalCore core, ref long lastVideoSequence, ref int lastDiagnosticCount)
    {
        core.SetInput(CoreHostFunctions.ReadInput(reader));
        core.RunFrame();
        var frame = core.LatestVideoFrame;
        CoreHostFunctions.WriteResizableSharedFrame(writer,
            frame?.Sequence == lastVideoSequence ? null : frame, video);
        if (frame is not null) lastVideoSequence = frame.Sequence;
        var bufferedAudioFrames = core.BufferedAudioFrames;
        var audioOverrunCount = core.AudioOverrunCount;
        var audioUnderrunCount = core.AudioUnderrunCount;
        var audio = new List<AudioChunk>();
        while (core.TryDequeueAudio(out var chunk) && chunk is not null) audio.Add(chunk);
        CoreHostFunctions.WriteAudio(writer, audio);
        writer.Write(core.FramesPerSecond);
        writer.Write(core.SampleRate);
        WriteRuntimeStatus(writer, core.Region, bufferedAudioFrames, audioOverrunCount, audioUnderrunCount);
        var diagnosticsChanged = core.Diagnostics.Count != lastDiagnosticCount;
        writer.Write(diagnosticsChanged);
        if (diagnosticsChanged)
        {
            writer.Write(JsonSerializer.Serialize(core.Diagnostics, CoreHostFunctions.JsonOptions));
            lastDiagnosticCount = core.Diagnostics.Count;
        }
        CoreHostFunctions.WriteLedStates(writer, core.LedStates);
    }

    private static void WriteRuntimeStatus(BinaryWriter writer, RuntimeRegion? region, int bufferedAudioFrames,
        long audioOverrunCount, long audioUnderrunCount)
    {
        writer.Write(RuntimeFunctions.RegionValue(region));
        writer.Write(bufferedAudioFrames);
        writer.Write(audioOverrunCount);
        writer.Write(audioUnderrunCount);
    }

    private static MediaConfiguration ReadMedia(BinaryReader reader) =>
        JsonSerializer.Deserialize<MediaConfiguration>(reader.ReadString(), CoreHostFunctions.JsonOptions)
        ?? throw new InvalidDataException(CoreHostErrors.InvalidConfiguration);

    private static ExternalCore EnsureCore(ExternalCore? core) => core ??
        throw new InvalidOperationException(CoreHostErrors.NotInitialized);
}
