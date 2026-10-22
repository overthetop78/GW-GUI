namespace GWGUI.Emulation.Sharp.Common.Interfaces;

internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null);
}
