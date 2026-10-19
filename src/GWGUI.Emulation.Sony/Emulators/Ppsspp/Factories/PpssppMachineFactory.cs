using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Factories;

internal sealed class PpssppMachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => PpssppConstants.Definition;
    public override IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration) =>
        CoreSettingsFunctions.Blocks(PpssppOptionFunctions.Options(configuration), configuration);
    public override MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) =>
        PpssppOptionFunctions.ToNative(configuration with { GraphicsApi = graphicsApi });
    public override int GetMemoryKib(MachineConfiguration configuration) =>
        configuration.Options?.GetValueOrDefault(OptionConstants.PspModelKey) != PpssppConstants.Psp1000Model ? PpssppConstants.ExpandedMemoryKib : ModelCatalog.Get(configuration.Model).RamKib;
    public override int GetMemoryCardCount(MachineConfiguration configuration) => 1;
    public override bool IsCpuClock(string fieldId) => false;
    public override IReadOnlyList<EmulationSettingsBlock> GetMachineSettingsBlocks(MachineConfiguration configuration) =>
        CoreSettingsFunctions.MachineBlocks(this, configuration);
    public override IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) =>
        CoreSettingsFunctions.StorageFields(this, configuration, category);
    protected override MachineConfiguration Configure(MachineConfiguration configuration) => PpssppOptionFunctions.ToNative(configuration);
}