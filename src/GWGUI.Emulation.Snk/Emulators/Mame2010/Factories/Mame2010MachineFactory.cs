using GWGUI.Emulation.Snk.Emulators.Mame2010.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2010.Dictionaries;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.Mame2010.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.Mame2010.Functions.MediaRuntimeFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.Mame2010.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame2010.Factories;

internal sealed class Mame2010MachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    public override IReadOnlyList<CoreOption> CoreOptions => OptionCatalog.Options;
    public override IReadOnlyList<CoreOption> DescribeOptions(MachineConfiguration configuration) => NativeOptions.Describe(configuration);
    public override IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => NativeFirmware.Roles;
    public override bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => category == FirmwareCategory.SystemBios;
    public override ValueTask<EmulationFirmwareCandidate> InspectFirmwareAsync(string path, MachineModel model, CancellationToken cancellationToken) => NativeFirmware.Inspect(path, model, cancellationToken);
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    protected override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration.Model);
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    internal override IReadOnlyDictionary<ControllerCommand, uint> ControllerButtonIds(MachineConfiguration configuration) => new Dictionary<ControllerCommand, uint>
    {
        [ControllerCommand.Up] = InputConstants.JoypadUpId,
        [ControllerCommand.Down] = InputConstants.JoypadDownId,
        [ControllerCommand.Left] = InputConstants.JoypadLeftId,
        [ControllerCommand.Right] = InputConstants.JoypadRightId,
        [ControllerCommand.A] = InputConstants.JoypadAId,
        [ControllerCommand.B] = InputConstants.JoypadBId,
        [ControllerCommand.C] = InputConstants.JoypadXId,
        [ControllerCommand.D] = InputConstants.JoypadYId,
        [ControllerCommand.Start] = InputConstants.JoypadStartId,
        [ControllerCommand.Select] = InputConstants.JoypadSelectId
    };
}
