using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using System.IO;
using GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Constants;
using GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Dictionaries;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Functions.MediaRuntimeFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Factories;
internal sealed class FbAlpha2012MachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    public override IReadOnlyList<CoreOption> CoreOptions => OptionCatalog.Options;

    public override IReadOnlyList<CoreOption> DescribeOptions(MachineConfiguration configuration) => NativeOptions.Describe(configuration);
    public override IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => NativeFirmware.Roles;
    public override bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => category == FirmwareCategory.SystemBios;
    public override ValueTask<EmulationFirmwareCandidate> InspectFirmwareAsync(string path, MachineModel model, CancellationToken cancellationToken) => NativeFirmware.Inspect(path, model, cancellationToken);
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    protected override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices;
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    public override bool IsDeferredOption(MachineConfiguration configuration, string key)
    {
        var path = configuration.InitialContent ?? configuration.Media.FirstOrDefault(media => media.Slot == EmulationMediaSlot.Cartridge0 && media.IsInserted)?.Path;
        var driver = path is null ? null : Path.GetFileNameWithoutExtension(path);
        return !string.IsNullOrEmpty(driver) && key.StartsWith(RuntimeOptionConstants.DipSwitchPrefix + driver + "_", StringComparison.Ordinal);
    }

    internal override bool LoadContent(MachineConfiguration configuration, ExternalCoreExports exports, LoadedContent? content)
    {
        var loaded = exports.LoadGame(content?.GameInfo ?? nint.Zero);
        if (loaded)
            ControllerPortFunctions.Configure(exports, configuration);
        return loaded;
    }

    internal override uint ControllerDeviceId(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => peripheral == PeripheralCategory.None ? InputConstants.NoDevice : RuntimeOptionConstants.ClassicControllerDevice;
}