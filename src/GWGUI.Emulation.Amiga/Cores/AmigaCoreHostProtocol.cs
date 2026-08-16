using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Amiga.Cores;

internal enum AmigaHostCommand : byte
{
    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose
}

internal static class AmigaCoreHostProtocol
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal static void WriteString(BinaryWriter writer, string? value) => EmulationHostProtocolFunctions.WriteString(writer, value);
    internal static string? ReadString(BinaryReader reader) => EmulationHostProtocolFunctions.ReadString(reader);
    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> bytes) => EmulationHostProtocolFunctions.WriteBytes(writer, bytes);
    internal static byte[] ReadBytes(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadBytes(reader, AmigaCoreHostConstants.HostName);
    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input) => EmulationHostProtocolFunctions.WriteInput(writer, input);
    internal static EmulationInputSnapshot ReadInput(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadInput(reader, AmigaCoreHostConstants.HostName);
    internal static void WriteFrame(BinaryWriter writer, VideoFrame? frame) => EmulationHostProtocolFunctions.WriteFrame(writer, frame);
    internal static VideoFrame? ReadFrame(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadFrame(reader, AmigaCoreHostConstants.HostName);
    internal static void WriteSharedFrame(BinaryWriter writer, VideoFrame? frame, MemoryMappedViewAccessor videoMap) =>
        EmulationHostProtocolFunctions.WriteSharedFrame(writer, frame, videoMap, AmigaCoreHostConstants.HostName);
    internal static VideoFrame? ReadSharedFrame(BinaryReader reader, MemoryMappedViewAccessor videoMap) =>
        EmulationHostProtocolFunctions.ReadSharedFrame(reader, videoMap, AmigaCoreHostConstants.HostName);
    internal static void WriteAudio(BinaryWriter writer, IReadOnlyList<AudioChunk> chunks) => EmulationHostProtocolFunctions.WriteAudio(writer, chunks);
    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadAudio(reader, AmigaCoreHostConstants.HostName);
    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states) => EmulationHostProtocolFunctions.WriteLedStates(writer, states);
    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader) =>
        EmulationHostProtocolFunctions.ReadLedStates(reader, AmigaCoreHostConstants.HostName);
}

