using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Read;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Profiles;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using Microsoft.Win32;

namespace GWGUI.App;

public partial class MainWindow : Window
{
    private readonly ISettingsStore _settingsStore;
    private readonly IGreaseweazleRunner _runner = new GreaseweazleRunner();
    private AppSettings _settings = new();
    private CancellationTokenSource? _cancellation;
    private readonly IImageFormatCatalog _formatCatalog = new BuiltInImageFormatCatalog();
    private IProfileStore _profiles = new InMemoryProfileStore();

    public MainWindow()
    {
        InitializeComponent();
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GW GUI");
        _settingsStore = new JsonSettingsStore(Path.Combine(directory, "settings.json"));
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = await _settingsStore.LoadAsync();
        _profiles = new InMemoryProfileStore(_settings.Profiles.Select(ToProfile));
        Width = Math.Max(MinWidth, _settings.Window.Width);
        Height = Math.Max(MinHeight, _settings.Window.Height);
        ReadFolder.Text = _settings.DefaultImagesFolder;
        ReadFamilyCombo.ItemsSource = _formatCatalog.Formats.Where(x => x.Family != "Raw").Select(x => x.Family).Distinct().Order().ToArray();
        ReadFamilyCombo.SelectedIndex = 0;
        RefreshReadProfiles();
        RestoreReadSettings();
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
        CaptureReadSettings();
        CaptureProfiles();
        await _settingsStore.SaveAsync(_settings);
    }

