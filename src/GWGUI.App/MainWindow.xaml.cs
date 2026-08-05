using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Read;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Profiles;
using GWGUI.Domain.Write;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Maintenance;
using GWGUI.Domain.Hardware;
using GWGUI.Scp;
using GWGUI.Scp.Decoding;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using Microsoft.Win32;
using GWGUI.App.Localization;
using GWGUI.Infrastructure.HostTools;
using GWGUI.App.ViewModels;

namespace GWGUI.App;

public partial class MainWindow : Window
{
    private readonly ISettingsStore _settingsStore;
    private readonly IGreaseweazleRunner _runner;
    private AppSettings _settings = new();
    private CancellationTokenSource? _cancellation;
    private IImageFormatCatalog _formatCatalog = null!;
    private IProfileStore _profiles = new InMemoryProfileStore();
    private ImageFormatDetector _formatDetector;
    private DetectedImageFormat? _detectedWriteFormat;
    private readonly List<ConversionFormatControl> _conversionControls = [];
    private ScpImage? _scpImage;
    private bool _syncingScpZoom;
    private readonly FluxDecoderRegistry _fluxDecoders = new();
    private readonly ScpInspectorPresenter _scpInspector;
    private string? _lastScpPath;
    private ScpTrack? _selectedScpTrack;
    private readonly GwProgressTracker _progressTracker = new();
    private readonly string _logsDirectory;
    private readonly MainWindowViewModel _viewModel;
    private GwFormatCapabilities _gwCapabilities = GwFormatCapabilities.Unknown;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel(LocExtension.Get("Hardware.NotConfigured"), LocExtension.Get("Status.ReadyShort"));
        DataContext = _viewModel;
        _formatCatalog = new BuiltInImageFormatCatalog(key => LocExtension.Get(key));
        _scpInspector = new ScpInspectorPresenter(_fluxDecoders, (key, arguments) => LocExtension.Get(key, arguments));
        ScpSide0.TrackSelected += ScpTrack_Selected; ScpSide1.TrackSelected += ScpTrack_Selected;
        ScpSide0.ZoomChanged += ScpZoom_Changed; ScpSide1.ZoomChanged += ScpZoom_Changed;
        _formatDetector = new ImageFormatDetector(_formatCatalog);
        var directory = StoragePaths.DataDirectory;
        _settingsStore = new JsonSettingsStore(Path.Combine(directory, "settings.json"));
        _logsDirectory = Path.Combine(directory, "logs");
        _runner = new GreaseweazleRunner(new RotatingOperationLogWriter(_logsDirectory));
    }

    private async void OpenScp_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get("Visual.OpenFilter"), InitialDirectory = ReadFolder.Text };
        if (dialog.ShowDialog(this) != true) return;
        await LoadScpAsync(dialog.FileName);
    }

    private async void OpenLastScp_Click(object sender, RoutedEventArgs e)
    {
        if (_lastScpPath is null) return; MainTabs.SelectedIndex = 3; await LoadScpAsync(_lastScpPath);
    }

    private async Task LoadScpAsync(string path)
    {
        try
        {
            ScpSummary.Text = LocExtension.Get("Visual.Loading");
            _scpImage = await new ScpReader().ReadAsync(path);
            ScpFileName.Text = Path.GetFileName(path);
            var heads = _scpImage.Tracks.Select(x => x.Head).Distinct().Order().ToArray();
            ScpSummary.Text = LocExtension.Get("Visual.Summary", _scpImage.Header.VersionText, _scpImage.Tracks.Count, _scpImage.Header.Revolutions, _scpImage.Header.ResolutionNanoseconds, LocExtension.Get(_scpImage.ChecksumValid ? "Visual.ChecksumValid" : "Visual.ChecksumInvalid"));
            ScpSide0.SetImage(_scpImage, 0); ScpSide1.SetImage(_scpImage, 1);
            _selectedScpTrack = null;
            ScpSide0.Visibility = heads.Contains(0) ? Visibility.Visible : Visibility.Collapsed; ScpSide1.Visibility = heads.Contains(1) ? Visibility.Visible : Visibility.Collapsed;
            Grid.SetColumn(ScpSide0, 0); Grid.SetColumnSpan(ScpSide0, heads.Length == 1 && heads.Contains(0) ? 2 : 1);
            Grid.SetColumn(ScpSide1, heads.Length == 1 && heads.Contains(1) ? 0 : 1); Grid.SetColumnSpan(ScpSide1, heads.Length == 1 && heads.Contains(1) ? 2 : 1);
            ScpTrackInfo.Text = LocExtension.Get("Visual.SelectTrack");
        }
        catch (Exception exception) { _scpImage = null; ScpSummary.Text = LocExtension.Get("Visual.Invalid"); MessageBox.Show(exception.Message, LocExtension.Get("Visual.Title"), MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void ScpTrack_Selected(object? sender, ScpTrack? track)
    {
        _selectedScpTrack = track;
        UpdateScpInspector();
    }

    private void ScpDecoder_Changed(object sender, SelectionChangedEventArgs e)
    {
        var decoderId = (ScpDecoderCombo.SelectedItem as ScpDecoderChoice)?.Id;
        ScpSide0?.SetDecoder(decoderId); ScpSide1?.SetDecoder(decoderId);
        UpdateScpInspector();
    }

    private void UpdateScpInspector()
    {
        var track = _selectedScpTrack;
        if (track is null || _scpImage is null || ScpTrackInfo is null) return;
        var choice = ScpDecoderCombo.SelectedItem as ScpDecoderChoice;
        ScpTrackInfo.Text = _scpInspector.Build(_scpImage, track, choice?.Id);
    }

    private void ScpZoom_Changed(object? sender, float zoom)
    {
        if (_syncingScpZoom || LinkScpViews.IsChecked != true) return;
        _syncingScpZoom = true; try { (ReferenceEquals(sender, ScpSide0) ? ScpSide1 : ScpSide0).SetZoom(zoom); } finally { _syncingScpZoom = false; }
    }

    private void ResetScpViews_Click(object sender, RoutedEventArgs e) { ScpSide0.ResetView(); ScpSide1.ResetView(); }
    private void ToggleScpInspector_Click(object sender, RoutedEventArgs e) { var visible = ScpInspector.Visibility != Visibility.Visible; ScpInspector.Visibility = visible ? Visibility.Visible : Visibility.Collapsed; ScpInspectorColumn.Width = visible ? new GridLength(290) : new GridLength(0); }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = await _settingsStore.LoadAsync();
        if (!string.IsNullOrWhiteSpace(_settings.GwExecutablePath))
            _gwCapabilities = await new GwFormatCapabilityReader().ReadAsync(_settings.GwExecutablePath);
        LoadConfiguredDiskDefs();
        RebuildFormatCatalog();
        ScpDecoderCombo.ItemsSource = new[] { new ScpDecoderChoice(null, LocExtension.Get("Visual.Automatic")) }.Concat(_fluxDecoders.Decoders.Select(x => new ScpDecoderChoice(x.Id, DecoderName(x.Id)))).ToArray();
        ScpDecoderCombo.SelectedIndex = 0;
        _profiles = new InMemoryProfileStore(_settings.Profiles.Select(ToProfile));
        RestoreWindowPlacement();
        ConstrainToCurrentWorkArea();
        _viewModel.Read.Folder = _settings.DefaultImagesFolder;
        ReadFamilyCombo.ItemsSource = _formatCatalog.Formats.Where(x => x.Family != "Raw").Select(x => x.Family).Distinct().Order().ToArray();
        ReadFamilyCombo.SelectedIndex = 0;
        RefreshReadProfiles();
        RefreshWriteProfiles();
        RefreshConvertProfiles();
        RestoreReadSettings();
        RestoreWriteSettings();
        RestoreConversionSettings();
        BuildConversionFormats(null);
        RefreshHardwareSelector();
        SetConsoleVisibility(_settings.ConsoleExpanded);
        UpdateReadCommand();
        UpdateProfileStatus();
        _ = CheckHostToolsUpdateAsync();
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabs?.SelectedIndex == 1) UpdateWriteCommand();
        else if (MainTabs?.SelectedIndex == 0) UpdateReadCommand();
        else if (MainTabs?.SelectedIndex == 2) UpdateConvertCommand();
        else if (MainTabs?.SelectedIndex == 4) UpdateToolCommand();
        UpdateProfileStatus();
    }

    private void RefreshWriteProfiles(string? selectedId = null)
    {
        var items = LocalizedProfiles(OperationKind.Write);
        WriteProfileCombo.ItemsSource = items;
        WriteProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

    private void RefreshConvertProfiles(string? selectedId = null)
    {
        var items = LocalizedProfiles(OperationKind.Convert);
        ConvertProfileCombo.ItemsSource = items;
        ConvertProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

    private void BrowseWriteSource_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get("Common.DiskImageFilter"), InitialDirectory = ReadFolder.Text };
        if (dialog.ShowDialog(this) != true) return;
        _viewModel.Write.SourcePath = dialog.FileName;
        _detectedWriteFormat = _formatDetector.Detect(dialog.FileName, new FileInfo(dialog.FileName).Length);
        WriteDetectionText.Text = $"{_detectedWriteFormat.Format?.DisplayName ?? LocExtension.Get("Detection.Ambiguous")} — {LocExtension.Get(_detectedWriteFormat.ExplanationKey)}";
        WriteFormatCombo.ItemsSource = _detectedWriteFormat.Candidates.Count > 0 ? _detectedWriteFormat.Candidates : _formatCatalog.Formats;
        WriteFormatCombo.SelectedItem = _detectedWriteFormat.Format;
        WriteFormatCombo.Visibility = _detectedWriteFormat.RequiresUserChoice ? Visibility.Visible : Visibility.Collapsed;
        UpdateWriteCommand();
    }

    private void ToggleWriteFormat_Click(object sender, RoutedEventArgs e)
    {
        if (WriteFormatCombo.ItemsSource is null) WriteFormatCombo.ItemsSource = _formatCatalog.Formats;
        WriteFormatCombo.Visibility = WriteFormatCombo.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void WriteInput_Changed(object sender, RoutedEventArgs e) => UpdateWriteCommand();

    private GwCommand BuildWriteCommand()
    {
        return WriteCommandBuilder.Build(new WriteRequest(_settings.GwExecutablePath ?? "gw.exe", _viewModel.Write.SourcePath,
            (WriteFormatCombo?.SelectedItem as DiskFormat)?.Id ?? _detectedWriteFormat?.Format?.Id, _viewModel.Write.BuildOptions(),
            _viewModel.Write.DisableVerification, SelectedHardware()?.Port, SelectedDriveArgument(), _viewModel.Write.ExpertArguments));
    }

    private void UpdateWriteCommand()
    {
        if (CommandPreview is null || WriteSourceText is null || MainTabs?.SelectedIndex != 1) return;
        try { CommandPreview.Text = BuildWriteCommand().ToDisplayString(); }
        catch (ArgumentException exception) { CommandPreview.Text = $"⚠ {exception.Message}"; }
    }

    private async void ExecuteWrite_Click(object sender, RoutedEventArgs e)
    {
        if (_runner.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!ValidateDiskDefs(WriteDiskDefsEnabled, WriteDiskDefsValue, LocExtension.Get("Write.Title"))) return;
        if (!File.Exists(WriteSourceText.Text)) { MessageBox.Show(LocExtension.Get("Write.SelectSource"), LocExtension.Get("Write.Title"), MessageBoxButton.OK, MessageBoxImage.Information); return; }
        var selected = WriteFormatCombo.SelectedItem as DiskFormat ?? _detectedWriteFormat?.Format;
        if (selected is null || (_detectedWriteFormat?.RequiresUserChoice == true && WriteFormatCombo.SelectedItem is null))
        { MessageBox.Show(LocExtension.Get("Write.Ambiguous"), LocExtension.Get("Write.Title"), MessageBoxButton.OK, MessageBoxImage.Warning); WriteFormatCombo.Visibility = Visibility.Visible; return; }
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)) { MessageBox.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), MessageBoxButton.OK, MessageBoxImage.Information); return; }
        GwCommand command;
        try { command = BuildWriteCommand(); }
        catch (ArgumentException exception) { ShowAdvancedValidation(exception, LocExtension.Get("Write.Title")); return; }
        var warning = LocExtension.Get(_viewModel.Write.DisableVerification ? "Write.VerifyOff" : "Write.VerifyOn");
        var confirmation = LocExtension.Get("Write.Confirm", Path.GetFileName(WriteSourceText.Text), selected.DisplayName, SelectedHardware()?.Label ?? LocExtension.Get("Hardware.NotConfigured"), warning);
        if (MessageBox.Show(confirmation, LocExtension.Get("Write.ConfirmTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
        _cancellation = new CancellationTokenSource(); WriteExecuteButton.Content = LocExtension.Get("Common.Stop"); LogOutput.Clear(); BeginProgress();
        var output = new Progress<GwOutputLine>(ReportOutput);
        try { var result = await _runner.RunAsync(command, output, _cancellation.Token); SetOperationResult(result); LogOutput.AppendText(Environment.NewLine + LocExtension.Get("Operation.Finished", result.ExitCode, result.Duration.ToString("g"))); }
        catch (Exception exception) { SetOperationError(); LogOutput.AppendText(LocExtension.Get("Operation.Error", exception.Message)); }
        finally { EndProgress(); WriteExecuteButton.Content = LocExtension.Get("Common.Execute"); _cancellation.Dispose(); _cancellation = null; }
    }

    private void WriteProfile_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (WriteProfileCombo.SelectedItem is not OperationProfile profile || WriteNoVerify is null) return;
        _viewModel.Write.ApplyOptions(profile.EnabledOptions, profile.Values);
        UpdateWriteCommand();
        UpdateProfileStatus();
    }

    private void ResetWriteProfile_Click(object sender, RoutedEventArgs e) { if (WriteProfileCombo.SelectedItem is OperationProfile profile) { WriteProfileCombo.SelectedItem = null; WriteProfileCombo.SelectedItem = profile; } }

    private void SaveWriteProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProfileNameWindow { Owner = this }; if (dialog.ShowDialog() != true) return;
        var enabled = _viewModel.Write.CaptureEnabledOptions();
        var values = _viewModel.Write.CaptureValues();
        var profile = new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Write, dialog.ProfileName, values, enabled);
        try { profile = _profiles.Save(profile); } catch (InvalidOperationException) { if (MessageBox.Show(LocExtension.Get("Profile.Replace"), LocExtension.Get("Profile.Title"), MessageBoxButton.YesNo) != MessageBoxResult.Yes) return; profile = _profiles.Save(profile, true); }
        RefreshWriteProfiles(profile.Id);
    }

    private void BuildConversionFormats(string? sourceExtension, DetectedImageFormat? detection = null)
    {
        if (ConvertCommonPanel is null) return;
        foreach (var control in _conversionControls) _viewModel.Conversion.SetFormat(control.Format.Id, control.IsSelected, control.ExplicitExtensions);
        var selected = _viewModel.Conversion.SelectedFormats;
        var extensions = _viewModel.Conversion.ExplicitExtensions;
        _conversionControls.Clear(); ConvertPinnedPanel.Children.Clear(); ConvertCommonPanel.Children.Clear(); ConvertRarePanel.Children.Clear();
        var compatible = ConversionSourceCompatibility.GetOutputs(_formatCatalog, sourceExtension, detection).Select(x => x.Id).ToHashSet();
        foreach (var format in _formatCatalog.Formats.Where(x => x.Id != "raw.scp").OrderBy(x => x.Family).ThenBy(x => x.DisplayName))
        {
            var control = new ConversionFormatControl(format) { IsEnabled = compatible.Contains(format.Id) };
            if (!control.IsEnabled) control.ToolTip = LocExtension.Get("Conversion.Incompatible", format.DisplayName);
            control.SetState(selected.Contains(format.Id) && control.IsEnabled, extensions.GetValueOrDefault(format.Id));
            control.ValueChanged += ConversionSelectionChanged; _conversionControls.Add(control);
            (control.IsSelected ? ConvertPinnedPanel : format.IsCommon ? ConvertCommonPanel : ConvertRarePanel).Children.Add(control);
        }
    }

    private void ConvertProfile_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ConvertProfileCombo.SelectedItem is not OperationProfile profile || ConvertTracksEnabled is null) return;
        ApplyConvertProfile(profile);
        UpdateProfileStatus();
    }

    private void ApplyConvertProfile(OperationProfile profile)
    {
        _viewModel.Conversion.ApplyProfile(profile.EnabledOptions, profile.Values);
        foreach (var control in _conversionControls.ToArray())
        {
            control.SetState(_viewModel.Conversion.SelectedFormats.Contains(control.Format.Id) && control.IsEnabled, _viewModel.Conversion.ExplicitExtensions.GetValueOrDefault(control.Format.Id));
        }
        UpdateConvertCommand();
    }

    private void ResetConvertProfile_Click(object sender, RoutedEventArgs e) { if (ConvertProfileCombo.SelectedItem is OperationProfile profile) ApplyConvertProfile(profile); }

    private void SaveConvertProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProfileNameWindow { Owner = this }; if (dialog.ShowDialog() != true) return;
        foreach (var control in _conversionControls) _viewModel.Conversion.SetFormat(control.Format.Id, control.IsSelected, control.ExplicitExtensions);
        var enabled = _viewModel.Conversion.CaptureProfileEnabled();
        var values = _viewModel.Conversion.CaptureProfileValues();
        var profile = new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Convert, dialog.ProfileName, values, enabled);
        try { profile = _profiles.Save(profile); } catch (InvalidOperationException) { if (MessageBox.Show(LocExtension.Get("Profile.Replace"), LocExtension.Get("Profile.Title"), MessageBoxButton.YesNo) != MessageBoxResult.Yes) return; profile = _profiles.Save(profile, true); }
        RefreshConvertProfiles(profile.Id);
    }

    private void ConversionSelectionChanged(object? sender, EventArgs e)
    {
        if (sender is not ConversionFormatControl control) return;
        _viewModel.Conversion.SetFormat(control.Format.Id, control.IsSelected, control.ExplicitExtensions);
        if (control.Parent is Panel oldParent) oldParent.Children.Remove(control);
        var destination = control.IsSelected ? ConvertPinnedPanel : control.Format.IsCommon ? ConvertCommonPanel : ConvertRarePanel;
        var index = destination.Children.OfType<ConversionFormatControl>().TakeWhile(x => string.Compare(x.Format.DisplayName, control.Format.DisplayName, StringComparison.CurrentCulture) < 0).Count();
        destination.Children.Insert(index, control); UpdateConvertCommand();
    }

    private void BrowseConvertSource_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get("Common.DiskImageFilter"), InitialDirectory = ReadFolder.Text };
        if (dialog.ShowDialog(this) != true) return;
        _viewModel.Conversion.SourcePath = dialog.FileName; _viewModel.Conversion.OutputName = Path.GetFileNameWithoutExtension(dialog.FileName);
        var detection = _formatDetector.Detect(dialog.FileName, new FileInfo(dialog.FileName).Length);
        ConvertSourceInfo.Text = detection.Format?.DisplayName ?? LocExtension.Get("Conversion.SourceAmbiguous");
        BuildConversionFormats(Path.GetExtension(dialog.FileName), detection); UpdateConvertCommand();
    }

    private void ConvertInput_Changed(object sender, RoutedEventArgs e) => UpdateConvertCommand();

    private IReadOnlyList<ConversionOutput> PlanConversions()
    {
        if (string.IsNullOrWhiteSpace(_viewModel.Conversion.SourcePath)) return [];
        return new ConversionPlanner(_formatCatalog).Plan(_viewModel.Conversion.SourcePath, ReadFolder.Text, _viewModel.Conversion.OutputName.Trim(), _viewModel.Conversion.BuildSelections(_formatCatalog.Formats), _viewModel.Conversion.AddTags, _settings.Conversion.TagPattern);
    }

    private EnabledOption[] GetConvertOptions()
    {
        return _viewModel.Conversion.BuildOptions().ToArray();
    }

    private void UpdateConvertCommand()
    {
        if (CommandPreview is null || ConvertSourceText is null || MainTabs?.SelectedIndex != 2) return;
        try
        {
            var outputs = PlanConversions();
            if (outputs.Count == 0) { CommandPreview.Text = LocExtension.Get("Conversion.SelectOutput"); return; }
            var first = ConversionCommandBuilder.Build(_settings.GwExecutablePath ?? "gw.exe", _viewModel.Conversion.SourcePath, outputs[0], GetConvertOptions(), _viewModel.Conversion.ExpertArguments);
            CommandPreview.Text = first.ToDisplayString() + (outputs.Count > 1 ? LocExtension.Get("Conversion.More", outputs.Count - 1) : "");
        }
        catch (Exception exception) { CommandPreview.Text = $"⚠ {exception.Message}"; }
    }

    private async void ExecuteConvert_Click(object sender, RoutedEventArgs e)
    {
        if (_runner.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!ValidateDiskDefs(ConvertDiskDefsEnabled, ConvertDiskDefsValue, LocExtension.Get("Conversion.Title"))) return;
        if (!File.Exists(ConvertSourceText.Text)) { MessageBox.Show(LocExtension.Get("Conversion.SourceRequired"), LocExtension.Get("Conversion.Title")); return; }
        if (string.IsNullOrWhiteSpace(ConvertOutputName.Text)) { MessageBox.Show(LocExtension.Get("Conversion.NameRequired"), LocExtension.Get("Conversion.Title")); return; }
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)) { MessageBox.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title")); return; }
        IReadOnlyList<ConversionOutput> outputs;
        try { outputs = PlanConversions(); GwOptionValidator.Validate(GetConvertOptions()); } catch (Exception exception) { ShowAdvancedValidation(exception, LocExtension.Get("Conversion.Title")); return; }
        if (outputs.Count == 0) { MessageBox.Show(LocExtension.Get("Conversion.CheckOutput"), LocExtension.Get("Conversion.Title")); return; }
        var existing = outputs.Where(x => File.Exists(x.OutputPath)).ToArray();
        if (existing.Length > 0)
        {
            var dialog = new ConversionConflictWindow(existing) { Owner = this }; if (dialog.ShowDialog() != true) return;
            var resolved = outputs.Except(existing).ToList();
            foreach (var row in dialog.Rows)
            {
                if (row.Choice == ConversionConflictChoice.Skip) continue;
                resolved.Add(row.Choice == ConversionConflictChoice.Number ? row.Output with { OutputPath = NumberedPath(row.Output.OutputPath) } : row.Output);
            }
            outputs = resolved;
        }
        _cancellation = new CancellationTokenSource(); ConvertExecuteButton.Content = LocExtension.Get("Common.Stop"); LogOutput.Clear(); BeginProgress();
        var progress = new Progress<GwOutputLine>(ReportOutput);
        try
        {
            var items = outputs.Select(planned => new GwBatchItem(Path.GetFileName(planned.OutputPath), ConversionCommandBuilder.Build(_settings.GwExecutablePath, _viewModel.Conversion.SourcePath, planned, GetConvertOptions(), _viewModel.Conversion.ExpertArguments))).ToArray();
            var result = await new GwBatchExecutor(_runner).RunAsync(items, progress, item => Dispatcher.Invoke(() => { BeginProgress(); LogOutput.AppendText($"{Environment.NewLine}→ {item.Label}{Environment.NewLine}"); }), _cancellation.Token);
            if (result.WasCancelled) SetOperationCancelled(); else if (result.FailedLabels.Count == 0) SetOperationSuccess(); else SetOperationError();
            LogOutput.AppendText(Environment.NewLine + LocExtension.Get("Conversion.Summary", result.SuccessfulCount, result.FailedLabels.Count) + (result.FailedLabels.Count > 0 ? LocExtension.Get("Conversion.Failures", string.Join(", ", result.FailedLabels)) : ""));
        }
        catch (Exception exception) { SetOperationError(); LogOutput.AppendText(LocExtension.Get("Operation.Error", exception.Message)); }
        finally { EndProgress(); ConvertExecuteButton.Content = LocExtension.Get("Common.Execute"); _cancellation.Dispose(); _cancellation = null; }
    }

    private static string NumberedPath(string path)
    {
        var folder = Path.GetDirectoryName(path)!; var name = Path.GetFileNameWithoutExtension(path); var extension = Path.GetExtension(path);
        for (var number = 1; number < int.MaxValue; number++) { var candidate = Path.Combine(folder, $"{name} ({number}){extension}"); if (!File.Exists(candidate)) return candidate; }
        throw new IOException("Impossible de trouver un nom de sortie disponible.");
    }

    private void CaptureConversionSettings()
    {
        foreach (var control in _conversionControls) _viewModel.Conversion.SetFormat(control.Format.Id, control.IsSelected, control.ExplicitExtensions);
        _settings.Conversion.AddTags = _viewModel.Conversion.AddTags;
        _settings.Conversion.SelectedFormats = _viewModel.Conversion.SelectedFormats.ToHashSet();
        _settings.Conversion.ExplicitExtensions = _viewModel.Conversion.ExplicitExtensions.ToDictionary(x => x.Key, x => x.Value.ToHashSet());
        _settings.Conversion.EnabledOptions = _viewModel.Conversion.CaptureEnabledOptions();
        _settings.Conversion.OptionValues = _viewModel.Conversion.CaptureValues();
    }

    private void RestoreConversionSettings()
    {
        _viewModel.Conversion.ApplySettings(_settings.Conversion.AddTags, _settings.Conversion.SelectedFormats, _settings.Conversion.ExplicitExtensions, _settings.Conversion.EnabledOptions, _settings.Conversion.OptionValues);
    }

    private async void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_runner.IsRunning)
        {
            var answer = MessageBox.Show(LocExtension.Get("App.OperationRunningClose"), LocExtension.Get("App.Title"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
        CaptureWriteSettings();
        CaptureProfiles();
        CaptureConversionSettings();
        await _settingsStore.SaveAsync(_settings);
    }

    private void RefreshReadProfiles(string? selectedId = null)
    {
        var items = LocalizedProfiles(OperationKind.Read);
        ReadProfileCombo.ItemsSource = items;
        ReadProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

    private IReadOnlyList<OperationProfile> LocalizedProfiles(OperationKind operation) =>
        _profiles.Get(operation).Select(profile => profile.IsSystem ? profile with { Name = LocExtension.Get("Profile.Default") } : profile).ToArray();

    private void ReadProfile_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ReadProfileCombo.SelectedItem is not OperationProfile profile || ReadRevsEnabled is null) return;
        ApplyReadProfile(profile);
        UpdateProfileStatus();
    }

    private void ResetReadProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ReadProfileCombo.SelectedItem is OperationProfile profile) ApplyReadProfile(profile);
    }

    private void ApplyReadProfile(OperationProfile profile)
    {
        _viewModel.Read.ApplyOptions(profile.EnabledOptions, profile.Values);
        if (profile.IsSystem)
        {
            RawScpRadio.IsChecked = true;
        }
        UpdateReadCommand();
    }

    private void SaveReadProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProfileNameWindow { Owner = this };
        if (dialog.ShowDialog() != true) return;
        var enabled = _viewModel.Read.CaptureEnabledOptions();
        var values = _viewModel.Read.CaptureValues();
        var profile = new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Read, dialog.ProfileName, values, enabled);
        try { profile = _profiles.Save(profile); }
        catch (InvalidOperationException)
        {
            if (MessageBox.Show(LocExtension.Get("Profile.Replace"), LocExtension.Get("Profile.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
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

    private void LogHistory_Click(object sender, RoutedEventArgs e) => new LogHistoryWindow(_logsDirectory) { Owner = this }.ShowDialog();
    private void About_Click(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();
    private void Documentation_Click(object sender, RoutedEventArgs e)
    {
        var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr" ? "fr" : "en";
        var path = Path.Combine(AppContext.BaseDirectory, "Documentation", $"user-guide.{language}.md");
        if (File.Exists(path)) Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    private async void ExportConsole_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = LocExtension.Get("Logs.ExportFilter"), FileName = $"gw-gui-{DateTime.Now:yyyyMMdd-HHmmss}.txt", DefaultExt = ".txt" };
        if (dialog.ShowDialog(this) != true) return;
        await File.WriteAllTextAsync(dialog.FileName, CommandPreview.Text + Environment.NewLine + Environment.NewLine + LogOutput.Text);
    }

    private void SetConsoleVisibility(bool visible)
    {
        if (!visible && ConsolePanel.Visibility == Visibility.Visible && ConsoleRow.ActualHeight >= 100)
            _settings.ConsoleHeight = ConsoleRow.ActualHeight;
        ConsolePanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        ConsoleSplitter.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
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
        ReadExtensionText.Text = RawScpRadio?.IsChecked == true ? LocExtension.Get("Read.RawScp") : (ReadExtensionCombo?.SelectedItem as ImageExtension)?.DisplayName ?? LocExtension.Get("Read.ChooseType");
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
        => _viewModel.Read.BuildTarget(extension, "Exemple");

    private string GetReadExtension() => RawScpRadio?.IsChecked == true ? ".scp" : (ReadExtensionCombo?.SelectedItem as ImageExtension)?.Extension ?? "";

    private GwCommand BuildReadCommand(string target)
    {
        return ReadCommandBuilder.Build(new ReadRequest(
            _settings.GwExecutablePath ?? "gw.exe", target,
            RawScpRadio?.IsChecked == true ? ReadResultKind.RawScp : ReadResultKind.KnownFormat,
            (ReadFormatCombo?.SelectedItem as DiskFormat)?.Id, _viewModel.Read.BuildOptions(),
            SelectedHardware()?.Port, SelectedDriveArgument(), _viewModel.Read.ExpertArguments));
    }

    private static string SelectedText(ComboBox combo) => (combo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;

    private void ReadFakeIndex_Checked(object sender, RoutedEventArgs e) { _viewModel.Read.EnableFakeIndex(); ReadInput_Changed(sender, e); }
    private void ReadSequenceKind_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ReadSequenceValue is null) return;
        var targetKind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
        var sourceKind = targetKind == SequenceKind.Alphabetic ? SequenceKind.Numeric : SequenceKind.Alphabetic;
        if (SequenceFormatter.TryParse(ReadSequenceValue.Text, sourceKind, out var value))
            _viewModel.Read.SequenceValue = targetKind == SequenceKind.Numeric
                ? (value + 1).ToString()
                : SequenceFormatter.Format(Math.Max(0, value - 1), targetKind, 1);
        UpdateReadCommand();
    }
    private void ReadHardSectors_Checked(object sender, RoutedEventArgs e) { _viewModel.Read.EnableHardSectors(); ReadInput_Changed(sender, e); }
    private void ReadDensel_Checked(object sender, RoutedEventArgs e) { _viewModel.Read.EnableDensel(); ReadInput_Changed(sender, e); }
    private void ReadTg43_Checked(object sender, RoutedEventArgs e) { _viewModel.Read.EnableTg43(); ReadInput_Changed(sender, e); }
    private void WriteFakeIndex_Checked(object sender, RoutedEventArgs e) { _viewModel.Write.EnableFakeIndex(); WriteInput_Changed(sender, e); }
    private void WriteHardSectors_Checked(object sender, RoutedEventArgs e) { _viewModel.Write.EnableHardSectors(); WriteInput_Changed(sender, e); }
    private void WriteDensel_Checked(object sender, RoutedEventArgs e) { _viewModel.Write.EnableDensel(); WriteInput_Changed(sender, e); }
    private void WriteTg43_Checked(object sender, RoutedEventArgs e) { _viewModel.Write.EnableTg43(); WriteInput_Changed(sender, e); }

    private void BrowseReadDiskDefs_Click(object sender, RoutedEventArgs e) => BrowseDiskDefs(ReadDiskDefsValue, ReadDiskDefsEnabled, UpdateReadCommand);
    private void BrowseWriteDiskDefs_Click(object sender, RoutedEventArgs e) => BrowseDiskDefs(WriteDiskDefsValue, WriteDiskDefsEnabled, UpdateWriteCommand);
    private void BrowseConvertDiskDefs_Click(object sender, RoutedEventArgs e) => BrowseDiskDefs(ConvertDiskDefsValue, ConvertDiskDefsEnabled, UpdateConvertCommand);

    private void BrowseDiskDefs(TextBox target, CheckBox enabled, Action refresh)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get("Advanced.DiskDefsFilter"), FileName = target.Text };
        if (dialog.ShowDialog(this) != true) return;
        try { AddDiskDefs(dialog.FileName); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            ShowAdvancedValidation(exception, LocExtension.Get("Advanced.DiskDefs"));
            return;
        }
        if (ReferenceEquals(target, ReadDiskDefsValue)) { _viewModel.Read.DiskDefs.Value = dialog.FileName; _viewModel.Read.DiskDefs.Enabled = true; }
        else if (ReferenceEquals(target, WriteDiskDefsValue)) { _viewModel.Write.DiskDefs.Value = dialog.FileName; _viewModel.Write.DiskDefs.Enabled = true; }
        else if (ReferenceEquals(target, ConvertDiskDefsValue)) { _viewModel.Conversion.DiskDefs.Value = dialog.FileName; _viewModel.Conversion.DiskDefs.Enabled = true; }
        else { target.Text = dialog.FileName; enabled.IsChecked = true; }
        RefreshFormatSelectors();
        refresh();
    }

    private void LoadConfiguredDiskDefs()
    {
        var paths = new[]
        {
            _settings.Read.OptionValues.GetValueOrDefault("diskdefs"),
            _settings.Write.OptionValues.GetValueOrDefault("diskdefs"),
            _settings.Conversion.OptionValues.GetValueOrDefault("diskdefs")
        }.Concat(_settings.Profiles.Select(profile => profile.Values.GetValueOrDefault("diskdefs")));
        foreach (var path in paths.Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try { AddDiskDefs(path!); }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException) { /* Validation is shown when the profile is executed. */ }
        }
    }

    private void AddDiskDefs(string path)
    {
        var discovered = DiskDefsFormatReader.Read(path);
        var ids = _gwCapabilities.FormatIds.Concat(discovered).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var extensions = _gwCapabilities.ImageExtensions.Count > 0
            ? _gwCapabilities.ImageExtensions
            : new HashSet<string>([".scp", ".img", ".ima", ".hfe"], StringComparer.OrdinalIgnoreCase);
        _gwCapabilities = new GwFormatCapabilities(ids, extensions);
        RebuildFormatCatalog();
    }

    private void RebuildFormatCatalog()
    {
        _formatCatalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(key => LocExtension.Get(key)), _gwCapabilities);
        _formatDetector = new ImageFormatDetector(_formatCatalog);
    }

    private void RefreshFormatSelectors()
    {
        var selectedReadId = (ReadFormatCombo.SelectedItem as DiskFormat)?.Id;
        var selectedFamily = (ReadFormatCombo.SelectedItem as DiskFormat)?.Family ?? ReadFamilyCombo.SelectedItem as string;
        var families = _formatCatalog.Formats.Where(format => format.Family != "Raw").Select(format => format.Family).Distinct().Order().ToArray();
        ReadFamilyCombo.ItemsSource = families;
        ReadFamilyCombo.SelectedItem = selectedFamily is not null && families.Contains(selectedFamily) ? selectedFamily : families.FirstOrDefault();
        if (selectedReadId is not null)
            ReadFormatCombo.SelectedItem = _formatCatalog.Formats.FirstOrDefault(format => format.Id == selectedReadId);

        if (WriteFormatCombo.ItemsSource is not null)
        {
            var selectedWriteId = (WriteFormatCombo.SelectedItem as DiskFormat)?.Id;
            WriteFormatCombo.ItemsSource = _formatCatalog.Formats.Where(format => format.Family != "Raw").ToArray();
            WriteFormatCombo.SelectedItem = _formatCatalog.Formats.FirstOrDefault(format => format.Id == selectedWriteId);
        }

        DetectedImageFormat? detection = null;
        var sourceExtension = string.IsNullOrWhiteSpace(ConvertSourceText.Text) ? null : Path.GetExtension(ConvertSourceText.Text);
        if (File.Exists(ConvertSourceText.Text)) detection = _formatDetector.Detect(ConvertSourceText.Text, new FileInfo(ConvertSourceText.Text).Length);
        BuildConversionFormats(sourceExtension, detection);
    }

    private bool ValidateDiskDefs(CheckBox enabled, TextBox path, string title)
    {
        if (enabled.IsChecked != true || File.Exists(path.Text)) return true;
        MessageBox.Show(LocExtension.Get("Advanced.DiskDefsMissing"), title, MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    private void ShowAdvancedValidation(Exception exception, string title) =>
        MessageBox.Show(LocExtension.Get("Advanced.Invalid", exception.Message), title, MessageBoxButton.OK, MessageBoxImage.Warning);

    private void CopyReadName_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ReadFileName.Text)) Clipboard.SetText(ReadFileName.Text);
    }

    private void BrowseReadFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { InitialDirectory = ReadFolder.Text, Title = LocExtension.Get("Read.DestinationFolder") };
        if (dialog.ShowDialog(this) == true) { _viewModel.Read.Folder = dialog.FolderName; UpdateReadCommand(); }
    }

    private async void ExecuteRead_Click(object sender, RoutedEventArgs e)
    {
        if (_runner.IsRunning) { ConfirmAndRequestStop(); return; }
        if (!ValidateDiskDefs(ReadDiskDefsEnabled, ReadDiskDefsValue, LocExtension.Get("Read.Title"))) return;
        if (string.IsNullOrWhiteSpace(ReadFileName.Text))
        {
            MessageBox.Show(LocExtension.Get("Read.NameRequired"), LocExtension.Get("Read.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            MessageBox.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var extension = GetReadExtension();
        if (string.IsNullOrWhiteSpace(extension)) { MessageBox.Show(LocExtension.Get("Read.TypeRequired"), LocExtension.Get("Read.Title"), MessageBoxButton.OK, MessageBoxImage.Information); return; }
        var target = GetReadTarget(extension);
        if (File.Exists(target))
        {
            var answer = MessageBox.Show(LocExtension.Get("Read.FileExists"), LocExtension.Get("Read.FileExistsTitle"), MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (answer == MessageBoxResult.Cancel) { ReadFileName.Focus(); ReadFileName.SelectAll(); return; }
            if (answer == MessageBoxResult.No)
            {
                if (ReadAutoNumber.IsChecked != true) ReadAutoNumber.IsChecked = true;
                var kind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
                if (!SequenceFormatter.TryParse(ReadSequenceValue.Text, kind, out var next)) next = kind == SequenceKind.Alphabetic ? 0 : 1;
                var available = OutputConflictResolver.FindNextAvailableWithValue(ReadFolder.Text, ReadFileName.Text.Trim(), extension, kind, ReadSequenceWidth.SelectedIndex + 1, next);
                target = available.Path;
                _viewModel.Read.SequenceValue = kind == SequenceKind.Numeric ? available.Value.ToString() : SequenceFormatter.Format(available.Value, kind, 1);
            }
        }
        GwCommand command;
        try { command = BuildReadCommand(target); }
        catch (ArgumentException exception) { ShowAdvancedValidation(exception, LocExtension.Get("Read.Title")); return; }
        _cancellation = new CancellationTokenSource();
        ReadExecuteButton.Content = LocExtension.Get("Common.Stop");
        LogOutput.Clear();
        BeginProgress();
        var output = new Progress<GwOutputLine>(ReportOutput);
        try
        {
            var result = await _runner.RunAsync(command, output, _cancellation.Token);
            SetOperationResult(result);
            LogOutput.AppendText(Environment.NewLine + LocExtension.Get("Operation.Finished", result.ExitCode, result.Duration.ToString("g")));
            if (result.IsSuccess && extension.Equals(".scp", StringComparison.OrdinalIgnoreCase)) { _lastScpPath = target; OpenScpBanner.Visibility = Visibility.Visible; }
            var sequenceKind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
            if (result.IsSuccess) _viewModel.Read.TryAdvanceSequence();
        }
        catch (Exception exception) { SetOperationError(); LogOutput.AppendText(LocExtension.Get("Operation.Error", exception.Message)); }
        finally { EndProgress(); ReadExecuteButton.Content = LocExtension.Get("Common.Execute"); _cancellation.Dispose(); _cancellation = null; }
    }

    private void RestoreReadSettings()
    {
        KnownFormatRadio.IsChecked = _settings.Read.UseKnownFormat;
        RawScpRadio.IsChecked = !_settings.Read.UseKnownFormat;
        _viewModel.Read.AutoNumber = _settings.Read.AutoNumber;
        _viewModel.Read.SequenceKindIndex = _settings.Read.SequenceKind == "Alphabetic" ? 1 : 0;
        _viewModel.Read.SequenceWidthIndex = Math.Clamp(_settings.Read.SequenceWidth - 1, 0, 2);
        _viewModel.Read.SequenceValue = _settings.Read.SequenceKind == "Alphabetic" ? SequenceFormatter.Format(_settings.Read.NextSequence, SequenceKind.Alphabetic, 1) : _settings.Read.NextSequence.ToString();
        _viewModel.Read.ApplyOptions(_settings.Read.EnabledOptions, _settings.Read.OptionValues);
    }

    private void CaptureReadSettings()
    {
        _settings.Read.UseKnownFormat = KnownFormatRadio.IsChecked == true;
        _settings.Read.FormatId = (ReadFormatCombo.SelectedItem as DiskFormat)?.Id;
        _settings.Read.AutoNumber = _viewModel.Read.AutoNumber;
        _settings.Read.SequenceKind = _viewModel.Read.SequenceKind == SequenceKind.Alphabetic ? "Alphabetic" : "Numeric";
        _settings.Read.SequenceWidth = _viewModel.Read.SequenceWidthIndex + 1;
        if (SequenceFormatter.TryParse(_viewModel.Read.SequenceValue, _viewModel.Read.SequenceKind, out var sequence)) _settings.Read.NextSequence = sequence;
        _settings.Read.EnabledOptions = _viewModel.Read.CaptureEnabledOptions();
        _settings.Read.OptionValues = _viewModel.Read.CaptureValues();
    }

    private void RestoreWriteSettings()
    {
        _viewModel.Write.ApplyOptions(_settings.Write.EnabledOptions, _settings.Write.OptionValues);
    }

    private void CaptureWriteSettings()
    {
        _settings.Write.EnabledOptions = _viewModel.Write.CaptureEnabledOptions();
        _settings.Write.OptionValues = _viewModel.Write.CaptureValues();
    }

    private async void Preferences_Click(object sender, RoutedEventArgs e)
    {
        CaptureProfiles();
        var dialog = new OptionsWindow(_settings) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _profiles = new InMemoryProfileStore(_settings.Profiles.Select(ToProfile));
            RefreshReadProfiles(); RefreshWriteProfiles(); RefreshConvertProfiles();
            _viewModel.Read.Folder = _settings.DefaultImagesFolder;
            RefreshHardwareSelector();
            ((App)Application.Current).SetTheme(_settings.Theme);
            await _settingsStore.SaveAsync(_settings);
            UpdateReadCommand();
        }
    }

    private void RestoreWindowPlacement()
    {
        var placement = WindowPlacementPolicy.Normalize(_settings.Window, MinWidth, MinHeight,
            SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
        Width = placement.Width;
        Height = placement.Height;
        if (placement.Left is double left && placement.Top is double top)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = left;
            Top = top;
        }
        if (_settings.Window.Maximized) WindowState = WindowState.Maximized;
    }

    private void ConstrainToCurrentWorkArea()
    {
        if (WindowState == WindowState.Maximized) return;
        var dpi = VisualTreeHelper.GetDpi(this);
        var raw = SystemParameters.WorkArea;
        var area = new Rect(raw.X / dpi.DpiScaleX, raw.Y / dpi.DpiScaleY, raw.Width / dpi.DpiScaleX, raw.Height / dpi.DpiScaleY);
        Width = Math.Min(Width, area.Width);
        Height = Math.Min(Height, area.Height);
        Left = Math.Clamp(Left, area.Left, Math.Max(area.Left, area.Right - Width));
        Top = Math.Clamp(Top, area.Top, Math.Max(area.Top, area.Bottom - Height));
    }

    private void ToolsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ErasePanel is null) return;
        ErasePanel.Visibility = ToolsList.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        CleanPanel.Visibility = ToolsList.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        UpdateToolCommand();
    }

    private void ToolInput_Changed(object sender, RoutedEventArgs e) => UpdateToolCommand();

    private GwCommand BuildEraseCommand()
    {
        var options = new List<EnabledOption>();
        if (EraseTracksEnabled.IsChecked == true) options.Add(new("--tracks", EraseTracksValue.Text.Trim()));
        if (EraseRevsEnabled.IsChecked == true) options.Add(new("--revs", EraseRevsValue.Text.Trim()));
        var hardware = SelectedHardware();
        return MaintenanceCommandBuilder.Erase(new EraseRequest(_settings.GwExecutablePath ?? "gw.exe", options, hardware?.Port, SelectedDriveArgument(), EraseExpertArguments.Text));
    }

    private GwCommand BuildCleanCommand() => MaintenanceCommandBuilder.Clean(new CleanRequest(_settings.GwExecutablePath ?? "gw.exe",
        CleanCylindersEnabled.IsChecked == true && int.TryParse(CleanCylindersValue.Text, out var cylinders) ? cylinders : null,
        CleanPassesEnabled.IsChecked == true && int.TryParse(CleanPassesValue.Text, out var passes) ? passes : null,
        CleanLingerEnabled.IsChecked == true && int.TryParse(CleanLingerValue.Text, out var linger) ? linger : null,
        SelectedHardware()?.Port, SelectedDriveArgument(), CleanExpertArguments.Text));

    private void RefreshHardwareSelector()
    {
        if (HardwareSelector is null) return;
        var previousId = (HardwareSelector.SelectedItem as HardwareChoice)?.Drive.Id;
        var choices = (from drive in _settings.Drives
                       join controller in _settings.Controllers on drive.ControllerUsbId equals controller.UsbId
                       select new HardwareChoice(drive, controller.LastPort, controller.IsAvailable,
                           LocExtension.Get("Hardware.DriveLabel", drive.Size, drive.Density, controller.LastPort, drive.Selection) + (controller.IsAvailable ? "" : $" ({LocExtension.Get("Hardware.Disconnected")})"))).ToArray();
        HardwareSelector.ItemsSource = choices;
        HardwareSelector.SelectedItem = choices.FirstOrDefault(x => x.Drive.Id == previousId) ?? choices.FirstOrDefault();
        HardwareSelectorItem.Visibility = choices.Length > 1 ? Visibility.Visible : Visibility.Collapsed;
        UpdateHardwareStatus();
    }

    private HardwareChoice? SelectedHardware() => HardwareSelector?.SelectedItem as HardwareChoice;
    private string? SelectedDriveArgument() => HardwareRoutingPolicy.DriveArgument(_settings.Drives, SelectedHardware()?.Drive);
    private void HardwareSelector_Changed(object sender, SelectionChangedEventArgs e) { UpdateHardwareStatus(); UpdateReadCommand(); UpdateWriteCommand(); UpdateToolCommand(); }
    private void UpdateHardwareStatus()
    {
        var selected = SelectedHardware();
        _viewModel.HardwareText = selected is null ? LocExtension.Get("Hardware.NotConfigured") : selected.Label;
        _viewModel.HardwareBrush = new SolidColorBrush(selected?.Available == true ? Color.FromRgb(63, 171, 91) : Color.FromRgb(136, 136, 136));
    }

    private void UpdateToolCommand()
    {
        if (CommandPreview is null || ToolsList is null || MainTabs?.SelectedIndex != 4) return;
        try { CommandPreview.Text = (ToolsList.SelectedIndex == 0 ? BuildEraseCommand() : BuildCleanCommand()).ToDisplayString(); }
        catch (Exception exception) { CommandPreview.Text = $"⚠ {exception.Message}"; }
    }

    private async void ExecuteErase_Click(object sender, RoutedEventArgs e)
    {
        if (_runner.IsRunning) { ConfirmAndRequestStop(); return; }
        if (MessageBox.Show(LocExtension.Get("Maintenance.EraseConfirm"), LocExtension.Get("Maintenance.EraseTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
        await ExecuteMaintenanceAsync(BuildEraseCommand(), EraseExecuteButton);
    }

    private async void ExecuteClean_Click(object sender, RoutedEventArgs e)
    {
        if (_runner.IsRunning) { ConfirmAndRequestStop(); return; }
        if (MessageBox.Show(LocExtension.Get("Maintenance.CleanConfirm"), LocExtension.Get("Maintenance.CleanTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
        await ExecuteMaintenanceAsync(BuildCleanCommand(), CleanExecuteButton);
    }

    private async Task ExecuteMaintenanceAsync(GwCommand command, Button button)
    {
        if (_runner.IsRunning) { ConfirmAndRequestStop(); return; }
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath)) { MessageBox.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title")); return; }
        _cancellation = new CancellationTokenSource(); button.Content = LocExtension.Get("Common.Stop"); LogOutput.Clear(); BeginProgress();
        var progress = new Progress<GwOutputLine>(ReportOutput);
        try { var result = await _runner.RunAsync(command, progress, _cancellation.Token); SetOperationResult(result); LogOutput.AppendText(Environment.NewLine + LocExtension.Get("Operation.Finished", result.ExitCode, result.Duration.ToString("g"))); }
        catch (Exception exception) { SetOperationError(); LogOutput.AppendText(LocExtension.Get("Operation.Error", exception.Message)); }
        finally { EndProgress(); button.Content = LocExtension.Get("Common.Execute"); _cancellation.Dispose(); _cancellation = null; }
    }

    private void ConfirmAndRequestStop()
    {
        if (MessageBox.Show(this, LocExtension.Get("Operation.StopConfirm"), LocExtension.Get("Operation.StopTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            _cancellation?.Cancel();
    }

    private void BeginProgress()
    {
        _progressTracker.Reset();
        SetOperationState("Status.Running", Color.FromRgb(45, 125, 210));
        _viewModel.ProgressVisibility = Visibility.Visible;
        _viewModel.ProgressIndeterminate = true;
        _viewModel.ProgressValue = 0;
        _viewModel.ProgressText = "";
    }

    private void ReportOutput(GwOutputLine line)
    {
        LogOutput.AppendText(line.Text + Environment.NewLine);
        LogOutput.ScrollToEnd();
        var progress = _progressTracker.Accept(line.Text);
        if (progress is null) return;
        if (progress.TotalTracks is int total)
        {
            _viewModel.ProgressIndeterminate = false;
            _viewModel.ProgressValue = progress.Fraction.GetValueOrDefault() * 100;
            _viewModel.ProgressText = LocExtension.Get("Status.TrackProgress", progress.Cylinder, progress.Head, progress.CompletedTracks, total);
        }
        else _viewModel.ProgressText = LocExtension.Get("Status.TrackUnknown", progress.Cylinder, progress.Head, progress.CompletedTracks);
    }

    private void EndProgress()
    {
        _viewModel.ProgressIndeterminate = false;
        _viewModel.ProgressValue = 100;
        _viewModel.ProgressVisibility = Visibility.Collapsed;
    }

    private void SetOperationResult(GwExecutionResult result)
    {
        if (result.WasCancelled) SetOperationCancelled();
        else if (result.IsSuccess) SetOperationSuccess();
        else SetOperationError();
    }

    private void SetOperationSuccess() => SetOperationState("Status.Success", Color.FromRgb(63, 171, 91));
    private void SetOperationError() => SetOperationState("Status.Error", Color.FromRgb(210, 66, 66));
    private void SetOperationCancelled() => SetOperationState("Status.Cancelled", Color.FromRgb(220, 148, 45));
    private void SetOperationState(string resourceKey, Color color)
    {
        _viewModel.OperationText = LocExtension.Get(resourceKey);
        _viewModel.OperationBrush = new SolidColorBrush(color);
    }

    private void UpdateProfileStatus()
    {
        if (ProfileStatusItem is null || MainTabs is null) return;
        string? name = MainTabs.SelectedIndex switch
        {
            0 => (ReadProfileCombo?.SelectedItem as OperationProfile)?.Name,
            1 => (WriteProfileCombo?.SelectedItem as OperationProfile)?.Name,
            2 => (ConvertProfileCombo?.SelectedItem as OperationProfile)?.Name,
            _ => null
        };
        _viewModel.ProfileVisibility = name is null ? Visibility.Collapsed : Visibility.Visible;
        if (name is not null) _viewModel.ProfileText = LocExtension.Get("Status.Profile", name);
    }

    private async Task CheckHostToolsUpdateAsync()
    {
        if (_settings.LastHostToolsCheckUtc is DateTimeOffset checkedAt && DateTimeOffset.UtcNow - checkedAt < TimeSpan.FromDays(1))
        {
            ShowHostToolsUpdateIfNeeded(); return;
        }
        try
        {
            var root = StoragePaths.HostToolsDirectory;
            var manager = new GwInstallationManager(new HttpClient(), root);
            var release = await manager.GetLatestReleaseAsync();
            _settings.AvailableHostToolsVersion = release.Version; _settings.LastHostToolsCheckUtc = DateTimeOffset.UtcNow;
            ShowHostToolsUpdateIfNeeded();
        }
        catch { /* Update checks are intentionally silent. */ }
    }

    private void ShowHostToolsUpdateIfNeeded()
    {
        var available = _settings.AvailableHostToolsVersion;
        var installed = _settings.InstalledHostToolsVersion;
        var newer = Version.TryParse(available, out var availableVersion) && (!Version.TryParse(installed, out var installedVersion) || availableVersion > installedVersion);
        _viewModel.HostToolsUpdateVisibility = newer ? Visibility.Visible : Visibility.Collapsed;
        if (newer) _viewModel.HostToolsUpdateText = LocExtension.Get("HostTools.UpdateAvailable", available!);
    }

    private static string DecoderName(string id) => LocExtension.Get("Visual.DecoderName." + id);
    private void ToolCommand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string verb }) return;
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            MessageBox.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var hardware = SelectedHardware();
        var toolRunner = new GreaseweazleRunner(new RotatingOperationLogWriter(_logsDirectory));
        new GwToolWindow(_settings.GwExecutablePath, verb, hardware?.Port, SelectedDriveArgument(), toolRunner) { Owner = this }.ShowDialog();
    }
}

public sealed record HardwareChoice(DriveSettings Drive, string Port, bool Available, string Label);
