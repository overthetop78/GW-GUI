using System.IO;
using GWGUI.Emulation.Amstrad.Emulators.Common.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Services;

public static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void Run(string pipeName, string videoMapName, Func<string, IEmulatorCore> createCore)
    {
        ArgumentNullException.ThrowIfNull(createCore);
        EmulationHostProtocolFunctions.WithSharedVideoMap(videoMapName, BufferConstants.FirstBufferIndex, videoMap =>
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
                                var (corePath, session, saves, configuration) =
                                    EmulationHostProtocolFunctions.ReadInitializationRequest(reader,
                                        CoreHostProtocol.ReadString,
                                        static requestReader => JsonSerializer.Deserialize<MachineConfiguration>(requestReader.ReadString(), CoreHostProtocol.JsonOptions) ?? throw new InvalidDataException(CommonExceptions.HostConfigurationInvalid()));
                                try
                                {
                                    EmulationHostProtocolFunctions.DisposePreviousCore(ref core);
                                }
                                finally
                                {
                                    lastVideoSequence = VideoConstants.InitialFrameSequence;
                                }

                                core = createCore(corePath);
                                core.Initialize(configuration, session, saves);
                                writer.Write(true);
                                EmulationHostProtocolFunctions.WriteCoreInitialization<IEmulatorCore, MachineConfiguration, CoreOption>(
                                    writer, core, CoreHostProtocol.JsonOptions, ref lastDiagnosticCount, MediaConstants.SupportedExtensionSeparator);
                                EmulationHostProtocolFunctions.WriteDiskSelection(writer, core);
                                CoreHostProtocol.WriteLedStates(writer, core.LedStates);
                                break;
                            case HostCommand.RunFrame:
                                var activeCore = EnsureCore(core);
                                EmulationHostProtocolFunctions.RunCoreFrame<IEmulatorCore, MachineConfiguration, CoreOption>(
                                    reader, writer, activeCore, CoreHostProtocol.ReadInput,
                                    (frameWriter, pendingFrame) => CoreHostProtocol.WriteSharedFrame(frameWriter, pendingFrame, videoMap),
                                    EmulationHostProtocolFunctions.WriteDiskSelection, CoreHostProtocol.WriteLedStates, static () => CoreHostProtocol.JsonOptions,
                                    ref lastVideoSequence, ref lastDiagnosticCount);
                                break;
                            case HostCommand.SoftReset:
                                EnsureCore(core).SoftReset();
                                WriteSuccess(writer);
                                break;
                            case HostCommand.Dispose:
                                EmulationHostProtocolFunctions.DisposePreviousCore(ref core);
                                WriteSuccess(writer);
                                break;
                            default:
                                if (!EmulationHostProtocolFunctions.TryExecutePathMediaCommand((byte)command, reader, writer,
                                        () => EnsureCore(core), CoreHostProtocol.ReadBytes, WriteSuccess))
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
                    EmulationHostProtocolFunctions.DisposePreviousCore(ref core);
                }
            }));
    }

    private static IEmulatorCore EnsureCore(IEmulatorCore? core) => core ?? throw new InvalidOperationException(CommonExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
