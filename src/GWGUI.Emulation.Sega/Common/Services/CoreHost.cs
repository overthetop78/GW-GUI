using CommonCoreHostFunctions = GWGUI.Emulation.Sega.Common.Functions.CoreHostFunctions;

using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text.Json;

using System.IO;

namespace GWGUI.Emulation.Sega.Common.Services;

internal static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    public static void Run(string pipeName, string videoMapName, Func<MachineConfiguration, string, IEmulatorCore> createCore)
    {
        CommonCoreHostFunctions.Protocol.WithSuppressedErrorDialogs(() => RunSession(pipeName, videoMapName, createCore));
    }

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    private static void RunSession(string pipeName, string videoMapName, Func<MachineConfiguration, string, IEmulatorCore> createCore)
    {
        CommonCoreHostFunctions.Protocol.WithSharedVideoMap(videoMapName, 0, videoMap =>
            CommonCoreHostFunctions.Protocol.RunConnectedSession(pipeName, CoreHostConstants.LocalPipeServer, CoreHostConstants.ConnectionTimeoutMilliseconds, (reader, transportWriter) =>
            {
                IEmulatorCore? core = null;
                var lastVideoSequence = 0L;
                var lastDiagnosticCount = 0;
                try
                {
                    CommonCoreHostFunctions.Protocol.RunCommandLoop<HostCommand>(reader, transportWriter, CommonCoreHostFunctions.ReadRequestHeader, HostCommand.Dispose, (command, writer) =>
                    {
                        switch (command)
                        {
                            case HostCommand.Initialize:
                                var (corePath, session, saves, configuration) =
                                    CommonCoreHostFunctions.Protocol.ReadInitializationRequest(reader,
                                        CommonCoreHostFunctions.ReadString,
                                        static requestReader => JsonSerializer.Deserialize<MachineConfiguration>(requestReader.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(CoreExceptions.HostConfigurationInvalid()));
                                CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                                core = createCore(configuration, corePath);
                                core.Initialize(configuration, session, saves);
                                CommonCoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Success);
                                CommonCoreHostFunctions.Protocol.WriteCoreInitialization<IEmulatorCore, MachineConfiguration, CoreOption>(
                                    writer, core, CommonCoreHostFunctions.JsonOptions, ref lastDiagnosticCount, '|',
                                    static (statusWriter, currentCore) =>
                                    {
                                        statusWriter.Write(currentCore.MinimumAudioLatencyMilliseconds);
                                        statusWriter.Write(currentCore.SupportsSaveStates);
                                        CommonCoreHostFunctions.Protocol.WriteRuntimeStatus(statusWriter, currentCore.Region, currentCore.BufferedAudioFrames, currentCore.AudioOverrunCount, currentCore.AudioUnderrunCount);
                                    });
                                CommonCoreHostFunctions.Protocol.WriteDiskSelection(writer, core.DiskCount, core.CurrentDiskIndex);
                                CommonCoreHostFunctions.WriteLedStates(writer, core.LedStates);
                                writer.Write(core.ShutdownRequested);
                                CommonCoreHostFunctions.Protocol.WriteControllerFeedback(writer, core.ControllerFeedback);
                                break;
                            case HostCommand.RunFrame:
                                var activeCore = EnsureCore(core);
                                CommonCoreHostFunctions.Protocol.RunCoreFrame<IEmulatorCore, MachineConfiguration, CoreOption>(
                                    reader, writer, activeCore, CommonCoreHostFunctions.ReadInput,
                                    (frameWriter, pendingFrame) => CommonCoreHostFunctions.WriteSharedFrame(frameWriter, pendingFrame, videoMap),
                                    static (statusWriter, currentCore) => CommonCoreHostFunctions.Protocol.WriteDiskSelection(statusWriter, currentCore.DiskCount, currentCore.CurrentDiskIndex), CommonCoreHostFunctions.WriteLedStates, static () => CommonCoreHostFunctions.JsonOptions,
                                    ref lastVideoSequence, ref lastDiagnosticCount, WriteSuccess);
                                writer.Write(activeCore.ShutdownRequested);
                                CommonCoreHostFunctions.Protocol.WriteControllerFeedback(writer, activeCore.ControllerFeedback);
                                break;
                            case HostCommand.Dispose:
                                CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                                WriteSuccess(writer);
                                break;
                            default:
                                if (!CommonCoreHostFunctions.Protocol.TryExecuteCoreCommand<MediaConfiguration, PeripheralCategory>((byte)command, reader, writer,
                                        () => EnsureCore(core), static requestReader => System.Text.Json.JsonSerializer.Deserialize<MediaConfiguration>(requestReader.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(),
                                        CommonCoreHostFunctions.ReadBytes, static (statusWriter, status) => statusWriter.Write(System.Text.Json.JsonSerializer.Serialize(status, CommonCoreHostFunctions.JsonOptions)), static value => (PeripheralCategory)value, WriteSuccess))
                                    throw new InvalidDataException(CoreExceptions.UnknownHostCommand((byte)command));
                                break;
                        }
                    }, (writer, error) =>
                    {
                        CommonCoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Failure);
                        writer.Write(error.ToString());
                    });
                }
                finally
                {
                    CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                }
            }));
    }

    private static IEmulatorCore EnsureCore(IEmulatorCore? core) => core ?? throw new InvalidOperationException(CoreExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => CommonCoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Success);

}
