using GWGUI.Emulation.Philips.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Philips.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Philips.Emulators.SameCdi.Functions;
using Settings = GWGUI.Emulation.Philips.Emulators.SameCdi.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Philips.Emulators.SameCdi.Constants;

using System.IO;

namespace GWGUI.Emulation.Philips.Emulators.SameCdi.Factories;

internal sealed class SameCdiMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter, IEmulatorStorageSettings
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();
    public IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.SameCdi.Functions.StorageDescriptionFunctions.Describe(configuration);

    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => path is not null && string.Equals(Path.GetExtension(path), MediaRuntimeConstants.PlaylistExtension, StringComparison.OrdinalIgnoreCase);

    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.SameCdi.Functions.SameCdiRuntimeFunctions.Options(configuration);
}
