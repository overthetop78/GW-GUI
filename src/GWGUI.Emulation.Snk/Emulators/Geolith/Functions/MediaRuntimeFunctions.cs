using System.IO;

namespace GWGUI.Emulation.Snk.Emulators.Geolith.Functions;

internal static class MediaRuntimeFunctions
{
    private const string CartridgeExtension = ".neo";
    private const string CueExtension = ".cue";
    private const string ChdExtension = ".chd";
    private const string MemoryCardExtension = ".mcr";
    private const int MemoryCardBytes = 2048;
    private const bool NeedsFullPath = true;
    internal sealed record PreparedMemoryCard(string SourcePath, string RuntimePath, bool IsReadOnly);

    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineConfiguration configuration) => ConfigurationFunctions.Family(configuration.Model) == MachineFamily.NeoGeoCd ? [new(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc, [CueExtension, ChdExtension], RequiresMachineRecreation: true)] : [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, [CartridgeExtension], RequiresMachineRecreation: true), new(EmulationMediaSlot.MemoryCard0, EmulationMediaType.MemoryCard, [MemoryCardExtension], RequiresMachineRecreation: true)];

    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var path = InitialContentPath(configuration);
        if (path is null) return [];
        var device = Devices(configuration).First(item => item.MediaType != EmulationMediaType.MemoryCard);
        return [ContentFunctions.Validate(path, device.AcceptedExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }

    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        var paths = InitialContentPaths(configuration);
        return paths.Count == 0 ? null : ContentFunctions.Create(paths[0], NeedsFullPath, supportedExtensions);
    }

    private static string? InitialContentPath(MachineConfiguration configuration)
    {
        if (configuration.InitialContent is { Length: > 0 } path) return path;
        var slot = ConfigurationFunctions.Family(configuration.Model) == MachineFamily.NeoGeoCd ? EmulationMediaSlot.Cd0 : EmulationMediaSlot.Cartridge0;
        return configuration.Media.SingleOrDefault(item => item.Slot == slot && item.IsInserted)?.Path;
    }

    internal static PreparedMemoryCard? PrepareMemoryCard(MachineConfiguration configuration, string saveDirectory)
    {
        var card = configuration.Media.SingleOrDefault(item => item.Slot == EmulationMediaSlot.MemoryCard0 && item.IsInserted);
        if (card is null) return null;
        var source = ContentFunctions.Validate(card.Path, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { MemoryCardExtension.TrimStart(MediaConstants.ExtensionPrefix) });
        if (new FileInfo(source).Length != MemoryCardBytes) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var contentPath = InitialContentPaths(configuration).SingleOrDefault();
        if (contentPath is null) throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentRequired, ErrorMessages.ContentRequired);
        Directory.CreateDirectory(saveDirectory);
        var target = Path.Combine(saveDirectory, Path.GetFileNameWithoutExtension(contentPath) + MemoryCardExtension);
        if (!string.Equals(source, Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(source, target, overwrite: true);
            File.SetAttributes(target, FileAttributes.Normal);
        }
        return new(source, target, card.IsReadOnly);
    }

    internal static void CompleteMemoryCard(PreparedMemoryCard? card)
    {
        if (card is null || card.IsReadOnly || !File.Exists(card.RuntimePath)) return;
        if (new FileInfo(card.RuntimePath).Length != MemoryCardBytes) throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
        if (!string.Equals(card.SourcePath, Path.GetFullPath(card.RuntimePath), StringComparison.OrdinalIgnoreCase)) File.Copy(card.RuntimePath, card.SourcePath, overwrite: true);
    }
}
