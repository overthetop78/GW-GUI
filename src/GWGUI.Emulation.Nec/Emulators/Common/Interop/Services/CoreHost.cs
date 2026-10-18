using GWGUI.Emulation.Nec.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Enums;
using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;
using System.IO;



namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

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
                            var corePath = reader.ReadString();
                            var session = reader.ReadString();
                            var saves = CoreHostProtocol.ReadString(reader);
                            var configuration = JsonSerializer.Deserialize<MachineConfiguration>(reader.ReadString(), CoreHostProtocol.JsonOptions) ?? throw new InvalidDataException(CoreExceptions.HostConfigurationInvalid());
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
                            activeCore.SetInput(CoreHostProtocol.ReadInput(reader));
                            activeCore.RunFrame();
                            writer.Write(true);
                            EmulationHostProtocolFunctions.WriteFrameIfChanged(writer, activeCore.LatestVideoFrame, ref lastVideoSequence,
                                (frameWriter, pendingFrame) => CoreHostProtocol.WriteSharedFrame(frameWriter, pendingFrame, videoMap));
                            EmulationHostProtocolFunctions.WriteCoreAudio(writer, activeCore);
                            writer.Write(activeCore.DiskCount);
                            writer.Write(activeCore.CurrentDiskIndex);
                            var diagnostics = activeCore.Diagnostics;
                            EmulationHostProtocolFunctions.WriteChangedDiagnostics(writer, () => diagnostics.Count,
                                diagnosticWriter => diagnosticWriter.Write(JsonSerializer.Serialize(diagnostics, CoreHostProtocol.JsonOptions)), ref lastDiagnosticCount);

                            CoreHostProtocol.WriteLedStates(writer, activeCore.LedStates);
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
                    writer.Write((byte)(error is GWGUI.Emulation.Nec.Common.Exceptions.CoreMediaRequiredException ? GWGUI.Emulation.Nec.Common.Enums.CoreHostErrorKind.MediaRequired : GWGUI.Emulation.Nec.Common.Enums.CoreHostErrorKind.General));
                    writer.Write(error.ToString());
                });
            }
            finally
            {
                core?.Dispose();
            }
        });
    }

    private static ExternalCore EnsureCore(ExternalCore? core) => core ?? throw new InvalidOperationException(CoreExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
