namespace GWGUI.Emulation.Philips.Common.Interfaces;

internal interface IEmulatorSettings
{
    IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration);
    IReadOnlyList<CoreOption> GetSettingsOptions();
}
