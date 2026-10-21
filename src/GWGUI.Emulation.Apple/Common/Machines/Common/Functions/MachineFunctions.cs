using GWGUI.Emulation;
using System.IO;
using System.Diagnostics;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class MachineFunctions
{
    internal static string ThreadName(Guid id, Emulator core) => $"{MachineConstants.ThreadNamePrefix} {core} {id:N}";

    internal static long NextFrameTimestamp(long current, double framesPerSecond) => current + (long)(Stopwatch.Frequency / Math.Clamp(framesPerSecond, MachineConstants.MinimumFramesPerSecond, MachineConstants.MaximumFramesPerSecond));

    internal static void ReleaseInput(IEmulatorCore core) => core.SetInput(EmulationInputSnapshot.Empty);

    internal static void TryReleaseInput(IEmulatorCore core)
    {
        try { ReleaseInput(core); }
        catch (Exception) { }
    }

    internal static void DeleteSessionDirectory(string sessionDirectory)
        => EmulationSdk.SessionDirectories.DeleteGeneratedSessionDirectory(sessionDirectory);

}
