using System.IO;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Enums;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class FileSystemFunctions
{
    private static bool Enabled(MachineConfiguration configuration, string option) => configuration.Options?.GetValueOrDefault(option) == EmulationSettingConstants.Enabled;

    private static string? PathSelection(MachineConfiguration configuration, string pathKey) => configuration.Options?.GetValueOrDefault(pathKey) is { } path && !string.IsNullOrWhiteSpace(path) ? path : null;

    private static string? Selection(MachineConfiguration configuration, string option, string pathKey) => Enabled(configuration, option)
        && (option != OptionConstants.UseExternalGameindexKey || GameIndexAvailable(configuration)) ? PathSelection(configuration, pathKey) : null;

    private static bool GameIndexAvailable(MachineConfiguration configuration)
    {
        var renderer = configuration.Options?.GetValueOrDefault(OptionConstants.RendererKey) ?? OptionValueFunctions.ToNative(RendererBackend.Automatic);
        return Enabled(configuration, OptionConstants.EnableHwHacksKey)
            && new[] { RendererBackend.Automatic, RendererBackend.OpenGl, RendererBackend.Direct3D11, RendererBackend.Direct3D12, RendererBackend.Vulkan }
                .Any(value => OptionValueFunctions.ToNative(value) == renderer);
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        var gameIndex = Selection(configuration, OptionConstants.UseExternalGameindexKey, FileSystemConstants.GameIndexPathKey);
        if (gameIndex is not null && !File.Exists(gameIndex)) throw new FileNotFoundException(null, gameIndex);
        var cheats = Selection(configuration, OptionConstants.EnableCheatsKey, FileSystemConstants.CheatsDirectoryKey);
        if (cheats is not null && !Directory.Exists(cheats)) throw new DirectoryNotFoundException(cheats);
    }

    internal static string SystemDirectory(MachineConfiguration configuration, string physicalDirectory) => HardDiskRuntimeFunctions.Selected(configuration) is not null
        || Selection(configuration, OptionConstants.UseExternalGameindexKey, FileSystemConstants.GameIndexPathKey) is not null
        || PathSelection(configuration, FileSystemConstants.CheatsDirectoryKey) is not null ? FileSystemConstants.SystemUri : physicalDirectory;

    internal static (string Path, bool IsReadOnly) Resolve(MachineConfiguration configuration, string systemDirectory, string path, bool? cheatsEnabled = null)
    {
        var normalized = path.Replace('\\', '/');
        if (!normalized.Equals(FileSystemConstants.SystemUri, StringComparison.Ordinal)
            && !normalized.StartsWith(FileSystemConstants.SystemUri + "/", StringComparison.Ordinal)) return (path, false);
        var relative = normalized[FileSystemConstants.SystemUri.Length..].TrimStart('/');
        if (relative.Equals(HardDiskConstants.NativeFile, StringComparison.OrdinalIgnoreCase) && HardDiskRuntimeFunctions.Selected(configuration) is { } disk)
            return (Path.GetFullPath(disk.Path), disk.IsReadOnly);
        if (relative.Equals(FileSystemConstants.GameIndexFile, StringComparison.OrdinalIgnoreCase)
            && Selection(configuration, OptionConstants.UseExternalGameindexKey, FileSystemConstants.GameIndexPathKey) is { } database)
            return (Path.GetFullPath(database), true);
        if ((relative.Equals(FileSystemConstants.CheatsDirectory, StringComparison.OrdinalIgnoreCase)
            || relative.StartsWith(FileSystemConstants.CheatsDirectory + "/", StringComparison.OrdinalIgnoreCase))
            && (cheatsEnabled ?? Enabled(configuration, OptionConstants.EnableCheatsKey))
            && PathSelection(configuration, FileSystemConstants.CheatsDirectoryKey) is { } cheats)
            return (Within(cheats, relative[FileSystemConstants.CheatsDirectory.Length..].TrimStart('/')), true);
        return (Within(systemDirectory, relative), false);
    }

    private static string Within(string directory, string relative)
    {
        var root = Path.GetFullPath(directory);
        var resolved = Path.GetFullPath(Path.Combine(root, relative));
        if (!resolved.Equals(root, StringComparison.OrdinalIgnoreCase) && !resolved.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(relative);
        return resolved;
    }

    internal static IReadOnlyList<EmulationSettingsBlock> Describe(MachineConfiguration configuration, IReadOnlyList<EmulationSettingsBlock> blocks) => blocks
        .Select(block => block with { Fields = block.Fields.SelectMany(field => WithPath(configuration, field)).ToArray() }).ToArray();

    private static IEnumerable<EmulationSettingsField> WithPath(MachineConfiguration configuration, EmulationSettingsField field)
    {
        var database = field.Id == OptionConstants.UseExternalGameindexKey;
        var cheats = field.Id == OptionConstants.EnableCheatsKey;
        var available = !database || GameIndexAvailable(configuration);
        yield return database || cheats ? field with { RefreshSettingsOnChange = true, IsEnabled = field.IsEnabled && available }
            : field.Id is OptionConstants.EnableHwHacksKey or OptionConstants.RendererKey ? field with { RefreshSettingsOnChange = true } : field;
        if (!database && !cheats) yield break;
        var key = database ? FileSystemConstants.GameIndexPathKey : FileSystemConstants.CheatsDirectoryKey;
        yield return new EmulationSettingsField(key, field.Tab, field.BlockId,
            database ? FileSystemConstants.GameIndexLabel : FileSystemConstants.CheatsDirectoryLabel,
            database ? EmulationSettingsEditor.Path : EmulationSettingsEditor.OpenDirectory,
            configuration.Options?.GetValueOrDefault(key), IsEnabled: cheats || available && field.Value == field.EnabledValue, RequiresRestart: true,
            DefaultFolderCategory: cheats ? EmulationDefaultFolderCategory.Cheats : null);
    }
}
