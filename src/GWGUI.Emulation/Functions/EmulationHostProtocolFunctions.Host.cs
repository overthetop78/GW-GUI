using System.IO;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace GWGUI.Emulation.Functions;

internal static partial class EmulationHostProtocolFunctions
{
    internal static void WriteRuntimeStatus(BinaryWriter writer, EmulationVideoStandard? region,
        int bufferedAudioFrames, long audioOverrunCount, long audioUnderrunCount)
    {
        writer.Write(RegionValue(region));
        writer.Write(bufferedAudioFrames);
        writer.Write(audioOverrunCount);
        writer.Write(audioUnderrunCount);
    }

    internal static void ReadRuntimeStatus(BinaryReader reader, Action<int> setRegion,
        Action<int> setBufferedAudioFrames, Action<long> setAudioOverrunCount, Action<long> setAudioUnderrunCount)
    {
        setRegion(reader.ReadInt32());
        setBufferedAudioFrames(reader.ReadInt32());
        setAudioOverrunCount(reader.ReadInt64());
        setAudioUnderrunCount(reader.ReadInt64());
    }


    internal static void WriteFrameIfChanged(BinaryWriter writer, VideoFrame? frame,
        ref long lastVideoSequence, Action<BinaryWriter, VideoFrame?> writeFrame)
    {
        writeFrame(writer, frame?.Sequence == lastVideoSequence ? null : frame);
        if (frame is not null) lastVideoSequence = frame.Sequence;
    }

    internal static void WriteQueuedAudio(BinaryWriter writer, Func<AudioChunk?> dequeue)
    {
        var audio = new List<AudioChunk>();
        while (dequeue() is { } chunk) audio.Add(chunk);
        WriteAudio(writer, audio);
    }

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
    internal static bool TryExecuteCoreCommand<TMedia, TPeripheral, TDiskStatus>(
        byte command, BinaryReader reader, BinaryWriter writer,
        Func<IEmulationCoreCommands<TMedia, TPeripheral, TDiskStatus>> ensureCore,
        Func<BinaryReader, TMedia> readMedia, Func<BinaryReader, byte[]> readBytes,
        Action<BinaryWriter, TDiskStatus> writeDiskStatus, Func<int, TPeripheral> decodePeripheral)
    {
        switch (command)
        {
            case EmulationHostProtocolConstants.HardResetCommand:
                ensureCore().HardReset();
                break;
            case EmulationHostProtocolConstants.StopCommand:
                ensureCore().Stop();
                break;
            case EmulationHostProtocolConstants.InsertMediaCommand:
                ensureCore().InsertMedia(readMedia(reader));
                break;
            case EmulationHostProtocolConstants.EjectMediaCommand:
                ensureCore().EjectMedia(EmulationMediaSlot.FromProtocolValue(reader.ReadInt32()));
                break;
            case EmulationHostProtocolConstants.SaveStateCommand:
                WriteBytes(writer, ensureCore().SaveState());
                break;
            case EmulationHostProtocolConstants.LoadStateCommand:
                ensureCore().LoadState(readBytes(reader));
                break;
            case EmulationHostProtocolConstants.SetOptionCommand:
                ensureCore().SetOption(reader.ReadString(), reader.ReadString());
                break;
            case EmulationHostProtocolConstants.SelectDiskCommand:
                ensureCore().SelectDisk(reader.ReadInt32());
                break;
            case EmulationHostProtocolConstants.GetDiskStatusCommand:
                writeDiskStatus(writer, ensureCore().GetDiskStatus());
                break;
            case EmulationHostProtocolConstants.SetControllerPortDeviceCommand:
                ensureCore().SetControllerPortDevice(reader.ReadInt32(), decodePeripheral(reader.ReadInt32()));
                break;
            default:
                return false;
        }
        return true;
    }

    internal static TState GetProcessState<TState>(Process? process, bool connectionFailed, bool disposed,
        TState faulted, TState notStarted, TState exited, TState running)
        where TState : struct, Enum
    {
        if (connectionFailed) return faulted;
        if (process is null) return notStarted;
        if (disposed) return exited;
        try { return process.HasExited ? exited : running; }
        catch (InvalidOperationException) { return exited; }
    }

    internal static EmulationVideoStandard? Region(uint nativeRegion) => nativeRegion switch
    {
        0 => EmulationVideoStandard.Ntsc,
        1 => EmulationVideoStandard.Pal,
        _ => null
    };

    internal static int RegionValue(EmulationVideoStandard? region) => region is null ? -1 : (int)region.Value;

    internal static EmulationVideoStandard? ReadRegion(int value, bool allowAllDefinedStandards) =>
        allowAllDefinedStandards
            ? Enum.IsDefined(typeof(EmulationVideoStandard), value) ? (EmulationVideoStandard)value : null
            : value is (int)EmulationVideoStandard.Ntsc or (int)EmulationVideoStandard.Pal
                ? (EmulationVideoStandard)value : null;

    internal static void RunConnectedSession(string pipeName, string serverName, int connectionTimeoutMilliseconds,
        Action<BinaryReader, BinaryWriter> run)
    {
        using var pipe = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.None);
        pipe.Connect(connectionTimeoutMilliseconds);
        using var reader = new BinaryReader(pipe, Encoding.UTF8, leaveOpen: true);
        using var transportWriter = new BinaryWriter(pipe, Encoding.UTF8, leaveOpen: true);
        run(reader, transportWriter);
    }

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
}
