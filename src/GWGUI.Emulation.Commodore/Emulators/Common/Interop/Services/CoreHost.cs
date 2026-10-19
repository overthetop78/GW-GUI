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
        EmulationHostProtocolFunctions.WithSharedVideoMap(videoMapName, ProcessCoreConstants.ViewStartOffset, videoMap =>
            EmulationHostProtocolFunctions.RunConnectedSession(pipeName, CoreHostConstants.LocalServerName, CoreHostConstants.PipeConnectionTimeoutMilliseconds, (reader, transportWriter) =>
            {
                ExternalCore? core = null;
                var lastVideoSequence = CoreHostConstants.InitialVideoSequence;
                var lastDiagnosticCount = CoreHostConstants.InitialDiagnosticCount;
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
                                core = new ExternalCore(corePath);
                                core.Initialize(configuration, session, saves);
                                writer.Write(true);
                                EmulationHostProtocolFunctions.WriteCoreInitialization<ExternalCore, MachineConfiguration, CoreOption>(
                                    writer, core, CoreHostProtocol.JsonOptions, ref lastDiagnosticCount, ProcessCoreConstants.ExtensionSeparator);
                                EmulationHostProtocolFunctions.WriteDiskSelection(writer, core);
                                CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                                break;
                            case HostCommand.RunFrame:
                                var activeCore = EnsureCore(core);
                                EmulationHostProtocolFunctions.RunCoreFrame<ExternalCore, MachineConfiguration, CoreOption>(
                                    reader, writer, activeCore, CoreHostProtocol.ReadInput,
                                    (frameWriter, pendingFrame) => CoreHostProtocol.WriteSharedFrame(frameWriter, pendingFrame, videoMap),
                                    EmulationHostProtocolFunctions.WriteDiskSelection, CoreHostProtocol.WriteLedStates, static () => CoreHostProtocol.JsonOptions,
                                    ref lastVideoSequence, ref lastDiagnosticCount);
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
                        writer.Write(error.ToString());
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
