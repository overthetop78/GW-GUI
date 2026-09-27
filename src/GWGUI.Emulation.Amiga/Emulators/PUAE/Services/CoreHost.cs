using GWGUI.Emulation.Amiga.Emulators.PUAE.Exceptions;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Contracts;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Factories;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Functions;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

public static class CoreHost
{
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    public static void Run(string pipeName, string videoMapName)
    {
        using var videoMemory = MemoryMappedFile.OpenExisting(videoMapName, MemoryMappedFileRights.ReadWrite);
        using var videoMap = videoMemory.CreateViewAccessor(0, EmulationHostProtocolConstants.VideoMapCapacity,
            MemoryMappedFileAccess.ReadWrite);
        using var pipe = new NamedPipeClientStream(CoreHostConstants.Value, pipeName, PipeDirection.InOut, PipeOptions.None);
        pipe.Connect(15_000);
        using var reader = new BinaryReader(pipe, System.Text.Encoding.UTF8, true);
        using var transportWriter = new BinaryWriter(pipe, System.Text.Encoding.UTF8, true);
        ExternalCore? core = null;
        var lastVideoSequence = 0L;
        var lastDiagnosticCount = 0;
        while (true)
        {
            HostCommand command;
            try { command = (HostCommand)reader.ReadByte(); }
            catch (EndOfStreamException) { break; }
            var exit = command == HostCommand.Dispose;
            using var responseStream = new MemoryStream();
            using var writer = new BinaryWriter(responseStream, System.Text.Encoding.UTF8, true);
            try
            {
                switch (command)
                {
                    case HostCommand.Initialize:
                        var corePath = reader.ReadString();
                        var session = reader.ReadString();
                        var saves = CoreHostProtocol.ReadString(reader);
                        var configuration = JsonSerializer.Deserialize<MachineConfiguration>(reader.ReadString(), CoreHostProtocol.JsonOptions)
                            ?? throw new InvalidDataException(PuaeExceptions.HostConfigurationInvalid());
                        core = new ExternalCore(corePath);
                        core.Initialize(configuration, session, saves);
                        writer.Write(true);
                        writer.Write(core.CoreSha256); writer.Write(core.FramesPerSecond); writer.Write(core.SampleRate);
                        writer.Write(JsonSerializer.Serialize(core.Options, CoreHostProtocol.JsonOptions));
                        writer.Write(JsonSerializer.Serialize(core.Diagnostics, CoreHostProtocol.JsonOptions));
                        lastDiagnosticCount = core.Diagnostics.Count;
                        writer.Write(core.CoreName); writer.Write(core.CoreVersion);
                        writer.Write(string.Join('|', core.SupportedContentExtensions.Order(StringComparer.OrdinalIgnoreCase)));
                        writer.Write(core.DiskCount); writer.Write(core.CurrentDiskIndex);
                        CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                        break;
                    case HostCommand.RunFrame:
                        var activeCore = EnsureCore(core);
                        activeCore.SetInput(CoreHostProtocol.ReadInput(reader));
                        activeCore.RunFrame();
                        writer.Write(true);
                        var frame = activeCore.LatestVideoFrame;
                        CoreHostProtocol.WriteSharedFrame(writer,
                            frame?.Sequence == lastVideoSequence ? null : frame, videoMap);
                        if (frame is not null) lastVideoSequence = frame.Sequence;
                        var audio = new List<AudioChunk>();
                        while (activeCore.TryDequeueAudio(out var chunk) && chunk is not null) audio.Add(chunk);
                        CoreHostProtocol.WriteAudio(writer, audio);
                        writer.Write(activeCore.FramesPerSecond); writer.Write(activeCore.SampleRate);
                        writer.Write(activeCore.DiskCount); writer.Write(activeCore.CurrentDiskIndex);
                        var diagnostics = activeCore.Diagnostics;
                        var diagnosticsChanged = diagnostics.Count != lastDiagnosticCount;
                        writer.Write(diagnosticsChanged);
                        if (diagnosticsChanged)
                        {
                            writer.Write(JsonSerializer.Serialize(diagnostics, CoreHostProtocol.JsonOptions));
                            lastDiagnosticCount = diagnostics.Count;
                        }
                        CoreHostProtocol.WriteLedStates(writer, activeCore.LedStates);
                        break;
                    case HostCommand.HardReset: EnsureCore(core).HardReset(); WriteSuccess(writer); break;
                    case HostCommand.Stop: EnsureCore(core).Stop(); WriteSuccess(writer); break;
                    case HostCommand.InsertMedia: EnsureCore(core).InsertMedia(reader.ReadString()); WriteSuccess(writer); break;
                    case HostCommand.EjectMedia: EnsureCore(core).EjectMedia(); WriteSuccess(writer); break;
                    case HostCommand.SaveState:
                        var state = EnsureCore(core).SaveState(); writer.Write(true); CoreHostProtocol.WriteBytes(writer, state); break;
                    case HostCommand.LoadState: EnsureCore(core).LoadState(CoreHostProtocol.ReadBytes(reader)); WriteSuccess(writer); break;
                    case HostCommand.SetOption: EnsureCore(core).SetOption(reader.ReadString(), reader.ReadString()); WriteSuccess(writer); break;
                    case HostCommand.SelectDisk: EnsureCore(core).SelectDisk(reader.ReadInt32()); WriteSuccess(writer); break;
                    case HostCommand.Dispose: core?.Dispose(); core = null; WriteSuccess(writer); break;
                    default: throw new InvalidDataException(PuaeExceptions.UnknownHostCommand((byte)command));
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
            CoreHostProtocol.WriteBytes(transportWriter,
                responseStream.GetBuffer().AsSpan(BufferConstants.FirstBufferIndex,
                    checked((int)responseStream.Length)));
            if (exit) break;
        }
        core?.Dispose();
    }

    private static ExternalCore EnsureCore(ExternalCore? core) => core ?? throw new InvalidOperationException(PuaeExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
