using GWGUI.App.Constants.Localization;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.Domain.Settings;
using System.IO;

namespace GWGUI.App.Services.DiskImages.Selection;

internal sealed class DiskImageFileSelectionService(
    Func<AppSettings> getSettings,
    IFileDialogService fileDialogs,
    Func<string, object[], string> localize)
{
    internal string? SelectVisualizerImage() => SelectImage(
        settings => settings.LastVisualizerImageFolder,
        (settings, folder) => settings.LastVisualizerImageFolder = folder);

    internal string? SelectExplorerImage() => SelectImage(
        settings => settings.LastExplorerImageFolder,
        (settings, folder) => settings.LastExplorerImageFolder = folder);

    private string? SelectImage(
        Func<AppSettings, string?> getLastFolder,
        Action<AppSettings, string?> setLastFolder)
    {
        var settings = getSettings();
        var lastFolder = getLastFolder(settings);
        var initialDirectory = !string.IsNullOrWhiteSpace(lastFolder) && Directory.Exists(lastFolder)
            ? lastFolder
            : settings.DefaultImagesFolder;
        var path = fileDialogs.OpenFile(new(localize(DiskImageResourceKeys.CommonDiskImageFilter, []), initialDirectory));
        if (path is not null) setLastFolder(settings, Path.GetDirectoryName(path));
        return path;
    }
}
