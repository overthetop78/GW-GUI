using GWGUI.Emulation.Sony.Emulators.Ppsspp.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Contracts;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Factories;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Enums;

using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;

using System.IO;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

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
                            var (corePath, session, saves, configuration) =
                                EmulationHostProtocolFunctions.ReadInitializationRequest(reader,
                                    CoreHostProtocol.ReadString,
                                    static requestReader => JsonSerializer.Deserialize<MachineConfiguration>(requestReader.ReadString(), CoreHostProtocol.JsonOptions) ?? throw new InvalidDataException(PpssppExceptions.HostConfigurationInvalid()));
                            core = new ExternalCore(corePath);
                            core.Initialize(configuration, session, saves);
                            writer.Write(true);
                            EmulationHostProtocolFunctions.WriteCoreInitialization<ExternalCore, MachineConfiguration, CoreOption>(
                                writer, core, CoreHostProtocol.JsonOptions, ref lastDiagnosticCount, '|');
                            writer.Write(core.DiskCount);
                            writer.Write(core.CurrentDiskIndex);
                            CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                            writer.Write(core.ShutdownRequested);
                            break;
                        case HostCommand.RunFrame:
                            var activeCore = EnsureCore(core);
                            EmulationHostProtocolFunctions.RunCoreFrame<ExternalCore, MachineConfiguration, CoreOption>(
                                reader, writer, activeCore, CoreHostProtocol.ReadInput,
                                (frameWriter, pendingFrame) => CoreHostProtocol.WriteSharedFrame(frameWriter, pendingFrame, videoMap),
                                static (statusWriter, frameCore) =>
                                {
                                    statusWriter.Write(frameCore.DiskCount);
                                    statusWriter.Write(frameCore.CurrentDiskIndex);
                                }, CoreHostProtocol.WriteLedStates, static () => CoreHostProtocol.JsonOptions,
                                ref lastVideoSequence, ref lastDiagnosticCount);
                            writer.Write(activeCore.ShutdownRequested);
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
                                throw new InvalidDataException(PpssppExceptions.UnknownHostCommand((byte)command));
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

    private static ExternalCore EnsureCore(ExternalCore? core) => core ?? throw new InvalidOperationException(PpssppExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
