using System.IO;

namespace GWGUI.Emulation.Snk.Emulators.MednafenNgp.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static IReadOnlyList<FirmwareCategory> Roles => Array.Empty<FirmwareCategory>();
    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolute = Path.GetFullPath(path);
        return ValueTask.FromResult(new EmulationFirmwareCandidate(absolute, absolute, Path.GetFileName(absolute), null, EmulationFirmwareCompatibility.Incompatible));
    }
}
