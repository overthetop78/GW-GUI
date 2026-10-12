namespace GWGUI.Emulation.Sharp.Common.Interfaces;

internal interface IEmulatorSettings
{
    IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration);
    IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null);
}
