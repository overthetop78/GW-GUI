using System.IO;
using GWGUI.Emulation.Amstrad.Emulators.Common.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Services;

public static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void Run(string pipeName, string videoMapName, Func<string, IEmulatorCore> createCore)
    {
        ArgumentNullException.ThrowIfNull(createCore);
        using var videoMemory = MemoryMappedFile.OpenExisting(videoMapName, MemoryMappedFileRights.ReadWrite);
        using var videoMap = videoMemory.CreateViewAccessor(BufferConstants.FirstBufferIndex, EmulationHostProtocolConstants.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
        EmulationHostProtocolFunctions.RunConnectedSession(pipeName, CoreHostConstants.LocalPipeServerName, CoreHostConstants.PipeConnectTimeoutMilliseconds, (reader, transportWriter) =>
        {
            IEmulatorCore? core = null;
            var lastVideoSequence = VideoConstants.InitialFrameSequence;
            var lastDiagnosticCount = BufferConstants.EmptyCollectionCount;
            try
            {
                EmulationHostProtocolFunctions.RunCommandLoop<HostCommand>(reader, transportWriter, static commandReader => (HostCommand)commandReader.ReadByte(), HostCommand.Dispose, (command, writer) =>
                {
                    switch (command)
                    {
                        case HostCommand.Initialize:
                            var corePath = reader.ReadString();
                            var session = reader.ReadString();
                            var saves = CoreHostProtocol.ReadString(reader);
                            var configuration = JsonSerializer.Deserialize<MachineConfiguration>(reader.ReadString(), CoreHostProtocol.JsonOptions) ?? throw new InvalidDataException(CommonExceptions.HostConfigurationInvalid());
                            var previousCore = core;
                            core = null;
                            try
                            {
                                previousCore?.Dispose();
                            }
                            finally
                            {
                                lastVideoSequence = VideoConstants.InitialFrameSequence;
                            }

                            core = createCore(corePath);
                            core.Initialize(configuration, session, saves);
                            writer.Write(true);
                            writer.Write(core.CoreSha256);
                            writer.Write(core.FramesPerSecond);
                            writer.Write(core.SampleRate);
                            writer.Write(JsonSerializer.Serialize(core.Options, CoreHostProtocol.JsonOptions));
                            writer.Write(JsonSerializer.Serialize(core.Diagnostics, CoreHostProtocol.JsonOptions));
                            lastDiagnosticCount = core.Diagnostics.Count;
                            writer.Write(core.CoreName);
                            writer.Write(core.CoreVersion);
                            writer.Write(string.Join(MediaConstants.SupportedExtensionSeparator, core.SupportedContentExtensions.Order(StringComparer.OrdinalIgnoreCase)));
                            writer.Write(core.DiskCount);
                            writer.Write(core.CurrentDiskIndex);
                            CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                            break;
                        case HostCommand.RunFrame:
                            var activeCore = EnsureCore(core);
                            activeCore.SetInput(CoreHostProtocol.ReadInput(reader));
                            activeCore.RunFrame();
                            writer.Write(true);
                            EmulationHostProtocolFunctions.WriteFrameIfChanged(writer, activeCore.LatestVideoFrame, ref lastVideoSequence,
                                (frameWriter, pendingFrame) => CoreHostProtocol.WriteSharedFrame(frameWriter, pendingFrame, videoMap));
                            EmulationHostProtocolFunctions.WriteQueuedAudio(writer, () => activeCore.TryDequeueAudio(out var chunk) ? chunk : null);
                            writer.Write(activeCore.FramesPerSecond);
                            writer.Write(activeCore.SampleRate);
                            writer.Write(activeCore.DiskCount);
                            writer.Write(activeCore.CurrentDiskIndex);
                            var diagnostics = activeCore.Diagnostics;
                            EmulationHostProtocolFunctions.WriteChangedDiagnostics(writer, () => diagnostics.Count,
                                diagnosticWriter => diagnosticWriter.Write(JsonSerializer.Serialize(diagnostics, CoreHostProtocol.JsonOptions)), ref lastDiagnosticCount);

                            CoreHostProtocol.WriteLedStates(writer, activeCore.LedStates);
                            break;
                        case HostCommand.HardReset:
                            EnsureCore(core).HardReset();
                            WriteSuccess(writer);
                            break;
                        case HostCommand.SoftReset:
                            EnsureCore(core).SoftReset();
                            WriteSuccess(writer);
                            break;
                        case HostCommand.Stop:
                            EnsureCore(core).Stop();
                            WriteSuccess(writer);
                            break;
                        case HostCommand.InsertMedia:
                            EnsureCore(core).InsertMedia(reader.ReadString());
                            WriteSuccess(writer);
                            break;
                        case HostCommand.EjectMedia:
                            EnsureCore(core).EjectMedia();
                            WriteSuccess(writer);
                            break;
                        case HostCommand.SaveState:
                            var state = EnsureCore(core).SaveState();
                            writer.Write(true);
                            CoreHostProtocol.WriteBytes(writer, state);
                            break;
                        case HostCommand.LoadState:
                            EnsureCore(core).LoadState(CoreHostProtocol.ReadBytes(reader));
                            WriteSuccess(writer);
                            break;
                        case HostCommand.SetOption:
                            EnsureCore(core).SetOption(reader.ReadString(), reader.ReadString());
                            WriteSuccess(writer);
                            break;
                        case HostCommand.SelectDisk:
                            EnsureCore(core).SelectDisk(reader.ReadInt32());
                            WriteSuccess(writer);
                            break;
                        case HostCommand.Dispose:
                            core?.Dispose();
                            core = null;
                            WriteSuccess(writer);
                            break;
                        default:
                            throw new InvalidDataException(CommonExceptions.UnknownHostCommand((byte)command));
                    }
                }, (writer, error) =>
                {
                    writer.Write(false);
                    writer.Write(error.ToString());
                });
            }
            finally
            {
                core?.Dispose();
            }
        });
    }

    private static IEmulatorCore EnsureCore(IEmulatorCore? core) => core ?? throw new InvalidOperationException(CommonExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
