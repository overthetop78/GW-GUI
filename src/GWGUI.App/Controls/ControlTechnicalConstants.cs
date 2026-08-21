namespace GWGUI.App.Controls;

internal static class ControlTechnicalConstants
{
    internal static readonly TimeSpan EmulationInputPollingInterval = TimeSpan.FromMilliseconds(16);
    internal static readonly TimeSpan MediaActivityPersistence = TimeSpan.FromMilliseconds(140);
    internal static readonly TimeSpan ControllerCapturePollingInterval = TimeSpan.FromMilliseconds(30);
    internal const double FrequencyComparisonTolerance = 0.01d;
}
