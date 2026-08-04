using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Settings;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;

namespace GWGUI.App;

public partial class MainWindow : Window
{
    private readonly ISettingsStore _settingsStore;
    private readonly IGreaseweazleRunner _runner = new GreaseweazleRunner();
    private AppSettings _settings = new();
    private CancellationTokenSource? _cancellation;

    public MainWindow()
    {
        InitializeComponent();
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GW GUI");
        _settingsStore = new JsonSettingsStore(Path.Combine(directory, "settings.json"));
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = await _settingsStore.LoadAsync();
        Width = Math.Max(MinWidth, _settings.Window.Width);
        Height = Math.Max(MinHeight, _settings.Window.Height);
        ReadFolder.Text = _settings.DefaultImagesFolder;
        SetConsoleVisibility(_settings.ConsoleExpanded);
        UpdateReadCommand();
    }

    private async void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_runner.IsRunning)
        {
            var answer = MessageBox.Show("Une opération est en cours. Voulez-vous l’arrêter et fermer GW GUI ?", "GW GUI", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) { e.Cancel = true; return; }
            _cancellation?.Cancel();
        }

        _settings.Window.Width = RestoreBounds.Width;
        _settings.Window.Height = RestoreBounds.Height;
        _settings.Window.Left = RestoreBounds.Left;
        _settings.Window.Top = RestoreBounds.Top;
        _settings.Window.Maximized = WindowState == WindowState.Maximized;
        _settings.ConsoleExpanded = ConsolePanel.Visibility == Visibility.Visible;
        if (_settings.ConsoleExpanded) _settings.ConsoleHeight = ConsoleRow.ActualHeight;
        await _settingsStore.SaveAsync(_settings);
    }

    private void ToggleConsole_Click(object sender, RoutedEventArgs e) => SetConsoleVisibility(ConsolePanel.Visibility != Visibility.Visible);

    private void SetConsoleVisibility(bool visible)
    {
        ConsolePanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        ConsoleRow.Height = visible ? new GridLength(Math.Max(100, _settings.ConsoleHeight)) : new GridLength(0);
    }

    private void ReadInput_Changed(object sender, RoutedEventArgs e) => UpdateReadCommand();

    private void UpdateReadCommand()
    {
        if (CommandPreview is null || ReadFileName is null || ReadFolder is null) return;
        var extension = RawScpRadio?.IsChecked == true ? ".scp" : ".img";
        var target = Path.Combine(ReadFolder.Text, ReadFileName.Text + extension);
        CommandPreview.Text = new GwCommand(_settings.GwExecutablePath ?? "gw.exe", "read", [target]).ToDisplayString();
    }

    private async void ExecuteRead_Click(object sender, RoutedEventArgs e)
    {
        if (_runner.IsRunning) { _cancellation?.Cancel(); return; }
        if (string.IsNullOrWhiteSpace(ReadFileName.Text))
        {
            MessageBox.Show("Indiquez un nom de fichier.", "Lecture", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            MessageBox.Show("Greaseweazle Tools n’est pas configuré. Ouvrez Options → Préférences.", "GW GUI", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var extension = RawScpRadio.IsChecked == true ? ".scp" : ".img";
        var command = new GwCommand(_settings.GwExecutablePath, "read", [Path.Combine(ReadFolder.Text, ReadFileName.Text + extension)]);
        _cancellation = new CancellationTokenSource();
        LogOutput.Clear();
        var output = new Progress<GwOutputLine>(line => { LogOutput.AppendText(line.Text + Environment.NewLine); LogOutput.ScrollToEnd(); });
        try
        {
            var result = await _runner.RunAsync(command, output, _cancellation.Token);
            LogOutput.AppendText($"{Environment.NewLine}Fin : code {result.ExitCode}, durée {result.Duration:g}.");
        }
        catch (Exception exception) { LogOutput.AppendText($"Erreur : {exception.Message}"); }
        finally { _cancellation.Dispose(); _cancellation = null; }
    }

    private async void Preferences_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OptionsWindow(_settings) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            ReadFolder.Text = _settings.DefaultImagesFolder;
            await _settingsStore.SaveAsync(_settings);
            UpdateReadCommand();
        }
    }

    private void ToolCommand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string verb }) return;
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            MessageBox.Show("Greaseweazle Tools n’est pas configuré. Ouvrez Options → Préférences.", "GW GUI", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        new GwToolWindow(_settings.GwExecutablePath, verb) { Owner = this }.ShowDialog();
    }
}
