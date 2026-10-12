using System.IO;
using GWGUI.Emulation.Snk.Emulators.FbNeo.Constants;
using GWGUI.Emulation.Snk.Emulators.FbNeo.Dictionaries;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.FbNeo.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.FbNeo.Functions.MediaRuntimeFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.FbNeo.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.FbNeo.Factories;

internal sealed class FbNeoMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    internal override int DirectoryStringBufferCapacity => RuntimeOptionConstants.DirectoryStringBufferCapacity;
    public override IReadOnlyList<CoreOption> CoreOptions => OptionCatalog.Options;
    public override IReadOnlyList<CoreOption> DescribeOptions(MachineConfiguration configuration) => NativeOptions.Describe(configuration);
    public override IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => NativeFirmware.Roles;
    public override bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => category == FirmwareCategory.SystemBios;
    public override ValueTask<EmulationFirmwareCandidate> InspectFirmwareAsync(string path, MachineModel model, CancellationToken cancellationToken) => NativeFirmware.Inspect(path, model, cancellationToken);
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal override object? PreparePersistentMedia(MachineConfiguration configuration, string saveDirectory) => NativeMedia.PrepareMemoryCard(configuration, saveDirectory);
    internal override void CompletePersistentMedia(object? state) => NativeMedia.CompleteMemoryCard(state as NativeMedia.PreparedMemoryCard);
    protected override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration.Model);
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);

    public override bool IsDeferredOption(MachineConfiguration configuration, string key)
    {
        var slot = configuration.Family == MachineFamily.NeoGeoCd ? EmulationMediaSlot.Cd0 : EmulationMediaSlot.Cartridge0;
        var path = configuration.InitialContent ?? configuration.Media.FirstOrDefault(media => media.Slot == slot && media.IsInserted)?.Path;
        var driver = configuration.Model == MachineModel.NeoGeoCdz ? RuntimeOptionConstants.CdDriver : path is null ? null : Path.GetFileNameWithoutExtension(path);
        if (driver is not null && configuration.Family == MachineFamily.NeoGeoPocket && !driver.StartsWith(RuntimeOptionConstants.PocketDriverPrefix, StringComparison.Ordinal)) driver = RuntimeOptionConstants.PocketDriverPrefix + driver;
        return !string.IsNullOrEmpty(driver) && key.StartsWith(RuntimeOptionConstants.DipSwitchPrefix + driver + "-", StringComparison.Ordinal);
    }

    internal override bool LoadContent(MachineConfiguration configuration, ExternalCoreExports exports, LoadedContent? content)
    {
        var loaded = configuration.Family switch
        {
            MachineFamily.NeoGeoCd => content is not null && exports.LoadGameSpecial(RuntimeOptionConstants.CdSubsystem, content.GameInfo, RuntimeOptionConstants.ContentCount),
            MachineFamily.NeoGeoPocket => content is not null && exports.LoadGameSpecial(RuntimeOptionConstants.PocketSubsystem, content.GameInfo, RuntimeOptionConstants.ContentCount),
            _ => exports.LoadGame(content?.GameInfo ?? nint.Zero)};
        if (loaded)
            ControllerPortFunctions.Configure(exports, configuration);
        return loaded;
    }

    internal override uint ControllerDeviceId(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => peripheral == PeripheralCategory.None ? InputConstants.NoDevice : RuntimeOptionConstants.ClassicControllerDevice;
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
            result[ControllerCommand.Option] = InputConstants.JoypadYId;
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
