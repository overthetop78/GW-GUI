using System.Diagnostics;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Emulation.Atari;

internal static class AtariMachineFunctions
{
    internal static string ThreadName(Guid id, AtariCoreKind core) =>
        $"{AtariMachineConstants.ThreadNamePrefix} {core} {id:N}";

    internal static long NextFrameTimestamp(long current, double framesPerSecond) => current +
        (long)(Stopwatch.Frequency / Math.Clamp(framesPerSecond,
            AtariMachineConstants.MinimumFramesPerSecond, AtariMachineConstants.MaximumFramesPerSecond));

    internal static void WaitForFrame(long target, CancellationToken cancellationToken)
    {
        var remaining = target - Stopwatch.GetTimestamp();
        if (remaining > AtariMachineConstants.NoRemainingTicks)
            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds((double)remaining / Stopwatch.Frequency));
    }

    internal static void ReleaseInput(IAtariCore core) => core.SetInput(EmulationInputSnapshot.Empty);

    internal static void TryReleaseInput(IAtariCore core)
    {
        try { ReleaseInput(core); }
        catch (Exception) { }
    }

    internal static void DeleteSessionDirectory(string sessionDirectory)
    {
        try
        {
            var path = Path.GetFullPath(sessionDirectory);
            if (Directory.Exists(path) && !string.IsNullOrWhiteSpace(Path.GetFileName(path)))
                Directory.Delete(path, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

}
