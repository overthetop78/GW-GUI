using System.IO;
using GWGUI.Emulation.Amstrad.Emulators.Common.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Services;

public static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void Run(string pipeName, string videoMapName, Func<string, IEmulatorCore> createCore)
    {
        ArgumentNullException.ThrowIfNull(createCore);
        using var videoMemory = MemoryMappedFile.OpenExisting(videoMapName, MemoryMappedFileRights.ReadWrite);
        using var videoMap = videoMemory.CreateViewAccessor(BufferConstants.FirstBufferIndex, EmulationHostProtocolConstants.VideoMapCapacity, MemoryMappedFileAccess.ReadWrite);
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
                            var previousCore = core;
                            core = null;
                            try
                            {
                                previousCore?.Dispose();
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
                core?.Dispose();
            }
        });
    }

    private static IEmulatorCore EnsureCore(IEmulatorCore? core) => core ?? throw new InvalidOperationException(CommonExceptions.HostNotInitialized());
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
