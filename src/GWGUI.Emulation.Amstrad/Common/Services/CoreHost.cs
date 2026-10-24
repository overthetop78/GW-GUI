using CommonCoreHostFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreHostFunctions;
using System.IO;

using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Common.Services;

internal static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void Run(string pipeName, string videoMapName, Func<MachineConfiguration, string, IEmulatorCore> createCore)
    {
        ArgumentNullException.ThrowIfNull(createCore);
        CommonCoreHostFunctions.Protocol.WithSharedVideoMap(videoMapName, BufferConstants.FirstBufferIndex, videoMap =>
            CommonCoreHostFunctions.Protocol.RunConnectedSession(pipeName, CoreHostConstants.LocalPipeServerName, CoreHostConstants.ConnectionTimeoutMilliseconds, (reader, transportWriter) =>
            {
                IEmulatorCore? core = null;
                var lastVideoSequence = VideoConstants.InitialFrameSequence;
                var lastDiagnosticCount = BufferConstants.EmptyCollectionCount;
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
                                        static requestReader => JsonSerializer.Deserialize<MachineConfiguration>(requestReader.ReadString(), CommonCoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(ErrorMessages.HostConfigurationInvalid()));
                                try
                                {
                                    CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                                }
                                finally
                                {
                                    lastVideoSequence = VideoConstants.InitialFrameSequence;
                                }

                                core = createCore(configuration, corePath);
                                core.Initialize(configuration, session, saves);
                                CommonCoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Success);
                                CommonCoreHostFunctions.Protocol.WriteCoreInitialization<IEmulatorCore, MachineConfiguration, CoreOption>(
                                    writer, core, CommonCoreHostFunctions.JsonOptions, ref lastDiagnosticCount, MediaConstants.SupportedExtensionSeparator,
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
                                    throw new InvalidDataException(ErrorMessages.UnknownHostCommand((byte)command));
                                break;
                        }
                    }, (writer, error) =>
                    {
                        CommonCoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Failure);
                        CommonCoreHostFunctions.WriteError(writer, error, core?.Diagnostics ?? []);
                    });
                }
                finally
                {
                    CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                }
            }));
    }

    private static IEmulatorCore EnsureCore(IEmulatorCore? core) => core ?? throw new InvalidOperationException(ErrorMessages.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => CommonCoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Success);
}
