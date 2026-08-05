using GWGUI.Domain.Settings;

namespace GWGUI.Domain.Hardware;

public interface IHardwareRegistry
{
    Task<IReadOnlyList<ControllerSettings>> ScanAsync(
        string executable,
        IReadOnlyList<ControllerSettings> configuredControllers,
        CancellationToken cancellationToken = default);
}
