using GWGUI.Domain.Settings;
using GWGUI.App.Dictionaries.Options;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Storage;
using GWGUI.App.ViewModels.Options;
using GWGUI.App.Views.Controls.Options;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace GWGUI.App.Options.Controllers;

internal sealed class LoggingOptionsController
{
    private readonly OptionsLogsSection _section;
    private readonly AppSettings _settings;
    private readonly Func<bool> _isInitializing;
    private readonly Func<Task> _persistAsync;
    private readonly Func<string, string> _localize;
    private readonly Action<Exception> _reportOpenFolderError;
    private readonly string _logsDirectory;

    public LoggingOptionsController(
        OptionsLogsSection section,
        AppSettings settings,
        Func<bool> isInitializing,
        Func<Task> persistAsync,
        Func<string, string> localize,
        Action<Exception> reportOpenFolderError,
        string? logsDirectory = null)
    {
        _section = section;
        _settings = settings;
        _isInitializing = isInitializing;
        _persistAsync = persistAsync;
        _localize = localize;
        _reportOpenFolderError = reportOpenFolderError;
        _logsDirectory = logsDirectory ?? StoragePaths.LogsDirectory;

        _section.LogRowChanged += RowChanged;
        _section.MaximumSizeEditingFinished += MaximumSizeEditingFinished;
        _section.NumericTextEntered += NumericTextEntered;
        _section.OpenLogsFolderRequested += OpenLogsFolder;
        _section.OptionsList.ItemsSource = Options;
        _section.DirectoryText.Text = _logsDirectory;
        RefreshLocalizedContent();
    }

    public ObservableCollection<LogOptionRow> Options { get; } = [];

    public void RefreshLocalizedContent()
    {
        _section.OptionsList.ItemsSource = null;
        Options.Clear();
        foreach (var definition in LogActionDefinitions.All)
            Options.Add(new(definition.Action, _localize(definition.LabelKey), _settings.Logging.GetOrCreate(definition.Action)));
        _section.OptionsList.ItemsSource = Options;
    }

    private async void RowChanged(object sender, RoutedEventArgs e)
    {
        if (_isInitializing()) return;
        await _persistAsync();
    }

    private async void MaximumSizeEditingFinished(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox textBox) await CommitMaximumSizeAsync(textBox);
    }

    internal async Task CommitMaximumSizeAsync(TextBox textBox)
    {
        if (textBox.DataContext is LogOptionRow row)
        {
            if (!int.TryParse(textBox.Text, out var value) || value < 0)
                textBox.SetCurrentValue(TextBox.TextProperty, row.Settings.MaximumKilobytes.ToString());
            else
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
        await _persistAsync();
    }

    private static void NumericTextEntered(object sender, TextCompositionEventArgs e) =>
        e.Handled = e.Text.Any(character => !char.IsDigit(character));

    private void OpenLogsFolder(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(_logsDirectory);
            Process.Start(new ProcessStartInfo(_logsDirectory) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            ErrorLog.Write(exception, "Opening Logs folder");
            _reportOpenFolderError(exception);
        }
    }
}
