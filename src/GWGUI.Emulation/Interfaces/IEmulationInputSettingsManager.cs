namespace GWGUI.Emulation.Interfaces;

public interface IEmulationInputSettingsManager
{
    EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration);

    IEmulationConfiguration ApplyInputSettings(
        IEmulationConfiguration configuration,
        EmulationInputSettings settings);

    ValueTask SaveInputSettingsAsync(
        IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default);
}
