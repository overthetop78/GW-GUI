namespace GWGUI.Emulation.Atari;

internal static class AtariMachineConstants
{
    internal const double MinimumFramesPerSecond = 1;
    internal const double MaximumFramesPerSecond = 1000;
    internal const int PauseWaitMilliseconds = 100;
    internal const int DiagnosticTailCount = 100;
    internal const int EmptyCount = 0;
    internal const long NoRemainingTicks = 0;
    internal const string ThreadNamePrefix = "gwgui Atari";
    internal const string DiagnosticDataKey = "AtariDiagnostics";
    internal const string InvalidStartStateMessage = "The Atari machine can only be started once.";
    internal const string InvalidStateMessage = "The Atari machine must be running or paused.";
    internal const string StoppedMessage = "The Atari machine stopped.";
}
