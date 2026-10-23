using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
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

internal static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    public static void Run(string pipeName, string videoMapName)
    {
        CommonCoreHostFunctions.Protocol.WithSharedVideoMap(videoMapName, 0, videoMap =>
            CommonCoreHostFunctions.Protocol.RunConnectedSession(pipeName, CoreHostConstants.LocalPipeServer, checked((int)TimeSpan.FromSeconds(ProcessCoreConstants.ConnectionTimeoutSeconds).TotalMilliseconds), (reader, transportWriter) =>
            {
                ExternalCore? core = null;
                var lastVideoSequence = 0L;
                var lastDiagnosticCount = 0;
                try
                {
                    CommonCoreHostFunctions.Protocol.RunCommandLoop<HostCommand>(reader, transportWriter, static commandReader => (HostCommand)commandReader.ReadByte(), HostCommand.Dispose, (command, writer) =>
                    {
                        switch (command)
                        {
                            case HostCommand.Initialize:
                                var (corePath, session, saves, configuration) =
                                    CommonCoreHostFunctions.Protocol.ReadInitializationRequest(reader,
                                        CoreHostProtocol.ReadString,
                                        static requestReader => JsonSerializer.Deserialize<MachineConfiguration>(requestReader.ReadString(), CoreHostProtocol.JsonOptions) ?? throw new InvalidDataException(CoreExceptions.HostConfigurationInvalid()));
                                CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                                core = new ExternalCore(CoreCatalog.Get(configuration.EmulatorId), corePath);
                                core.Initialize(configuration, session, saves);
                                writer.Write(true);
                                CommonCoreHostFunctions.Protocol.WriteCoreInitialization<ExternalCore, MachineConfiguration, CoreOption>(
                                    writer, core, CoreHostProtocol.JsonOptions, ref lastDiagnosticCount, '|');
                                CommonCoreHostFunctions.Protocol.WriteDiskSelection(writer, core);
                                CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                                writer.Write(core.ShutdownRequested);
                                CommonCoreHostFunctions.Protocol.WriteControllerFeedback(writer, core.ControllerFeedback);
                                break;
                            case HostCommand.RunFrame:
                                var activeCore = EnsureCore(core);
                                CommonCoreHostFunctions.Protocol.RunCoreFrame<ExternalCore, MachineConfiguration, CoreOption>(
                                    reader, writer, activeCore, CoreHostProtocol.ReadInput,
                                    (frameWriter, pendingFrame) => CoreHostProtocol.WriteSharedFrame(frameWriter, pendingFrame, videoMap),
                                    CommonCoreHostFunctions.Protocol.WriteDiskSelection, CoreHostProtocol.WriteLedStates, static () => CoreHostProtocol.JsonOptions,
                                    ref lastVideoSequence, ref lastDiagnosticCount);
                                writer.Write(activeCore.ShutdownRequested);
                                CommonCoreHostFunctions.Protocol.WriteControllerFeedback(writer, activeCore.ControllerFeedback);
                                break;
                            case HostCommand.SoftReset:
                                EnsureCore(core).SoftReset();
                                WriteSuccess(writer);
                                break;
                            case HostCommand.SetControllerPortDevice:
                                EnsureCore(core).SetControllerPortDevice(reader.ReadInt32(), (PeripheralCategory)reader.ReadInt32());
                                WriteSuccess(writer);
                                break;
                            case HostCommand.Dispose:
                                CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                                WriteSuccess(writer);
                                break;
                            default:
                                if (!CommonCoreHostFunctions.Protocol.TryExecutePathMediaCommand((byte)command, reader, writer,
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
                    CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                }
            }));
    }

    private static ExternalCore EnsureCore(ExternalCore? core) => core ?? throw new InvalidOperationException(CoreExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
