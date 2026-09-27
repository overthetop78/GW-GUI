using GWGUI.App.Constants.Localization;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.Infrastructure.Settings;
using System.IO;

namespace GWGUI.App.Services.DiskImages.Selection;

internal sealed class DiskImageFileSelectionService(
    Func<AppSettings> getSettings,
    IFileDialogService fileDialogs,
    Func<string, object[], string> localize,
    IReadOnlySet<string>? supportedExtensions = null)
{
    internal string? SelectVisualizerImage() => SelectImage(
        settings => settings.LastVisualizerImageFolder,
        (settings, folder) => settings.LastVisualizerImageFolder = folder);

    internal string? SelectExplorerImage() => SelectImage(
        settings => settings.LastExplorerImageFolder,
        (settings, folder) => settings.LastExplorerImageFolder = folder);

    internal string? SelectConversionImage() => SelectImage(
        settings => settings.LastDiskImageFolder,
        (settings, folder) => settings.LastDiskImageFolder = folder);

    private string? SelectImage(
        Func<AppSettings, string?> getLastFolder,
        Action<AppSettings, string?> setLastFolder)
    {
        var settings = getSettings();
        var lastFolder = getLastFolder(settings);
        var initialDirectory = !string.IsNullOrWhiteSpace(lastFolder) && Directory.Exists(lastFolder)
            ? lastFolder
            : settings.DefaultImagesFolder;
        var path = fileDialogs.OpenFile(new(BuildFilter(
            localize(DiskImageResourceKeys.CommonDiskImageFilter, []),
            supportedExtensions), initialDirectory));
        if (path is not null) setLastFolder(settings, Path.GetDirectoryName(path));
        return path;
    }

    internal static string BuildFilter(string localizedFilter, IReadOnlySet<string>? extensions)
    {
        if (extensions is null || extensions.Count == 0) return localizedFilter;
        var parts = localizedFilter.Split('|');
        if (parts.Length < 2) return localizedFilter;
        parts[1] = string.Join(';', extensions
            .Where(extension => !string.IsNullOrWhiteSpace(extension))
            .Select(extension => extension.StartsWith('.') ? extension : $".{extension}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select(extension => $"*{extension}"));
        return string.Join('|', parts);
    }
}
