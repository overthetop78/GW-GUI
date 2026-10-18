using ControllerFeedbackProtocol = GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.ControllerFeedbackProtocol;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Enums;

using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;

using System.IO;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;

public static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    public static void Run(string pipeName, string videoMapName)
    {
        using var videoMemory = MemoryMappedFile.OpenExisting(videoMapName, MemoryMappedFileRights.ReadWrite);
        using var videoMap = videoMemory.CreateViewAccessor(0, EmulationHostProtocolConstants.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
        EmulationHostProtocolFunctions.RunConnectedSession(pipeName, CoreHostConstants.Value, 15_000, (reader, transportWriter) =>
        {
            ExternalCore? core = null;
            var lastVideoSequence = 0L;
            var lastDiagnosticCount = 0;
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
                            var configuration = JsonSerializer.Deserialize<MachineConfiguration>(reader.ReadString(), CoreHostProtocol.JsonOptions) ?? throw new InvalidDataException(Pcsx2Exceptions.HostConfigurationInvalid());
                            core = new ExternalCore(corePath);
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
                            writer.Write(string.Join('|', core.SupportedContentExtensions.Order(StringComparer.OrdinalIgnoreCase)));
                            writer.Write(core.DiskCount);
                            writer.Write(core.CurrentDiskIndex);
                            CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                            writer.Write(core.ShutdownRequested);
                            ControllerFeedbackProtocol.Write(writer, core.ControllerFeedback);
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
                            writer.Write(activeCore.ShutdownRequested);
                            ControllerFeedbackProtocol.Write(writer, activeCore.ControllerFeedback);
                            break;
                        case HostCommand.SoftReset:
                            EnsureCore(core).SoftReset();
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
                        case HostCommand.SetControllerPortDevice:
                            EnsureCore(core).SetControllerPortDevice(reader.ReadInt32(), (ControllerType)reader.ReadInt32());
                            WriteSuccess(writer);
                            break;
                        case HostCommand.Dispose:
                            core?.Dispose();
                            core = null;
                            WriteSuccess(writer);
                            break;
                        default:
                            if (!EmulationHostProtocolFunctions.TryExecuteCoreStateCommand((byte)command, reader, writer,
                                    () => EnsureCore(core), CoreHostProtocol.ReadBytes, WriteSuccess))
                                throw new InvalidDataException(Pcsx2Exceptions.UnknownHostCommand((byte)command));
                            break;
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

    private static ExternalCore EnsureCore(ExternalCore? core) => core ?? throw new InvalidOperationException(Pcsx2Exceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