    private void RefreshReadProfiles(string? selectedId = null)
    {
        var items = _profiles.Get(OperationKind.Read);
        ReadProfileCombo.ItemsSource = items;
        ReadProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

    private void ReadProfile_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ReadProfileCombo.SelectedItem is not OperationProfile profile || ReadRevsEnabled is null) return;
        ApplyReadProfile(profile);
    }

    private void ResetReadProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ReadProfileCombo.SelectedItem is OperationProfile profile) ApplyReadProfile(profile);
    }

    private void ApplyReadProfile(OperationProfile profile)
    {
        ReadRevsEnabled.IsChecked = profile.EnabledOptions.Contains("revs");
        ReadRetriesEnabled.IsChecked = profile.EnabledOptions.Contains("retries");
        ReadTracksEnabled.IsChecked = profile.EnabledOptions.Contains("tracks");
        if (profile.Values.TryGetValue("revs", out var revs)) ReadRevsValue.Text = revs;
        if (profile.Values.TryGetValue("retries", out var retries)) ReadRetriesValue.Text = retries;
        if (profile.Values.TryGetValue("tracks", out var tracks)) ReadTracksValue.Text = tracks;
        if (profile.IsSystem)
        {
            RawScpRadio.IsChecked = true;
            ReadExpertArguments.Clear();
        }
        UpdateReadCommand();
    }

    private void SaveReadProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProfileNameWindow { Owner = this };
        if (dialog.ShowDialog() != true) return;
        var enabled = new HashSet<string>();
        if (ReadRevsEnabled.IsChecked == true) enabled.Add("revs");
        if (ReadRetriesEnabled.IsChecked == true) enabled.Add("retries");
        if (ReadTracksEnabled.IsChecked == true) enabled.Add("tracks");
        var values = new Dictionary<string, string> { ["revs"] = ReadRevsValue.Text, ["retries"] = ReadRetriesValue.Text, ["tracks"] = ReadTracksValue.Text };
        var profile = new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Read, dialog.ProfileName, values, enabled);
        try { profile = _profiles.Save(profile); }
        catch (InvalidOperationException)
        {
            if (MessageBox.Show("Ce profil existe déjà. Voulez-vous le remplacer ?", "Profil", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            profile = _profiles.Save(profile, true);
        }
        RefreshReadProfiles(profile.Id);
    }

    private void CaptureProfiles()
    {
        _settings.Profiles = Enum.GetValues<OperationKind>().SelectMany(_profiles.Get).Where(x => !x.IsSystem)
            .Select(x => new ProfileSettings { Id = x.Id, Operation = x.Operation.ToString(), Name = x.Name, Values = x.Values.ToDictionary(), EnabledOptions = x.EnabledOptions.ToHashSet() }).ToList();
    }

    private static OperationProfile ToProfile(ProfileSettings value) => new(value.Id, Enum.TryParse<OperationKind>(value.Operation, out var operation) ? operation : OperationKind.Read, value.Name, value.Values, value.EnabledOptions);

    private void ToggleConsole_Click(object sender, RoutedEventArgs e) => SetConsoleVisibility(ConsolePanel.Visibility != Visibility.Visible);

    private void SetConsoleVisibility(bool visible)
    {
        ConsolePanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        ConsoleRow.Height = visible ? new GridLength(Math.Max(100, _settings.ConsoleHeight)) : new GridLength(0);
    }

    private void ReadInput_Changed(object sender, RoutedEventArgs e) => UpdateReadCommand();

    private void ReadMode_Changed(object sender, RoutedEventArgs e)
    {
        if (KnownFormatPanel is null) return;
        KnownFormatPanel.Visibility = KnownFormatRadio.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        UpdateReadExtension();
        UpdateReadCommand();
    }

    private void ReadFamily_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ReadFormatCombo is null || ReadFamilyCombo.SelectedItem is not string family) return;
        ReadFormatCombo.ItemsSource = _formatCatalog.Formats.Where(x => x.Family == family).ToArray();
        ReadFormatCombo.SelectedIndex = 0;
    }

    private void ReadFormat_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ReadExtensionCombo is null) return;
        ReadExtensionCombo.ItemsSource = (ReadFormatCombo.SelectedItem as DiskFormat)?.Extensions;
        var extensions = ReadExtensionCombo.ItemsSource as IReadOnlyList<ImageExtension>;
        ReadExtensionCombo.SelectedIndex = extensions is null ? -1 : Math.Max(0, extensions.ToList().FindIndex(x => x.IsDefault));
        UpdateReadExtension();
        UpdateReadCommand();
    }

    private void UpdateReadExtension()
    {
        if (ReadExtensionText is null) return;
        ReadExtensionText.Text = RawScpRadio?.IsChecked == true ? "Image brute (SCP)" : (ReadExtensionCombo?.SelectedItem as ImageExtension)?.DisplayName ?? "Choisir un type d’image";
    }

    private void UpdateReadCommand()
    {
        if (CommandPreview is null || ReadFileName is null || ReadFolder is null) return;
        var extension = GetReadExtension();
        var target = GetReadTarget(extension);
        if (ReadNamePreview is not null) ReadNamePreview.Text = Path.GetFileName(target);
        try { CommandPreview.Text = BuildReadCommand(target).ToDisplayString(); }
        catch (ArgumentException exception) { CommandPreview.Text = $"⚠ {exception.Message}"; }
    }

    private string GetReadTarget(string extension)
    {
        var name = string.IsNullOrWhiteSpace(ReadFileName?.Text) ? "Exemple" : ReadFileName.Text.Trim();
        if (ReadAutoNumber?.IsChecked == true && long.TryParse(ReadSequenceValue.Text, out var sequence) && sequence >= 0)
        {
            var kind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
            var suffix = SequenceFormatter.Format(sequence, kind, ReadSequenceWidth.SelectedIndex + 1);
            name += " " + suffix;
        }
        return Path.Combine(ReadFolder.Text, name + extension);
    }

    private string GetReadExtension() => RawScpRadio?.IsChecked == true ? ".scp" : (ReadExtensionCombo?.SelectedItem as ImageExtension)?.Extension ?? "";

    private GwCommand BuildReadCommand(string target)
    {
        var options = new List<EnabledOption>();
        if (ReadRevsEnabled?.IsChecked == true) options.Add(new("--revs", ReadRevsValue.Text.Trim()));
        if (ReadRetriesEnabled?.IsChecked == true) options.Add(new("--retries", ReadRetriesValue.Text.Trim()));
        if (ReadTracksEnabled?.IsChecked == true) options.Add(new("--tracks", ReadTracksValue.Text.Trim()));
        return ReadCommandBuilder.Build(new ReadRequest(
            _settings.GwExecutablePath ?? "gw.exe", target,
            RawScpRadio?.IsChecked == true ? ReadResultKind.RawScp : ReadResultKind.KnownFormat,
            (ReadFormatCombo?.SelectedItem as DiskFormat)?.Id, options,
            ExpertArguments: ReadExpertArguments?.Text));
    }

    private void CopyReadName_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ReadFileName.Text)) Clipboard.SetText(ReadFileName.Text);
    }

    private void BrowseReadFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { InitialDirectory = ReadFolder.Text, Title = "Dossier de destination" };
        if (dialog.ShowDialog(this) == true) { ReadFolder.Text = dialog.FolderName; UpdateReadCommand(); }
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

        var extension = GetReadExtension();
        if (string.IsNullOrWhiteSpace(extension)) { MessageBox.Show("Choisissez un type d’image compatible.", "Lecture", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        var target = GetReadTarget(extension);
        if (File.Exists(target))
        {
            var answer = MessageBox.Show("Ce fichier existe déjà.\n\nOui : écraser\nNon : prendre le numéro suivant\nAnnuler : modifier le nom", "Fichier existant", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (answer == MessageBoxResult.Cancel) { ReadFileName.Focus(); ReadFileName.SelectAll(); return; }
            if (answer == MessageBoxResult.No)
            {
                if (ReadAutoNumber.IsChecked != true) ReadAutoNumber.IsChecked = true;
                if (!long.TryParse(ReadSequenceValue.Text, out var next)) next = 1;
                var kind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
                var available = OutputConflictResolver.FindNextAvailableWithValue(ReadFolder.Text, ReadFileName.Text.Trim(), extension, kind, ReadSequenceWidth.SelectedIndex + 1, next);
                target = available.Path;
                ReadSequenceValue.Text = available.Value.ToString();
            }
        }
        var command = BuildReadCommand(target);
        _cancellation = new CancellationTokenSource();
        ReadExecuteButton.Content = "Arrêter";
        LogOutput.Clear();
        var output = new Progress<GwOutputLine>(line => { LogOutput.AppendText(line.Text + Environment.NewLine); LogOutput.ScrollToEnd(); });
        try
        {
            var result = await _runner.RunAsync(command, output, _cancellation.Token);
            LogOutput.AppendText($"{Environment.NewLine}Fin : code {result.ExitCode}, durée {result.Duration:g}.");
            if (result.IsSuccess && ReadAutoNumber.IsChecked == true && long.TryParse(ReadSequenceValue.Text, out var value)) ReadSequenceValue.Text = (value + 1).ToString();
        }
        catch (Exception exception) { LogOutput.AppendText($"Erreur : {exception.Message}"); }
        finally { ReadExecuteButton.Content = "Exécuter"; _cancellation.Dispose(); _cancellation = null; }
    }

    private void RestoreReadSettings()
    {
        KnownFormatRadio.IsChecked = _settings.Read.UseKnownFormat;
        RawScpRadio.IsChecked = !_settings.Read.UseKnownFormat;
        ReadAutoNumber.IsChecked = _settings.Read.AutoNumber;
        ReadSequenceKind.SelectedIndex = _settings.Read.SequenceKind == "Alphabetic" ? 1 : 0;
        ReadSequenceWidth.SelectedIndex = Math.Clamp(_settings.Read.SequenceWidth - 1, 0, 2);
        ReadSequenceValue.Text = _settings.Read.NextSequence.ToString();
        ReadRevsEnabled.IsChecked = _settings.Read.EnabledOptions.Contains("revs");
        ReadRetriesEnabled.IsChecked = _settings.Read.EnabledOptions.Contains("retries");
        ReadTracksEnabled.IsChecked = _settings.Read.EnabledOptions.Contains("tracks");
        if (_settings.Read.OptionValues.TryGetValue("revs", out var revs)) ReadRevsValue.Text = revs;
        if (_settings.Read.OptionValues.TryGetValue("retries", out var retries)) ReadRetriesValue.Text = retries;
        if (_settings.Read.OptionValues.TryGetValue("tracks", out var tracks)) ReadTracksValue.Text = tracks;
    }

    private void CaptureReadSettings()
    {
        _settings.Read.UseKnownFormat = KnownFormatRadio.IsChecked == true;
        _settings.Read.FormatId = (ReadFormatCombo.SelectedItem as DiskFormat)?.Id;
        _settings.Read.AutoNumber = ReadAutoNumber.IsChecked == true;
        _settings.Read.SequenceKind = ReadSequenceKind.SelectedIndex == 1 ? "Alphabetic" : "Numeric";
        _settings.Read.SequenceWidth = ReadSequenceWidth.SelectedIndex + 1;
        if (long.TryParse(ReadSequenceValue.Text, out var sequence)) _settings.Read.NextSequence = sequence;
        _settings.Read.EnabledOptions = [];
        if (ReadRevsEnabled.IsChecked == true) _settings.Read.EnabledOptions.Add("revs");
        if (ReadRetriesEnabled.IsChecked == true) _settings.Read.EnabledOptions.Add("retries");
        if (ReadTracksEnabled.IsChecked == true) _settings.Read.EnabledOptions.Add("tracks");
        _settings.Read.OptionValues["revs"] = ReadRevsValue.Text;
        _settings.Read.OptionValues["retries"] = ReadRetriesValue.Text;
        _settings.Read.OptionValues["tracks"] = ReadTracksValue.Text;
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
