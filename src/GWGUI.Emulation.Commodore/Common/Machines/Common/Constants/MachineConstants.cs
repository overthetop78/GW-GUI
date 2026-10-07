namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Constants;


internal static class MachineConstants
{
    internal const string DiagnosticsDataKey = "CommodoreDiagnostics";
    internal const string ThreadNameFormat = "GWGUI Commodore {0:N}";
    internal const int ControllerPointerPulseMilliseconds = 100;
    internal const int PausedWorkerPollMilliseconds = 100;
    internal const int MaximumDiagnosticEntries = 100;
    internal const int MinimumFramesPerSecond = 1;
    internal const int MaximumFramesPerSecond = 1000;
    internal const double SecondsPerFrame = 1;
    internal const int UninitializedAudioSampleRate = 0;
    internal const long InitialVideoSequence = 0;

}
