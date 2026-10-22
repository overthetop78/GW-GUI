namespace GWGUI.Emulation.Fairchild.Common.Interfaces;

internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    IReadOnlyList<CoreOption> GetSettingsOptions();
}
