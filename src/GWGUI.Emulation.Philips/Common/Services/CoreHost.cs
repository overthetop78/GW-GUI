using System.IO;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Philips.Common.Services;

public static class CoreHost
                {
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void Run(string pipeName, string videoMapName, Func<string, Emulator, IEmulatorCore> createCore)
    {
        ArgumentNullException.ThrowIfNull(createCore);
        using var video = new SharedVideoWriter(videoMapName);
        EmulationHostProtocolFunctions.RunConnectedSession(pipeName, CoreHostConstants.LocalPipeServerName, CoreHostConstants.ConnectionTimeoutMilliseconds, (reader, transportWriter) =>
        {
            IEmulatorCore? core = null;
            var lastVideoSequence = CoreHostConstants.InitialVideoSequence;
            var lastDiagnosticCount = CoreHostConstants.InitialDiagnosticCount;
            try
            {
                EmulationHostProtocolFunctions.RunCommandLoop<HostCommand>(reader, transportWriter, static commandReader => CoreHostFunctions.ReadRequestHeader(commandReader), HostCommand.Dispose, (command, writer) =>
                {
                    CoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Success);
                    switch (command)
                    {
                        case HostCommand.Initialize:
                            Initialize(reader, writer, createCore, ref core, ref lastDiagnosticCount);
                            lastVideoSequence = CoreHostConstants.InitialVideoSequence;
                            break;
                        case HostCommand.RunFrame:
                            RunFrame(reader, writer, video, EnsureCore(core), ref lastVideoSequence, ref lastDiagnosticCount);
                            break;
                        case HostCommand.SaveMediaChanges:
                            EnsureCore(core).SaveMediaChanges(EmulationMediaSlot.FromProtocolValue(reader.ReadInt32()));
                            break;
                        case HostCommand.HasUnsavedMediaChanges:
                            writer.Write(EnsureCore(core).HasUnsavedMediaChanges(EmulationMediaSlot.FromProtocolValue(reader.ReadInt32())));
                            break;
                        case HostCommand.Dispose:
                            core?.Dispose();
                            core = null;
                            break;
                        default:
                            if (!EmulationHostProtocolFunctions.TryExecuteCoreCommand<MediaConfiguration, PeripheralCategory, DiskStatus>(
                                (byte)command, reader, writer, () => EnsureCore(core), ReadMedia,
                                CoreHostFunctions.ReadBytes, CoreHostFunctions.WriteDiskStatus,
                                static value => (PeripheralCategory)value))
                                throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, CoreHostErrors.UnknownCommandFormat, (byte)command));
                            break;
                    }
                }, (writer, error) =>
                {
                    CoreHostFunctions.WriteResponseHeader(writer, HostResponseStatus.Failure);
                    CoreHostFunctions.WriteError(writer, error);
                });
            }
            finally
            {
                core?.Dispose();
            }
        });
    }

    private static void Initialize(BinaryReader reader, BinaryWriter writer, Func<string, Emulator, IEmulatorCore> createCore, ref IEmulatorCore? core, ref int lastDiagnosticCount)
    {
        var previousCore = core;
        core = null;
        try { previousCore?.Dispose(); }
        finally { core = null; }
        var corePath = reader.ReadString();
        var emulator = (Emulator)reader.ReadInt32();
        var session = reader.ReadString();
        var saves = CoreHostFunctions.ReadString(reader);
        var configuration = JsonSerializer.Deserialize<MachineConfiguration>(reader.ReadString(), CoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.InvalidConfiguration);
        core = createCore(corePath, emulator);
        core.Initialize(configuration, session, saves);
        EmulationHostProtocolFunctions.WriteCoreInitialization<IEmulatorCore, MachineConfiguration, CoreOption>(
            writer, core, CoreHostFunctions.JsonOptions, ref lastDiagnosticCount, CoreHostConstants.ExtensionListSeparator,
            static (statusWriter, currentCore) =>
            {
                statusWriter.Write(currentCore.SupportsSaveStates);
                EmulationHostProtocolFunctions.WriteRuntimeStatus(statusWriter, currentCore.Region, currentCore.BufferedAudioFrames, currentCore.AudioOverrunCount, currentCore.AudioUnderrunCount);
            });
        CoreHostFunctions.WriteLedStates(writer, core.LedStates);
    }

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    private static void RunFrame(BinaryReader reader, BinaryWriter writer, SharedVideoWriter video, IEmulatorCore core, ref long lastVideoSequence, ref int lastDiagnosticCount)
    {
        core.SetInput(CoreHostFunctions.ReadInput(reader));
        core.RunFrame();
        EmulationHostProtocolFunctions.WriteFrameIfChanged(writer, core.LatestVideoFrame, ref lastVideoSequence,
            (frameWriter, pendingFrame) => CoreHostFunctions.WriteResizableSharedFrame(frameWriter, pendingFrame, video));
        var bufferedAudioFrames = core.BufferedAudioFrames;
        var audioOverrunCount = core.AudioOverrunCount;
        var audioUnderrunCount = core.AudioUnderrunCount;
        EmulationHostProtocolFunctions.WriteCoreAudio(writer, core);
        EmulationHostProtocolFunctions.WriteRuntimeStatus(writer, core.Region, bufferedAudioFrames, audioOverrunCount, audioUnderrunCount);
        writer.Write(core.SupportsSaveStates);
        EmulationHostProtocolFunctions.WriteChangedDiagnostics(writer, () => core.Diagnostics.Count,
            diagnosticWriter => diagnosticWriter.Write(JsonSerializer.Serialize(core.Diagnostics, CoreHostFunctions.JsonOptions)), ref lastDiagnosticCount);
        CoreHostFunctions.WriteLedStates(writer, core.LedStates);
    }

    private static MediaConfiguration ReadMedia(BinaryReader reader) => JsonSerializer.Deserialize<MediaConfiguration>(reader.ReadString(), CoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.InvalidConfiguration);

    private static IEmulatorCore EnsureCore(IEmulatorCore? core) => core ?? throw new InvalidOperationException(CoreHostErrors.NotInitialized);
}