public static class AmigaCoreHost
{
    [SupportedOSPlatform("windows")]
    public static void Run(string pipeName, string videoMapName)
    {
        using var videoMemory = MemoryMappedFile.OpenExisting(videoMapName, MemoryMappedFileRights.ReadWrite);
        using var videoMap = videoMemory.CreateViewAccessor(0, EmulationHostProtocolConstants.VideoMapCapacity,
            MemoryMappedFileAccess.ReadWrite);
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None);
        pipe.Connect(15_000);
        using var reader = new BinaryReader(pipe, System.Text.Encoding.UTF8, true);
        using var transportWriter = new BinaryWriter(pipe, System.Text.Encoding.UTF8, true);
        AmigaExternalCore? core = null;
        var lastVideoSequence = 0L;
        var lastDiagnosticCount = 0;
        while (true)
        {
            AmigaHostCommand command;
            try { command = (AmigaHostCommand)reader.ReadByte(); }
            catch (EndOfStreamException) { break; }
            var exit = command == AmigaHostCommand.Dispose;
            using var responseStream = new MemoryStream();
            using var writer = new BinaryWriter(responseStream, System.Text.Encoding.UTF8, true);
            try
            {
                switch (command)
                {
                    case AmigaHostCommand.Initialize:
                        var corePath = reader.ReadString();
                        var session = reader.ReadString();
                        var saves = AmigaCoreHostProtocol.ReadString(reader);
                        var configuration = JsonSerializer.Deserialize<AmigaMachineConfiguration>(reader.ReadString(), AmigaCoreHostProtocol.JsonOptions)
                            ?? throw new InvalidDataException("The Amiga host configuration is invalid.");
                        core = new AmigaExternalCore(corePath);
                        core.Initialize(configuration, session, saves);
                        writer.Write(true);
                        writer.Write(core.CoreSha256); writer.Write(core.FramesPerSecond); writer.Write(core.SampleRate);
                        writer.Write(JsonSerializer.Serialize(core.Options, AmigaCoreHostProtocol.JsonOptions));
                        writer.Write(JsonSerializer.Serialize(core.Diagnostics, AmigaCoreHostProtocol.JsonOptions));
                        lastDiagnosticCount = core.Diagnostics.Count;
                        writer.Write(core.CoreName); writer.Write(core.CoreVersion);
                        writer.Write(string.Join('|', core.SupportedContentExtensions.Order(StringComparer.OrdinalIgnoreCase)));
                        writer.Write(core.DiskCount); writer.Write(core.CurrentDiskIndex);
                        AmigaCoreHostProtocol.WriteLedStates(writer, core.LedStates);
                        break;
                    case AmigaHostCommand.RunFrame:
                        var activeCore = EnsureCore(core);
                        activeCore.SetInput(AmigaCoreHostProtocol.ReadInput(reader));
                        activeCore.RunFrame();
                        writer.Write(true);
                        var frame = activeCore.LatestVideoFrame;
                        AmigaCoreHostProtocol.WriteSharedFrame(writer,
                            frame?.Sequence == lastVideoSequence ? null : frame, videoMap);
                        if (frame is not null) lastVideoSequence = frame.Sequence;
                        var audio = new List<AudioChunk>();
                        while (activeCore.TryDequeueAudio(out var chunk) && chunk is not null) audio.Add(chunk);
                        AmigaCoreHostProtocol.WriteAudio(writer, audio);
                        writer.Write(activeCore.FramesPerSecond); writer.Write(activeCore.SampleRate);
                        writer.Write(activeCore.DiskCount); writer.Write(activeCore.CurrentDiskIndex);
                        var diagnostics = activeCore.Diagnostics;
                        var diagnosticsChanged = diagnostics.Count != lastDiagnosticCount;
                        writer.Write(diagnosticsChanged);
                        if (diagnosticsChanged)
                        {
                            writer.Write(JsonSerializer.Serialize(diagnostics, AmigaCoreHostProtocol.JsonOptions));
                            lastDiagnosticCount = diagnostics.Count;
                        }
                        AmigaCoreHostProtocol.WriteLedStates(writer, activeCore.LedStates);
                        break;
                    case AmigaHostCommand.HardReset: EnsureCore(core).HardReset(); WriteSuccess(writer); break;
                    case AmigaHostCommand.Stop: EnsureCore(core).Stop(); WriteSuccess(writer); break;
                    case AmigaHostCommand.InsertMedia: EnsureCore(core).InsertMedia(reader.ReadString()); WriteSuccess(writer); break;
                    case AmigaHostCommand.EjectMedia: EnsureCore(core).EjectMedia(); WriteSuccess(writer); break;
                    case AmigaHostCommand.SaveState:
                        var state = EnsureCore(core).SaveState(); writer.Write(true); AmigaCoreHostProtocol.WriteBytes(writer, state); break;
                    case AmigaHostCommand.LoadState: EnsureCore(core).LoadState(AmigaCoreHostProtocol.ReadBytes(reader)); WriteSuccess(writer); break;
                    case AmigaHostCommand.SetOption: EnsureCore(core).SetOption(reader.ReadString(), reader.ReadString()); WriteSuccess(writer); break;
                    case AmigaHostCommand.SelectDisk: EnsureCore(core).SelectDisk(reader.ReadInt32()); WriteSuccess(writer); break;
                    case AmigaHostCommand.Dispose: core?.Dispose(); core = null; WriteSuccess(writer); break;
                    default: throw new InvalidDataException($"Unknown Amiga host command {(byte)command}.");
                }
            }
            catch (Exception error)
            {
                responseStream.SetLength(0);
                responseStream.Position = 0;
                writer.Write(false);
                writer.Write(error.ToString());
            }
            writer.Flush();
            AmigaCoreHostProtocol.WriteBytes(transportWriter,
                responseStream.GetBuffer().AsSpan(0, checked((int)responseStream.Length)));
            if (exit) break;
        }
        core?.Dispose();
    }

    private static AmigaExternalCore EnsureCore(AmigaExternalCore? core) => core ?? throw new InvalidOperationException("The Amiga host is not initialized.");
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
