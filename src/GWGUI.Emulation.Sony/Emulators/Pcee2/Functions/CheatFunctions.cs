using System.IO;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;

internal static class CheatFunctions
{
    private static bool Enabled(MachineConfiguration configuration) => configuration.Options?.GetValueOrDefault(OptionConstants.CheatsKey) == ToggleValueConstants.Enabled;

    private static string? SelectedDirectory(MachineConfiguration configuration) => configuration.Options?.GetValueOrDefault(CheatConstants.DirectoryKey) is { } path && !string.IsNullOrWhiteSpace(path) ? path : null;

    internal static void Validate(MachineConfiguration configuration)
    {
        if (Enabled(configuration) && SelectedDirectory(configuration) is { } directory && !Directory.Exists(directory))
            throw new DirectoryNotFoundException(directory);
    }

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        if (!Enabled(configuration) || SelectedDirectory(configuration) is not { } sourceDirectory) return;
        Validate(configuration);
        var destination = Path.Combine(systemDirectory, CheatConstants.NativeDirectory);
        Directory.CreateDirectory(destination);
        foreach (var path in Directory.EnumerateFiles(sourceDirectory))
        {
            if (!Path.GetExtension(path).Equals(CheatConstants.FileExtension, StringComparison.OrdinalIgnoreCase)) continue;
            using var source = File.OpenRead(path);
            using var target = File.Create(Path.Combine(destination, Path.GetFileName(path)));
            source.CopyTo(target);
        }
    }

    internal static IReadOnlyList<EmulationSettingsBlock> Describe(MachineConfiguration configuration, IReadOnlyList<EmulationSettingsBlock> blocks) => blocks
        .Select(block => block with { Fields = block.Fields.SelectMany(field => WithDirectory(configuration, field)).ToArray() }).ToArray();

    private static IEnumerable<EmulationSettingsField> WithDirectory(MachineConfiguration configuration, EmulationSettingsField field)
    {
        if (field.Id != OptionConstants.CheatsKey)
        {
            yield return field;
            yield break;
        }
        yield return field with { RefreshSettingsOnChange = true };
        yield return new(CheatConstants.DirectoryKey, field.Tab, field.BlockId, CheatConstants.DirectoryLabelResource,
            EmulationSettingsEditor.OpenDirectory, SelectedDirectory(configuration),
            ExplanationResourceKey: OptionConstants.CheatsHelpResource, RequiresRestart: true,
            DefaultFolderCategory: EmulationDefaultFolderCategory.Cheats);
    }
}
