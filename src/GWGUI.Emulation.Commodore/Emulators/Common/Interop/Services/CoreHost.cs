using CommonCoreHostFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreHostFunctions;
using System.IO;

using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;

public static class CoreHost
            {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    public static void Run(string pipeName, string videoMapName)
    {
        CommonCoreHostFunctions.Protocol.WithSharedVideoMap(videoMapName, ProcessCoreConstants.ViewStartOffset, videoMap =>
            CommonCoreHostFunctions.Protocol.RunConnectedSession(pipeName, CoreHostConstants.LocalServerName, CoreHostConstants.PipeConnectionTimeoutMilliseconds, (reader, transportWriter) =>
            {
                ExternalCore? core = null;
                var lastVideoSequence = CoreHostConstants.InitialVideoSequence;
                var lastDiagnosticCount = CoreHostConstants.InitialDiagnosticCount;
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
                                core = new ExternalCore(corePath);
                                core.Initialize(configuration, session, saves);
                                writer.Write(true);
                                CommonCoreHostFunctions.Protocol.WriteCoreInitialization<ExternalCore, MachineConfiguration, CoreOption>(
                                    writer, core, CoreHostProtocol.JsonOptions, ref lastDiagnosticCount, ProcessCoreConstants.ExtensionSeparator);
                                CommonCoreHostFunctions.Protocol.WriteDiskSelection(writer, core);
                                CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                                break;
                            case HostCommand.RunFrame:
                                var activeCore = EnsureCore(core);
                                CommonCoreHostFunctions.Protocol.RunCoreFrame<ExternalCore, MachineConfiguration, CoreOption>(
                                    reader, writer, activeCore, CoreHostProtocol.ReadInput,
                                    (frameWriter, pendingFrame) => CoreHostProtocol.WriteSharedFrame(frameWriter, pendingFrame, videoMap),
                                    CommonCoreHostFunctions.Protocol.WriteDiskSelection, CoreHostProtocol.WriteLedStates, static () => CoreHostProtocol.JsonOptions,
                                    ref lastVideoSequence, ref lastDiagnosticCount);
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
                        writer.Write(error.ToString());
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
