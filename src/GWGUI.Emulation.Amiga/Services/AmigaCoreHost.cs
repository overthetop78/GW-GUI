using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga.Services;

public static class AmigaCoreHost
{
    [SupportedOSPlatform(AmigaCoreHostValues.Windows)]
    public static void Run(string pipeName, string videoMapName)
    {
        using var videoMemory = MemoryMappedFile.OpenExisting(videoMapName, MemoryMappedFileRights.ReadWrite);
        using var videoMap = videoMemory.CreateViewAccessor(0, EmulationHostProtocolConstants.VideoMapCapacity,
            MemoryMappedFileAccess.ReadWrite);
        using var pipe = new NamedPipeClientStream(AmigaCoreHostValues.Value, pipeName, PipeDirection.InOut, PipeOptions.None);
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
                            ?? throw new InvalidDataException(AmigaCoreHostValues.TheAmigaHostConfigurationIsInvalid);
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

    private static AmigaExternalCore EnsureCore(AmigaExternalCore? core) => core ?? throw new InvalidOperationException(AmigaCoreHostValues.TheAmigaHostIsNotInitialized);
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
