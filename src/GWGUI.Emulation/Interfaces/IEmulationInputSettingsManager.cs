namespace GWGUI.Emulation;

public interface IEmulationInputSettingsManager
{
    EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration);

    IEmulationConfiguration ApplyInputSettings(
        IEmulationConfiguration configuration,
        EmulationInputSettings settings);
}
