using System.Globalization;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

public static class AtariCoreHost
{
    [SupportedOSPlatform("windows")]
    public static void Run(string pipeName, string videoMapName)
    {
        using var video = new AtariSharedVideoWriter(videoMapName);
        using var pipe = new NamedPipeClientStream(AtariCoreHostConstants.LocalPipeServerName,
            pipeName, PipeDirection.InOut, PipeOptions.None);
        pipe.Connect(AtariCoreHostConstants.ConnectionTimeoutMilliseconds);
        using var reader = new BinaryReader(pipe, Encoding.UTF8, leaveOpen: true);
        using var transportWriter = new BinaryWriter(pipe, Encoding.UTF8, leaveOpen: true);
        AtariExternalCore? core = null;
        var lastVideoSequence = AtariCoreHostConstants.InitialVideoSequence;
        var lastDiagnosticCount = AtariCoreHostConstants.InitialDiagnosticCount;
        try
        {
            while (TryReadCommand(reader, out var command))
            {
                var exit = command == AtariHostCommand.Dispose;
                using var responseStream = new MemoryStream();
                using var writer = new BinaryWriter(responseStream, Encoding.UTF8, leaveOpen: true);
                try
                {
                    AtariCoreHostFunctions.WriteResponseHeader(writer, AtariHostResponseStatus.Success);
                    switch (command)
                    {
                        case AtariHostCommand.Initialize:
                            Initialize(reader, writer, ref core, ref lastDiagnosticCount);
                            break;
                        case AtariHostCommand.RunFrame:
                            RunFrame(reader, writer, video, EnsureCore(core), ref lastVideoSequence,
                                ref lastDiagnosticCount);
                            break;
                        case AtariHostCommand.HardReset:
                            EnsureCore(core).HardReset();
                            break;
                        case AtariHostCommand.Stop:
                            EnsureCore(core).Stop();
                            break;
                        case AtariHostCommand.InsertMedia:
                            EnsureCore(core).InsertMedia(ReadMedia(reader));
                            break;
                        case AtariHostCommand.EjectMedia:
                            EnsureCore(core).EjectMedia((EmulationMediaSlot)reader.ReadInt32());
                            break;
                        case AtariHostCommand.SaveState:
                            AtariCoreHostFunctions.WriteBytes(writer, EnsureCore(core).SaveState());
                            break;
                        case AtariHostCommand.LoadState:
                            EnsureCore(core).LoadState(AtariCoreHostFunctions.ReadBytes(reader));
                            break;
                        case AtariHostCommand.SetOption:
                            EnsureCore(core).SetOption(reader.ReadString(), reader.ReadString());
                            break;
                        case AtariHostCommand.SelectDisk:
                            EnsureCore(core).SelectDisk(reader.ReadInt32());
                            break;
                        case AtariHostCommand.SaveMediaChanges:
                            EnsureCore(core).SaveMediaChanges((EmulationMediaSlot)reader.ReadInt32());
                            break;
                        case AtariHostCommand.GetDiskStatus:
                            AtariCoreHostFunctions.WriteDiskStatus(writer, EnsureCore(core).GetDiskStatus());
                            break;
                        case AtariHostCommand.HasUnsavedMediaChanges:
                            writer.Write(EnsureCore(core).HasUnsavedMediaChanges(
                                (EmulationMediaSlot)reader.ReadInt32()));
                            break;
                        case AtariHostCommand.Dispose:
                            core?.Dispose();
                            core = null;
                            break;
                        default:
                            throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
                                AtariCoreHostErrors.UnknownCommandFormat, (byte)command));
                    }
                }
                catch (Exception error)
                {
                    responseStream.SetLength(0);
                    responseStream.Position = AtariConstants.FirstBufferIndex;
                    AtariCoreHostFunctions.WriteResponseHeader(writer, AtariHostResponseStatus.Failure);
                    AtariCoreHostFunctions.WriteError(writer, error);
                }
                writer.Flush();
                AtariCoreHostFunctions.WriteBytes(transportWriter,
                    responseStream.GetBuffer().AsSpan(AtariConstants.FirstBufferIndex,
                        checked((int)responseStream.Length)));
                if (exit) break;
            }
        }
        finally
        {
            core?.Dispose();
        }
    }

    private static bool TryReadCommand(BinaryReader reader, out AtariHostCommand command)
    {
        try
        {
            command = AtariCoreHostFunctions.ReadRequestHeader(reader);
            return true;
        }
        catch (EndOfStreamException)
        {
            command = default;
            return false;
        }
    }

    private static void Initialize(BinaryReader reader, BinaryWriter writer, ref AtariExternalCore? core,
        ref int lastDiagnosticCount)
    {
        core?.Dispose();
        var corePath = reader.ReadString();
        var kind = (AtariCoreKind)reader.ReadInt32();
        var session = reader.ReadString();
        var saves = AtariCoreHostFunctions.ReadString(reader);
        var configuration = JsonSerializer.Deserialize<AtariMachineConfiguration>(reader.ReadString(),
            AtariCoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(AtariCoreHostErrors.InvalidConfiguration);
        core = new AtariExternalCore(corePath, kind);
        core.Initialize(configuration, session, saves);
        writer.Write(core.CoreSha256);
        writer.Write(core.FramesPerSecond);
        writer.Write(core.SampleRate);
        writer.Write(JsonSerializer.Serialize(core.Options, AtariCoreHostFunctions.JsonOptions));
        writer.Write(JsonSerializer.Serialize(core.Diagnostics, AtariCoreHostFunctions.JsonOptions));
        lastDiagnosticCount = core.Diagnostics.Count;
        writer.Write(core.CoreName);
        writer.Write(core.CoreVersion);
        writer.Write(string.Join(AtariCoreHostConstants.ExtensionListSeparator,
            core.SupportedContentExtensions.Order(StringComparer.OrdinalIgnoreCase)));
        AtariCoreHostFunctions.WriteLedStates(writer, core.LedStates);
    }

    [SupportedOSPlatform("windows")]
    private static void RunFrame(BinaryReader reader, BinaryWriter writer, AtariSharedVideoWriter video,
        AtariExternalCore core, ref long lastVideoSequence, ref int lastDiagnosticCount)
    {
        core.SetInput(AtariCoreHostFunctions.ReadInput(reader));
        core.RunFrame();
        var frame = core.LatestVideoFrame;
        AtariCoreHostFunctions.WriteResizableSharedFrame(writer,
            frame?.Sequence == lastVideoSequence ? null : frame, video);
        if (frame is not null) lastVideoSequence = frame.Sequence;
        var audio = new List<AudioChunk>();
        while (core.TryDequeueAudio(out var chunk) && chunk is not null) audio.Add(chunk);
        AtariCoreHostFunctions.WriteAudio(writer, audio);
        writer.Write(core.FramesPerSecond);
        writer.Write(core.SampleRate);
        var diagnosticsChanged = core.Diagnostics.Count != lastDiagnosticCount;
        writer.Write(diagnosticsChanged);
        if (diagnosticsChanged)
        {
            writer.Write(JsonSerializer.Serialize(core.Diagnostics, AtariCoreHostFunctions.JsonOptions));
            lastDiagnosticCount = core.Diagnostics.Count;
        }
        AtariCoreHostFunctions.WriteLedStates(writer, core.LedStates);
    }

    private static AtariMediaConfiguration ReadMedia(BinaryReader reader) =>
        JsonSerializer.Deserialize<AtariMediaConfiguration>(reader.ReadString(), AtariCoreHostFunctions.JsonOptions)
        ?? throw new InvalidDataException(AtariCoreHostErrors.InvalidConfiguration);

    private static AtariExternalCore EnsureCore(AtariExternalCore? core) => core ??
        throw new InvalidOperationException(AtariCoreHostErrors.NotInitialized);
}
