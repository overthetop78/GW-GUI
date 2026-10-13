using GWGUI.Emulation.Snk.Emulators.Geolith.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Snk.Emulators.Geolith.Constants;
using GWGUI.Emulation.Snk.Emulators.Geolith.Dictionaries;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.Geolith.Functions.MediaRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.Geolith.Functions.RuntimeOptionFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.Geolith.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Geolith.Factories;
internal sealed class GeolithMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    public override IReadOnlyList<CoreOption> CoreOptions => OptionCatalog.Options;

    public override IReadOnlyList<CoreOption> DescribeOptions(MachineConfiguration configuration) => NativeOptions.Describe(configuration);
    public override int ControllerPortCount(MachineConfiguration configuration) => NativeOptions.ControllerPorts(configuration);
    public override IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => NativeFirmware.Roles(model);
    public override bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => NativeFirmware.Required(model, category);
    public override ValueTask<EmulationFirmwareCandidate> InspectFirmwareAsync(string path, MachineModel model, CancellationToken cancellationToken) => NativeFirmware.Inspect(path, model, cancellationToken);
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal override object? PreparePersistentMedia(MachineConfiguration configuration, string saveDirectory) => NativeMedia.PrepareMemoryCard(configuration, saveDirectory);
    internal override void CompletePersistentMedia(object? state) => NativeMedia.CompleteMemoryCard(state as NativeMedia.PreparedMemoryCard);
    protected override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration);
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    internal override IReadOnlyDictionary<ControllerCommand, uint> ControllerButtonIds(MachineConfiguration configuration) => new Dictionary<ControllerCommand, uint>
    {
        [ControllerCommand.Up] = InputConstants.JoypadUpId,
        [ControllerCommand.Down] = InputConstants.JoypadDownId,
        [ControllerCommand.Left] = InputConstants.JoypadLeftId,
        [ControllerCommand.Right] = InputConstants.JoypadRightId,
        [ControllerCommand.A] = InputConstants.JoypadBId,
        [ControllerCommand.B] = InputConstants.JoypadAId,
        [ControllerCommand.C] = InputConstants.JoypadYId,
        [ControllerCommand.D] = InputConstants.JoypadXId,
        [ControllerCommand.Start] = InputConstants.JoypadStartId,
        [ControllerCommand.Select] = InputConstants.JoypadSelectId
    };
}