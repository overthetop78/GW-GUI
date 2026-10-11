using System.IO;
using GWGUI.Emulation.Snk.Emulators.Race.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Race.Functions;

internal static class MediaRuntimeFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices => [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true)];
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var path = configuration.InitialContent is { Length: > 0 } initial ? initial : configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.Cartridge0 && media.IsInserted)?.Path;
        return path is null ? [] : [ContentFunctions.Validate(path, RuntimeOptionConstants.CartridgeExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }
    internal static LoadedContent? Prepare(MachineConfiguration configuration, IReadOnlySet<string> supportedExtensions)
    {
        var paths = InitialContentPaths(configuration);
        return paths.Count == 0 ? null : ContentFunctions.Create(paths[0], RuntimeOptionConstants.NeedsFullPath, supportedExtensions);
    }
}
