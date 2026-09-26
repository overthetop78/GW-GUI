namespace GWGUI.Emulation.Atari.Common.Machines.Common.Constants;


internal static class MachineConstants
{
    internal const double MinimumFramesPerSecond = 1;
    internal const double MaximumFramesPerSecond = 1000;
    internal const int PauseWaitMilliseconds = 100;
    internal const int DiagnosticTailCount = 100;
    internal const int EmptyCount = 0;
    internal const long NoRemainingTicks = 0;
    internal const int InvalidSampleRate = 0;
    internal const string ThreadNamePrefix = "gwgui Atari";
    internal const string DiagnosticDataKey = "AtariDiagnostics";
}
