using GWGUI.Emulation.Philips.Emulators.SameCdi.Constants;

using System.IO;

namespace GWGUI.Emulation.Philips.Emulators.SameCdi.Factories;

internal sealed class SameCdiMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) =>
        path is not null && string.Equals(Path.GetExtension(path),
            MediaRuntimeConstants.PlaylistExtension, StringComparison.OrdinalIgnoreCase);

    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) =>
        GWGUI.Emulation.Philips.Emulators.SameCdi.Functions.SameCdiRuntimeFunctions.Options(configuration);
}
