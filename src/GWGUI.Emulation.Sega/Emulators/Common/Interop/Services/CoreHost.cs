using GWGUI.Emulation.Sega.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Enums;

using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Text.Json;

using System.IO;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

public static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    public static void Run(string pipeName, string videoMapName)
    {
        var previousErrorMode = GetErrorMode();
        SetErrorMode(previousErrorMode | CoreHostConstants.SuppressCriticalErrorDialogs | CoreHostConstants.SuppressFaultDialogs);
        try { RunSession(pipeName, videoMapName); }
        finally { SetErrorMode(previousErrorMode); }
    }

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    private static void RunSession(string pipeName, string videoMapName)
    {
        EmulationHostProtocolFunctions.WithSharedVideoMap(videoMapName, 0, videoMap =>
            EmulationHostProtocolFunctions.RunConnectedSession(pipeName, CoreHostConstants.LocalPipeServer, CoreHostConstants.ConnectionTimeoutMilliseconds, (reader, transportWriter) =>
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
                                core = new ExternalCore(corePath);
                                core.Initialize(configuration, session, saves);
                                writer.Write(true);
                                EmulationHostProtocolFunctions.WriteCoreInitialization<ExternalCore, MachineConfiguration, CoreOption>(
                                    writer, core, CoreHostProtocol.JsonOptions, ref lastDiagnosticCount, '|');
                                writer.Write(core.DiskCount);
                                writer.Write(core.CurrentDiskIndex);
                                CoreHostProtocol.WriteLedStates(writer, core.LedStates);
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
                            case HostCommand.Dispose:
                                core?.Dispose();
                                core = null;
                                WriteSuccess(writer);
                                break;
                            default:
                                if (!EmulationHostProtocolFunctions.TryExecuteCoreStateCommand((byte)command, reader, writer,
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
                    core?.Dispose();
                }
            }));
    }

    private static ExternalCore EnsureCore(ExternalCore? core) => core ?? throw new InvalidOperationException(CoreExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);

    [DllImport(CoreHostConstants.KernelLibrary)]
    private static extern uint GetErrorMode();

    [DllImport(CoreHostConstants.KernelLibrary)]
    private static extern uint SetErrorMode(uint mode);
}
