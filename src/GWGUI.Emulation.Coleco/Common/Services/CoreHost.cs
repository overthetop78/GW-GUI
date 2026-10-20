using System.IO;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Coleco.Common.Services;

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
                            EmulationHostProtocolFunctions.DisposePreviousCore(ref core);
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
                EmulationHostProtocolFunctions.DisposePreviousCore(ref core);
            }
        });
    }

    private static void Initialize(BinaryReader reader, BinaryWriter writer, Func<string, Emulator, IEmulatorCore> createCore, ref IEmulatorCore? core, ref int lastDiagnosticCount)
    {
        EmulationHostProtocolFunctions.InitializeCore<IEmulatorCore, Emulator, MachineConfiguration, CoreOption>(
            reader, writer, createCore, ref core, ref lastDiagnosticCount,
            static requestReader => (Emulator)requestReader.ReadInt32(), CoreHostFunctions.ReadString,
            static requestReader => JsonSerializer.Deserialize<MachineConfiguration>(requestReader.ReadString(), CoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.InvalidConfiguration),
            static () => CoreHostFunctions.JsonOptions, CoreHostConstants.ExtensionListSeparator,
            static (statusWriter, currentCore) =>
            {
                statusWriter.Write(currentCore.SupportsSaveStates);
                EmulationHostProtocolFunctions.WriteRuntimeStatus(statusWriter, currentCore.Region, currentCore.BufferedAudioFrames, currentCore.AudioOverrunCount, currentCore.AudioUnderrunCount);
            }, CoreHostFunctions.WriteLedStates);
    }

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    private static void RunFrame(BinaryReader reader, BinaryWriter writer, SharedVideoWriter video, IEmulatorCore core, ref long lastVideoSequence, ref int lastDiagnosticCount)
    {
        core.SetInput(CoreHostFunctions.ReadInput(reader));
        core.RunFrame();
        EmulationHostProtocolFunctions.WriteNativeCoreFrameResponse<IEmulatorCore, MachineConfiguration, CoreOption>(
            writer, core, (frameWriter, pendingFrame) => CoreHostFunctions.WriteResizableSharedFrame(frameWriter, pendingFrame, video),
            CoreHostFunctions.WriteLedStates, static () => CoreHostFunctions.JsonOptions,
            ref lastVideoSequence, ref lastDiagnosticCount);
    }

    private static MediaConfiguration ReadMedia(BinaryReader reader) => JsonSerializer.Deserialize<MediaConfiguration>(reader.ReadString(), CoreHostFunctions.JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.InvalidConfiguration);

    private static IEmulatorCore EnsureCore(IEmulatorCore? core) => core ?? throw new InvalidOperationException(CoreHostErrors.NotInitialized);
}
