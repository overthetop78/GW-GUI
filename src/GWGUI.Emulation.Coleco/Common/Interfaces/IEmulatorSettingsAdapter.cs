namespace GWGUI.Emulation.Coleco.Common.Interfaces;

internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    IReadOnlyList<CoreOption> GetSettingsOptions();
}
