using CommonCoreHostFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreHostFunctions;
using System.IO;
using GWGUI.Emulation.Amstrad.Emulators.Common.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Services;

internal static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void Run(string pipeName, string videoMapName, Func<string, IEmulatorCore> createCore)
    {
        ArgumentNullException.ThrowIfNull(createCore);
        CommonCoreHostFunctions.Protocol.WithSharedVideoMap(videoMapName, BufferConstants.FirstBufferIndex, videoMap =>
            CommonCoreHostFunctions.Protocol.RunConnectedSession(pipeName, CoreHostConstants.LocalPipeServerName, CoreHostConstants.PipeConnectTimeoutMilliseconds, (reader, transportWriter) =>
            {
                IEmulatorCore? core = null;
                var lastVideoSequence = VideoConstants.InitialFrameSequence;
                var lastDiagnosticCount = BufferConstants.EmptyCollectionCount;
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
                                        static requestReader => JsonSerializer.Deserialize<MachineConfiguration>(requestReader.ReadString(), CoreHostProtocol.JsonOptions) ?? throw new InvalidDataException(CommonExceptions.HostConfigurationInvalid()));
                                try
                                {
                                    CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                                }
                                finally
                                {
                                    lastVideoSequence = VideoConstants.InitialFrameSequence;
                                }

                                core = createCore(corePath);
                                core.Initialize(configuration, session, saves);
                                writer.Write(true);
                                CommonCoreHostFunctions.Protocol.WriteCoreInitialization<IEmulatorCore, MachineConfiguration, CoreOption>(
                                    writer, core, CoreHostProtocol.JsonOptions, ref lastDiagnosticCount, MediaConstants.SupportedExtensionSeparator,
                                    static (statusWriter, currentCore) =>
                                    {
                                        statusWriter.Write(currentCore.SupportsSaveStates);
                                        CommonCoreHostFunctions.Protocol.WriteRuntimeStatus(statusWriter, currentCore.Region, currentCore.BufferedAudioFrames, currentCore.AudioOverrunCount, currentCore.AudioUnderrunCount);
                                    });
                                CommonCoreHostFunctions.Protocol.WriteDiskSelection(writer, core);
                                CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                                writer.Write(core.ShutdownRequested);
                                CommonCoreHostFunctions.Protocol.WriteControllerFeedback(writer, core.ControllerFeedback);
                                break;
                            case HostCommand.RunFrame:
                                var activeCore = EnsureCore(core);
                                CommonCoreHostFunctions.Protocol.RunCoreFrame<IEmulatorCore, MachineConfiguration, CoreOption>(
                                    reader, writer, activeCore, CoreHostProtocol.ReadInput,
                                    (frameWriter, pendingFrame) => CoreHostProtocol.WriteSharedFrame(frameWriter, pendingFrame, videoMap),
                                    CommonCoreHostFunctions.Protocol.WriteDiskSelection, CoreHostProtocol.WriteLedStates, static () => CoreHostProtocol.JsonOptions,
                                    ref lastVideoSequence, ref lastDiagnosticCount);
                                writer.Write(activeCore.ShutdownRequested);
                                CommonCoreHostFunctions.Protocol.WriteControllerFeedback(writer, activeCore.ControllerFeedback);
                                break;
                            case HostCommand.Dispose:
                                CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                                WriteSuccess(writer);
                                break;
                            default:
                                if (!CommonCoreHostFunctions.Protocol.TryExecutePathMediaCommand((byte)command, reader, writer,
                                        () => EnsureCore(core), CoreHostProtocol.ReadBytes, WriteSuccess, static (statusWriter, status) => statusWriter.Write(System.Text.Json.JsonSerializer.Serialize(status, CoreHostProtocol.JsonOptions))))
                                    throw new InvalidDataException(CommonExceptions.UnknownHostCommand((byte)command));
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
                    CommonCoreHostFunctions.Protocol.DisposePreviousCore(ref core);
                }
            }));
    }

    private static IEmulatorCore EnsureCore(IEmulatorCore? core) => core ?? throw new InvalidOperationException(CommonExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
