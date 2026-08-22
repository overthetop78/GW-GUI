namespace GWGUI.App.Constants.Emulation;

internal static class EmulationRuntimeConstants
{
    internal static readonly TimeSpan EmulationInputPollingInterval = TimeSpan.FromMilliseconds(16);
    internal static readonly TimeSpan MediaActivityPersistence = TimeSpan.FromMilliseconds(140);
    internal const double FrequencyComparisonTolerance = 0.01d;
}
