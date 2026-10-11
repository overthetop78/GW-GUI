using GWGUI.Emulation.Sinclair.Emulators.EightyOne.Constants;
using NativeMedia = GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions.MediaRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Sinclair.Emulators.EightyOne.Factories;

internal sealed class EightyOneMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    protected override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) { }
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => [new(EmulationMediaSlot.Cassette0, EmulationMediaType.Cassette, MediaRuntimeConstants.TapeExtensions, RequiresMachineRecreation: true)];
}
