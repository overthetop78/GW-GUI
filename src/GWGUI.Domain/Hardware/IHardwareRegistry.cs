using GWGUI.Domain.Settings.Hardware;
namespace GWGUI.Domain.Hardware;

public sealed record HardwareScanResult(
    IReadOnlyList<ControllerSettings> ConfiguredControllers,
    IReadOnlyList<ControllerSettings> UnconfiguredControllers);

public interface IHardwareRegistry
{
    Task<HardwareScanResult> ScanAsync(
        string executable,
        IReadOnlyList<ControllerSettings> configuredControllers,
        CancellationToken cancellationToken = default);
}
