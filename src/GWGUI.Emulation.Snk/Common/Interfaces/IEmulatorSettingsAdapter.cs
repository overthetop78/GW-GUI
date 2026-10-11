namespace GWGUI.Emulation.Snk.Common.Interfaces;

internal interface IEmulatorSettingsAdapter
{
    IReadOnlyList<CoreOption> CoreOptions { get; }
    IReadOnlyList<CoreOption> DescribeOptions(MachineConfiguration configuration);
    bool IsDeferredOption(MachineConfiguration configuration, string key);
}
