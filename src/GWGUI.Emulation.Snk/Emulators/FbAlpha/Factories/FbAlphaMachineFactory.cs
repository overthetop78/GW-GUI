using System.IO;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Constants;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Dictionaries;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions.MediaRuntimeFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha.Factories;

internal sealed class FbAlphaMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
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
    public override bool IsDeferredOption(MachineConfiguration configuration, string key)
    {
        var path = configuration.InitialContent ?? configuration.Media.FirstOrDefault(media => media.Slot == EmulationMediaSlot.Cartridge0 && media.IsInserted)?.Path;
        var driver = configuration.Model == MachineModel.NeoGeoCdz ? RuntimeOptionConstants.CdDriver : path is null ? null : Path.GetFileNameWithoutExtension(path);
        return !string.IsNullOrEmpty(driver) && key.StartsWith(RuntimeOptionConstants.DipSwitchPrefix + driver + "-", StringComparison.Ordinal);
    }

    internal override bool LoadContent(MachineConfiguration configuration, ExternalCoreExports exports, LoadedContent? content)
    {
        var loaded = configuration.Model == MachineModel.NeoGeoCdz ? content is not null && exports.LoadGameSpecial(RuntimeOptionConstants.CdSubsystem, content.GameInfo, RuntimeOptionConstants.CdContentCount) : exports.LoadGame(content?.GameInfo ?? nint.Zero);
        if (loaded) ControllerPortFunctions.Configure(exports, configuration);
        return loaded;
    }

    internal override uint ControllerDeviceId(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => peripheral == PeripheralCategory.None ? InputConstants.NoDevice : RuntimeOptionConstants.ClassicControllerDevice;
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
