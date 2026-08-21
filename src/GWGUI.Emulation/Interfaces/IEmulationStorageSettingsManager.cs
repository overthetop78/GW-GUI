namespace GWGUI.Emulation;

public interface IEmulationStorageSettingsManager
{
    EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration);
    IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration,
        EmulationStorageSettings settings);
}
