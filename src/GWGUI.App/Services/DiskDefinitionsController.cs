using System.IO;
using System.Windows.Controls;
using GWGUI.App.Controls;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Services;

internal sealed class DiskDefinitionsController
{
    private readonly Func<AppSettings> _getSettings;
    private readonly ImageFormatWorkspace _workspace;
    private readonly IFileDialogService _fileDialogs;
    private readonly IMessageDialogService _dialogs;
    private readonly Action _workspaceChanged;
    private readonly Func<string, object[], string> _localize;

    public DiskDefinitionsController(
        ReadAdvancedSection read,
        WriteAdvancedSection write,
        ConversionAdvancedSection conversion,
        Func<AppSettings> getSettings,
        ImageFormatWorkspace workspace,
        IFileDialogService fileDialogs,
        IMessageDialogService dialogs,
        Action workspaceChanged,
        Action<string> setReadPath,
        Action<string> setWritePath,
        Action<string> setConversionPath,
        Action refreshRead,
        Action refreshWrite,
        Action refreshConversion,
        Func<string, object[], string> localize)
    {
        _getSettings = getSettings;
        _workspace = workspace;
        _fileDialogs = fileDialogs;
        _dialogs = dialogs;
        _workspaceChanged = workspaceChanged;
        _localize = localize;

        read.BrowseDiskDefinitionsRequested += (_, _) => Browse(read.DiskDefinitionsValue, setReadPath, refreshRead);
        write.BrowseDiskDefinitionsRequested += (_, _) => Browse(write.DiskDefinitionsValue, setWritePath, refreshWrite);
        conversion.BrowseDiskDefinitionsRequested += (_, _) => Browse(conversion.DiskDefinitionsValue, setConversionPath, refreshConversion);
    }

    public void LoadConfigured()
    {
        var settings = _getSettings();
        var paths = new[]
        {
            settings.Read.OptionValues.GetValueOrDefault("diskdefs"),
            settings.Write.OptionValues.GetValueOrDefault("diskdefs"),
            settings.Conversion.OptionValues.GetValueOrDefault("diskdefs")
        }.Concat(settings.Profiles.Select(profile => profile.Values.GetValueOrDefault("diskdefs")));

        foreach (var path in paths.Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try { Add(path!); }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            {
                // The corresponding operation reports the invalid definition when it is executed.
            }
        }
    }

    public bool Validate(CheckBox enabled, TextBox path, string title)
    {
        if (enabled.IsChecked != true || File.Exists(path.Text)) return true;
        _dialogs.Show(_localize("Advanced.DiskDefsMissing", []), title, icon: UserDialogIcon.Warning);
        return false;
    }

    public void ShowInvalid(string title) =>
        _dialogs.Show(_localize("Advanced.Invalid", [_localize("Common.Unknown", [])]), title, icon: UserDialogIcon.Warning);

    public void ShowInvalid(Exception _, string title) => ShowInvalid(title);

    private void Browse(TextBox target, Action<string> assign, Action refreshCommand)
    {
        var path = _fileDialogs.OpenFile(new OpenFileRequest(_localize("Advanced.DiskDefsFilter", []), FileName: target.Text));
        if (path is null) return;

        try { Add(path); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            ShowInvalid(_localize("Advanced.DiskDefs", []));
            return;
        }

        assign(path);
        refreshCommand();
    }

    private void Add(string path)
    {
        _workspace.AddDiskDefinitions(path);
        _workspaceChanged();
    }
}
