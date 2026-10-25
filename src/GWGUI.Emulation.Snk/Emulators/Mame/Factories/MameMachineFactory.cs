using GWGUI.Emulation.Snk.Emulators.Mame.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame.Dictionaries;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.Mame.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.Mame.Functions.MediaRuntimeFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.Mame.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame.Factories;
internal sealed class MameMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    public override IReadOnlyList<CoreOption> CoreOptions => OptionCatalog.Options;

    public override IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => configuration is null ? CoreOptions : NativeOptions.Describe(configuration);
    protected override IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => NativeFirmware.Roles;
    protected override bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => category == FirmwareCategory.SystemBios;
    public override async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => [await (NativeFirmware.Inspect(path, configuration.Model, cancellationToken)).ConfigureAwait(false)];
    public override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal override object? PreparePersistentMedia(MachineConfiguration configuration, string saveDirectory, LoadedContent? content) => NativeMedia.PrepareMemoryCard(configuration, content);
    internal override void CompletePersistentMedia(object? state) => NativeMedia.CompleteMemoryCard(state as GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.MameMediaFunctions.PreparedMemoryCard);
    public override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration.Model);
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    internal override IReadOnlyDictionary<ControllerCommand, uint> ControllerButtonIds(MachineConfiguration configuration)
    {
        var result = new Dictionary<ControllerCommand, uint>
        {
            [ControllerCommand.Up] = InputConstants.JoypadUpId,
            [ControllerCommand.Down] = InputConstants.JoypadDownId,
            [ControllerCommand.Left] = InputConstants.JoypadLeftId,
            [ControllerCommand.Right] = InputConstants.JoypadRightId,
            [ControllerCommand.A] = InputConstants.JoypadBId,
            [ControllerCommand.B] = InputConstants.JoypadAId
        };
        if (configuration.Family == MachineFamily.NeoGeoPocket)
            result[ControllerCommand.Option] = InputConstants.JoypadSelectId;
        else
        {
            result[ControllerCommand.C] = InputConstants.JoypadYId;
            result[ControllerCommand.D] = InputConstants.JoypadXId;
            result[ControllerCommand.Start] = InputConstants.JoypadStartId;
            result[ControllerCommand.Select] = InputConstants.JoypadSelectId;
        }

        return result;
    }
}
