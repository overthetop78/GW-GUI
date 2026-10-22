namespace GWGUI.Emulation.Bandai.Common.Interfaces;

internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    IReadOnlyList<CoreOption> GetSettingsOptions();
}