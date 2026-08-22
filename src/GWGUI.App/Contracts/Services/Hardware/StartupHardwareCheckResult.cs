using GWGUI.Domain.Settings.Hardware;
namespace GWGUI.App.Contracts.Services.Hardware;

public sealed record StartupHardwareCheckResult(
    bool Performed,
    IReadOnlyList<ControllerSettings> MissingControllers,
    IReadOnlyList<ControllerSettings> NewControllers);
