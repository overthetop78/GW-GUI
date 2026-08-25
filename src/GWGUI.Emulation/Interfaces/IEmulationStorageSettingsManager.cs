namespace GWGUI.Emulation.Interfaces;

public interface IEmulationStorageSettingsManager
{
    EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration);
    IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration,
        EmulationStorageSettings settings);
}
