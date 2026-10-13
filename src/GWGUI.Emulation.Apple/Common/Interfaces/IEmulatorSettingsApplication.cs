namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorSettingsApplication
{
    MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values);
}
