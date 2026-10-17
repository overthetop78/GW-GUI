using GWGUI.Emulation.Snk.Emulators.Mame2016.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2016.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2016.Dictionaries;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.Mame2016.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.Mame2016.Functions.MediaRuntimeFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.Mame2016.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame2016.Factories;
internal sealed class Mame2016MachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    public override IReadOnlyList<CoreOption> CoreOptions => OptionCatalog.Options;

    public override IReadOnlyList<CoreOption> DescribeOptions(MachineConfiguration configuration) => NativeOptions.Describe(configuration);
    public override IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => NativeFirmware.Roles;
    public override bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => category == FirmwareCategory.SystemBios;
    public override ValueTask<EmulationFirmwareCandidate> InspectFirmwareAsync(string path, MachineModel model, CancellationToken cancellationToken) => NativeFirmware.Inspect(path, model, cancellationToken);
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal override object? PreparePersistentMedia(MachineConfiguration configuration, string saveDirectory, LoadedContent? content) => NativeMedia.PrepareMemoryCard(configuration, content);
    internal override void CompletePersistentMedia(object? state) => NativeMedia.CompleteMemoryCard(state as GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.MameMediaFunctions.PreparedMemoryCard);
    protected override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration.Model);
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    internal override IReadOnlyDictionary<ControllerCommand, uint> ControllerButtonIds(MachineConfiguration configuration)
    {
        var arcade = configuration.Model == MachineModel.NeoGeoMvs;
        var result = new Dictionary<ControllerCommand, uint>
        {
            [ControllerCommand.Up] = InputConstants.JoypadUpId,
            [ControllerCommand.Down] = InputConstants.JoypadDownId,
            [ControllerCommand.Left] = InputConstants.JoypadLeftId,
            [ControllerCommand.Right] = InputConstants.JoypadRightId,
            [ControllerCommand.A] = arcade ? InputConstants.JoypadBId : InputConstants.JoypadAId,
            [ControllerCommand.B] = arcade ? InputConstants.JoypadAId : InputConstants.JoypadBId
        };
        if (configuration.Family == MachineFamily.NeoGeoPocket)
            result[ControllerCommand.Option] = InputConstants.JoypadSelectId;
        else
        {
            result[ControllerCommand.C] = arcade ? InputConstants.JoypadYId : InputConstants.JoypadXId;
            result[ControllerCommand.D] = arcade ? InputConstants.JoypadXId : InputConstants.JoypadYId;
            result[ControllerCommand.Start] = InputConstants.JoypadStartId;
            result[ControllerCommand.Select] = InputConstants.JoypadSelectId;
        }

        return result;
    }
}
