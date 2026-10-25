using System.IO;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Constants;

namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Functions;

internal static class ContentFunctions
{
    internal static string Prepare(MachineConfiguration configuration, string path, string contentDirectory)
    {
        if (configuration.Model != ModelConstants.Sc3000
            || !Path.GetExtension(path).Equals(ContentConstants.Sc3000SourceExtension, StringComparison.OrdinalIgnoreCase)) return path;
        var destination = Path.Combine(contentDirectory, Path.GetFileNameWithoutExtension(path) + ContentConstants.Sc3000CoreExtension);
        File.Copy(path, destination, true);
        return destination;
    }
}
