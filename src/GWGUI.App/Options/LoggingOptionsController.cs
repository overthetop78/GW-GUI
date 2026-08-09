using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GWGUI.App.Controls;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Options;

internal sealed class LoggingOptionsController
{
    private readonly OptionsLogsSection _section;
    private readonly AppSettings _settings;
    private readonly Func<bool> _isInitializing;
    private readonly Func<Task> _persistAsync;
    private readonly Func<string, string> _localize;
    private readonly Action<Exception> _reportOpenFolderError;

    public LoggingOptionsController(
        OptionsLogsSection section,
        AppSettings settings,
        Func<bool> isInitializing,
        Func<Task> persistAsync,
        Func<string, string> localize,
        Action<Exception> reportOpenFolderError)
    {
        _section = section;
        _settings = settings;
        _isInitializing = isInitializing;
        _persistAsync = persistAsync;
        _localize = localize;
        _reportOpenFolderError = reportOpenFolderError;

        _section.LogRowChanged += RowChanged;
        _section.MaximumSizeEditingFinished += MaximumSizeEditingFinished;
        _section.NumericTextEntered += NumericTextEntered;
        _section.OpenLogsFolderRequested += OpenLogsFolder;
        _section.OptionsList.ItemsSource = Options;
        _section.DirectoryText.Text = StoragePaths.LogsDirectory;
        RefreshLocalizedContent();
    }

    public ObservableCollection<LogOptionRow> Options { get; } = [];

    public void RefreshLocalizedContent()
    {
        Options.Clear();
        foreach (var definition in OptionsDefinitions.LogActions)
            Options.Add(new(definition.Action, _localize(definition.LabelKey), _settings.Logging.GetOrCreate(definition.Action)));
    }

    private async void RowChanged(object sender, RoutedEventArgs e)
    {
        if (_isInitializing()) return;
        await _persistAsync();
    }

    private async void MaximumSizeEditingFinished(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is LogOptionRow row &&
            (!int.TryParse(textBox.Text, out var value) || value < 0))
            textBox.Text = row.Settings.MaximumKilobytes.ToString();
        await _persistAsync();
    }

    private static void NumericTextEntered(object sender, TextCompositionEventArgs e) =>
        e.Handled = e.Text.Any(character => !char.IsDigit(character));

    private void OpenLogsFolder(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(StoragePaths.LogsDirectory);
            Process.Start(new ProcessStartInfo(StoragePaths.LogsDirectory) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            ErrorLog.Write(exception, "Opening Logs folder");
            _reportOpenFolderError(exception);
        }
    }
}
