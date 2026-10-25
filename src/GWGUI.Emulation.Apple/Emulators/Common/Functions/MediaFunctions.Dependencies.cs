using GWGUI.Emulation.Apple.Emulators.Common.Constants;
using System.IO;

namespace GWGUI.Emulation.Apple.Emulators.Common.Functions;

internal static class MediaDependencyFunctions
{
    internal static IReadOnlyList<string> GetPaths(string path) => string.Equals(Path.GetExtension(path), SessionMediaConstants.PlaylistExtension, StringComparison.OrdinalIgnoreCase) ? [path, .. SessionMediaFunctions.ReadSourcePaths(path)] : [];
}
