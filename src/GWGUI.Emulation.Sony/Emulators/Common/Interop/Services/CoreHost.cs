using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;

using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text.Json;

using System.IO;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Dictionaries;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

public static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    public static void Run(string pipeName, string videoMapName)
    {
        EmulationHostProtocolFunctions.WithSharedVideoMap(videoMapName, 0, videoMap =>
            EmulationHostProtocolFunctions.RunConnectedSession(pipeName, CoreHostConstants.LocalPipeServer, checked((int)TimeSpan.FromSeconds(ProcessCoreConstants.ConnectionTimeoutSeconds).TotalMilliseconds), (reader, transportWriter) =>
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
                                        static requestReader => JsonSerializer.Deserialize<MachineConfiguration>(requestReader.ReadString(), CoreHostProtocol.JsonOptions) ?? throw new InvalidDataException(CoreExceptions.HostConfigurationInvalid()));
                                EmulationHostProtocolFunctions.DisposePreviousCore(ref core);
                                core = new ExternalCore(CoreCatalog.Get(configuration.EmulatorId), corePath);
                                core.Initialize(configuration, session, saves);
                                writer.Write(true);
                                EmulationHostProtocolFunctions.WriteCoreInitialization<ExternalCore, MachineConfiguration, CoreOption>(
                                    writer, core, CoreHostProtocol.JsonOptions, ref lastDiagnosticCount, '|');
                                EmulationHostProtocolFunctions.WriteDiskSelection(writer, core);
                                CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                                writer.Write(core.ShutdownRequested);
                                ControllerFeedbackProtocol.Write(writer, core.ControllerFeedback);
                                break;
                            case HostCommand.RunFrame:
                                var activeCore = EnsureCore(core);
                                EmulationHostProtocolFunctions.RunCoreFrame<ExternalCore, MachineConfiguration, CoreOption>(
                                    reader, writer, activeCore, CoreHostProtocol.ReadInput,
                                    (frameWriter, pendingFrame) => CoreHostProtocol.WriteSharedFrame(frameWriter, pendingFrame, videoMap),
                                    EmulationHostProtocolFunctions.WriteDiskSelection, CoreHostProtocol.WriteLedStates, static () => CoreHostProtocol.JsonOptions,
                                    ref lastVideoSequence, ref lastDiagnosticCount);
                                writer.Write(activeCore.ShutdownRequested);
                                ControllerFeedbackProtocol.Write(writer, activeCore.ControllerFeedback);
                                break;
                            case HostCommand.SoftReset:
                                EnsureCore(core).SoftReset();
                                WriteSuccess(writer);
                                break;
                            case HostCommand.SetControllerPortDevice:
                                EnsureCore(core).SetControllerPortDevice(reader.ReadInt32(), (ControllerType)reader.ReadInt32());
                                WriteSuccess(writer);
                                break;
                            case HostCommand.Dispose:
                                EmulationHostProtocolFunctions.DisposePreviousCore(ref core);
                                WriteSuccess(writer);
                                break;
                            default:
                                if (!EmulationHostProtocolFunctions.TryExecutePathMediaCommand((byte)command, reader, writer,
                                        () => EnsureCore(core), CoreHostProtocol.ReadBytes, WriteSuccess))
                                    throw new InvalidDataException(CoreExceptions.UnknownHostCommand((byte)command));
                                break;
                        }
                    }, (writer, error) =>
                    {
                        writer.Write(false);
                        writer.Write(CoreDiagnosticMonitor.WithNativeDiagnostics(error, core?.Diagnostics ?? []).ToString());
                    });
                }
                finally
                {
                    EmulationHostProtocolFunctions.DisposePreviousCore(ref core);
                }
            }));
    }

    private static ExternalCore EnsureCore(ExternalCore? core) => core ?? throw new InvalidOperationException(CoreExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
