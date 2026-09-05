using System.IO;
using GWGUI.App.Services.Storage;
using GWGUI.VideoPresentation.Constants;
using GWGUI.VideoPresentation.Services;

namespace GWGUI.App.Services.Emulation;

internal static class EmulationVideoPresentationProfiles
{
    internal static VideoPresentationProfileStore Store { get; } = new(
        Path.Combine(StoragePaths.EmulationDirectory, VideoPresentationStorageConstants.DirectoryName),
        LegacyPaths);

    private static IEnumerable<string> LegacyPaths(string module, Guid id)
    {
        var directory = module.ToLowerInvariant() switch
        {
            "amiga" => "Amiga",
            "atari" => "Atari",
            _ => null
        };
        if (directory is null) return [];
        var root = Path.Combine(StoragePaths.EmulationDirectory, "Machines", directory, "Configurations");
        var name = id.ToString(VideoPresentationStorageConstants.IdentifierFormat);
        return [Path.Combine(root, name, "machine.json"), Path.Combine(root, name + ".json")];
    }
}
