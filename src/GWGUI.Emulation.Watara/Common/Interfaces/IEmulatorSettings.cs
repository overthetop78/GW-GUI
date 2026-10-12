namespace GWGUI.Emulation.Watara.Common.Interfaces;

internal interface IEmulatorSettings
{
    IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration);
}
