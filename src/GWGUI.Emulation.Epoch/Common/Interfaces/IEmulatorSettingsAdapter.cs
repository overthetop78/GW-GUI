namespace GWGUI.Emulation.Epoch.Common.Interfaces;

internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    IReadOnlyList<CoreOption> GetSettingsOptions();
}
