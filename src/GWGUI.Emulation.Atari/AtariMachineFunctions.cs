using System.Diagnostics;

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

    internal static void SaveState(string path, byte[] state) => File.WriteAllBytes(path, state);

    internal static byte[] LoadState(string path) => File.ReadAllBytes(path);
}
