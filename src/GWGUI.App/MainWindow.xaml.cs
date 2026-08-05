using System.ComponentModel;
using System.IO;
using System.Net.Http;
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
using GWGUI.Scp;
using GWGUI.Scp.Decoding;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using Microsoft.Win32;
using GWGUI.App.Localization;
using GWGUI.Infrastructure.HostTools;

namespace GWGUI.App;

public partial class MainWindow : Window
{
    private readonly ISettingsStore _settingsStore;
    private readonly IGreaseweazleRunner _runner = new GreaseweazleRunner();
    private AppSettings _settings = new();
    private CancellationTokenSource? _cancellation;
    private IImageFormatCatalog _formatCatalog = new BuiltInImageFormatCatalog();
    private IProfileStore _profiles = new InMemoryProfileStore();
    private ImageFormatDetector _formatDetector;
    private DetectedImageFormat? _detectedWriteFormat;
    private readonly List<ConversionFormatControl> _conversionControls = [];
    private ScpImage? _scpImage;
    private bool _syncingScpZoom;
    private readonly FluxDecoderRegistry _fluxDecoders = new();
    private string? _lastScpPath;
    private ScpTrack? _selectedScpTrack;
    private readonly GwProgressTracker _progressTracker = new();

    public MainWindow()
    {
        InitializeComponent();
        ScpSide0.TrackSelected += ScpTrack_Selected; ScpSide1.TrackSelected += ScpTrack_Selected;
        ScpSide0.ZoomChanged += ScpZoom_Changed; ScpSide1.ZoomChanged += ScpZoom_Changed;
        _formatDetector = new ImageFormatDetector(_formatCatalog);
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GW GUI");
        _settingsStore = new JsonSettingsStore(Path.Combine(directory, "settings.json"));
    }

    private async void OpenScp_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Capture SuperCard Pro (*.scp)|*.scp|Tous les fichiers|*.*", InitialDirectory = ReadFolder.Text };
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

    private void ScpDecoder_Changed(object sender, SelectionChangedEventArgs e) => UpdateScpInspector();

    private void UpdateScpInspector()
    {
        var track = _selectedScpTrack;
        if (track is null || _scpImage is null || ScpTrackInfo is null) return;
        var choice = ScpDecoderCombo.SelectedItem as ScpDecoderChoice;
        var decoded = track.Revolutions.Count == 0 ? null : choice?.Id is null ? _fluxDecoders.DecodeAutomatic(track.Revolutions[0]) : _fluxDecoders.Decode(choice.Id, track.Revolutions[0]);
        var revolutions = string.Join(Environment.NewLine, track.Revolutions.Select((revolution, index) => LocExtension.Get("Visual.Revolution", index + 1, revolution.FluxIntervals.Count, revolution.DurationMilliseconds(_scpImage.Header.ResolutionNanoseconds), revolution.Rpm(_scpImage.Header.ResolutionNanoseconds))));
        var details = decoded is null ? "" : string.Join(Environment.NewLine, decoded.Structures.Take(30).Select(x => $"• {x.Description} @ bit {x.BitOffset:N0}"));
        var analysis = decoded is null ? "" : "\n\n" + LocExtension.Get("Visual.Analysis", decoded.DisplayName, decoded.Confidence, decoded.EstimatedBitCellTicks, decoded.Structures.Count) + (details.Length > 0 ? $"\n\n{details}" : "");
        ScpTrackInfo.Text = LocExtension.Get("Visual.Track", track.Head, track.Cylinder, track.TrackNumber) + $"\n\n{revolutions}{analysis}";
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
        {
            var capabilities = await new GwFormatCapabilityReader().ReadAsync(_settings.GwExecutablePath);
            _formatCatalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);
            _formatDetector = new ImageFormatDetector(_formatCatalog);
        }
        ScpDecoderCombo.ItemsSource = new[] { new ScpDecoderChoice(null, "Automatique") }.Concat(_fluxDecoders.Decoders.Select(x => new ScpDecoderChoice(x.Id, x.DisplayName))).ToArray();
        ScpDecoderCombo.SelectedIndex = 0;
        _profiles = new InMemoryProfileStore(_settings.Profiles.Select(ToProfile));
        RestoreWindowPlacement();
        ReadFolder.Text = _settings.DefaultImagesFolder;
        ReadFamilyCombo.ItemsSource = _formatCatalog.Formats.Where(x => x.Family != "Raw").Select(x => x.Family).Distinct().Order().ToArray();
        ReadFamilyCombo.SelectedIndex = 0;
        RefreshReadProfiles();
        RefreshWriteProfiles();
        RefreshConvertProfiles();
        ConvertTags.IsChecked = _settings.Conversion.AddTags;
        BuildConversionFormats(null);
        RestoreReadSettings();
        RestoreWriteSettings();
        RestoreConversionSettings();
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
        var items = _profiles.Get(OperationKind.Write);
        WriteProfileCombo.ItemsSource = items;
        WriteProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

    private void RefreshConvertProfiles(string? selectedId = null)
    {
        var items = _profiles.Get(OperationKind.Convert);
        ConvertProfileCombo.ItemsSource = items;
        ConvertProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

    private void BrowseWriteSource_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Images de disquette|*.scp;*.adf;*.st;*.msa;*.ima;*.img;*.hfe;*.d64|Tous les fichiers|*.*", InitialDirectory = ReadFolder.Text };
        if (dialog.ShowDialog(this) != true) return;
        WriteSourceText.Text = dialog.FileName;
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
        var options = new List<EnabledOption>();
        if (WriteEraseEmpty?.IsChecked == true) options.Add(new("--erase-empty"));
        if (WriteRetriesEnabled?.IsChecked == true) options.Add(new("--retries", WriteRetriesValue.Text.Trim()));
        if (WriteTracksEnabled?.IsChecked == true) options.Add(new("--tracks", WriteTracksValue.Text.Trim()));
        if (WritePreErase?.IsChecked == true) options.Add(new("--pre-erase"));
        if (WriteFakeIndexEnabled?.IsChecked == true) options.Add(new("--fake-index", WriteFakeIndexValue.Text.Trim()));
        if (WriteHardSectors?.IsChecked == true) options.Add(new("--hard-sectors"));
        if (WritePrecompEnabled?.IsChecked == true) options.Add(new("--precomp", WritePrecompValue.Text.Trim()));
        if (WriteReverse?.IsChecked == true) options.Add(new("--reverse"));
        if (WriteDenselEnabled?.IsChecked == true) options.Add(new("--densel", SelectedText(WriteDenselValue)));
        if (WriteTg43?.IsChecked == true) options.Add(new("--gen-tg43"));
        if (WriteDiskDefsEnabled?.IsChecked == true) options.Add(new("--diskdefs", WriteDiskDefsValue.Text.Trim()));
        return WriteCommandBuilder.Build(new WriteRequest(_settings.GwExecutablePath ?? "gw.exe", WriteSourceText.Text,
            (WriteFormatCombo?.SelectedItem as DiskFormat)?.Id ?? _detectedWriteFormat?.Format?.Id, options,
            WriteNoVerify?.IsChecked == true, SelectedHardware()?.Port, SelectedHardware()?.Drive.Selection, WriteExpertArguments?.Text));
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
        var warning = LocExtension.Get(WriteNoVerify.IsChecked == true ? "Write.VerifyOff" : "Write.VerifyOn");
        var confirmation = LocExtension.Get("Write.Confirm", Path.GetFileName(WriteSourceText.Text), selected.DisplayName, SelectedHardware()?.Label ?? LocExtension.Get("Hardware.NotConfigured"), warning);
        if (MessageBox.Show(confirmation, LocExtension.Get("Write.ConfirmTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) return;
        var command = BuildWriteCommand();
        _cancellation = new CancellationTokenSource(); WriteExecuteButton.Content = LocExtension.Get("Common.Stop"); LogOutput.Clear(); BeginProgress();
        var output = new Progress<GwOutputLine>(ReportOutput);
        try { var result = await _runner.RunAsync(command, output, _cancellation.Token); LogOutput.AppendText(Environment.NewLine + LocExtension.Get("Operation.Finished", result.ExitCode, result.Duration.ToString("g"))); }
        catch (Exception exception) { LogOutput.AppendText($"Erreur : {exception.Message}"); }
        finally { EndProgress(); WriteExecuteButton.Content = LocExtension.Get("Common.Execute"); _cancellation.Dispose(); _cancellation = null; }
    }

    private void WriteProfile_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (WriteProfileCombo.SelectedItem is not OperationProfile profile || WriteNoVerify is null) return;
        WriteNoVerify.IsChecked = profile.EnabledOptions.Contains("no-verify"); WriteEraseEmpty.IsChecked = profile.EnabledOptions.Contains("erase-empty"); WriteRetriesEnabled.IsChecked = profile.EnabledOptions.Contains("retries");
        WriteTracksEnabled.IsChecked = profile.EnabledOptions.Contains("tracks"); WritePreErase.IsChecked = profile.EnabledOptions.Contains("pre-erase"); WriteFakeIndexEnabled.IsChecked = profile.EnabledOptions.Contains("fake-index"); WriteHardSectors.IsChecked = profile.EnabledOptions.Contains("hard-sectors");
        WritePrecompEnabled.IsChecked = profile.EnabledOptions.Contains("precomp"); WriteReverse.IsChecked = profile.EnabledOptions.Contains("reverse"); WriteDenselEnabled.IsChecked = profile.EnabledOptions.Contains("densel"); WriteTg43.IsChecked = profile.EnabledOptions.Contains("gen-tg43");
        WriteDiskDefsEnabled.IsChecked = profile.EnabledOptions.Contains("diskdefs");
        if (profile.Values.TryGetValue("retries", out var retries)) WriteRetriesValue.Text = retries;
        if (profile.Values.TryGetValue("tracks", out var tracks)) WriteTracksValue.Text = tracks;
        if (profile.Values.TryGetValue("fake-index", out var fakeIndex)) WriteFakeIndexValue.Text = fakeIndex;
        if (profile.Values.TryGetValue("precomp", out var precomp)) WritePrecompValue.Text = precomp;
        if (profile.Values.TryGetValue("densel", out var densel)) WriteDenselValue.SelectedIndex = densel == "L" ? 1 : 0;
        if (profile.Values.TryGetValue("diskdefs", out var diskdefs)) WriteDiskDefsValue.Text = diskdefs;
        WriteExpertArguments.Text = profile.Values.GetValueOrDefault("expert", "");
        if (profile.IsSystem) WriteNoVerify.IsChecked = false;
        UpdateWriteCommand();
        UpdateProfileStatus();
    }

    private void ResetWriteProfile_Click(object sender, RoutedEventArgs e) { if (WriteProfileCombo.SelectedItem is OperationProfile profile) { WriteProfileCombo.SelectedItem = null; WriteProfileCombo.SelectedItem = profile; } }

    private void SaveWriteProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProfileNameWindow { Owner = this }; if (dialog.ShowDialog() != true) return;
        var enabled = new HashSet<string>(); if (WriteNoVerify.IsChecked == true) enabled.Add("no-verify"); if (WriteEraseEmpty.IsChecked == true) enabled.Add("erase-empty"); if (WriteRetriesEnabled.IsChecked == true) enabled.Add("retries");
        if (WriteTracksEnabled.IsChecked == true) enabled.Add("tracks"); if (WritePreErase.IsChecked == true) enabled.Add("pre-erase"); if (WriteFakeIndexEnabled.IsChecked == true) enabled.Add("fake-index"); if (WriteHardSectors.IsChecked == true) enabled.Add("hard-sectors");
        if (WritePrecompEnabled.IsChecked == true) enabled.Add("precomp"); if (WriteReverse.IsChecked == true) enabled.Add("reverse"); if (WriteDenselEnabled.IsChecked == true) enabled.Add("densel"); if (WriteTg43.IsChecked == true) enabled.Add("gen-tg43");
        if (WriteDiskDefsEnabled.IsChecked == true) enabled.Add("diskdefs");
        var values = new Dictionary<string, string> { ["retries"] = WriteRetriesValue.Text, ["tracks"] = WriteTracksValue.Text, ["fake-index"] = WriteFakeIndexValue.Text, ["precomp"] = WritePrecompValue.Text, ["densel"] = SelectedText(WriteDenselValue), ["diskdefs"] = WriteDiskDefsValue.Text, ["expert"] = WriteExpertArguments.Text };
        var profile = new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Write, dialog.ProfileName, values, enabled);
        try { profile = _profiles.Save(profile); } catch (InvalidOperationException) { if (MessageBox.Show(LocExtension.Get("Profile.Replace"), LocExtension.Get("Profile.Title"), MessageBoxButton.YesNo) != MessageBoxResult.Yes) return; profile = _profiles.Save(profile, true); }
        RefreshWriteProfiles(profile.Id);
    }

    private void BuildConversionFormats(string? sourceExtension)
    {
        if (ConvertCommonPanel is null) return;
        var selected = _conversionControls.Count == 0 ? _settings.Conversion.SelectedFormats : _conversionControls.Where(x => x.IsSelected).Select(x => x.Format.Id).ToHashSet();
        var extensions = _conversionControls.Count == 0 ? _settings.Conversion.ExplicitExtensions : _conversionControls.Where(x => x.ExplicitExtensions.Count > 0).ToDictionary(x => x.Format.Id, x => x.ExplicitExtensions.ToHashSet());
        _conversionControls.Clear(); ConvertPinnedPanel.Children.Clear(); ConvertCommonPanel.Children.Clear(); ConvertRarePanel.Children.Clear();
        var compatible = sourceExtension is null ? _formatCatalog.Formats.Select(x => x.Id).ToHashSet() : _formatCatalog.GetCompatibleOutputs(sourceExtension).Select(x => x.Id).ToHashSet();
        foreach (var format in _formatCatalog.Formats.Where(x => x.Id != "raw.scp").OrderBy(x => x.Family).ThenBy(x => x.DisplayName))
        {
            var control = new ConversionFormatControl(format) { IsEnabled = compatible.Contains(format.Id) };
            if (!control.IsEnabled) control.ToolTip = $"{format.DisplayName} n’est pas compatible avec cette source.";
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
        ConvertTracksEnabled.IsChecked = profile.EnabledOptions.Contains("tracks"); ConvertOutTracksEnabled.IsChecked = profile.EnabledOptions.Contains("out-tracks"); ConvertAdjustSpeedEnabled.IsChecked = profile.EnabledOptions.Contains("adjust-speed");
        ConvertPllEnabled.IsChecked = profile.EnabledOptions.Contains("pll"); ConvertHardSectors.IsChecked = profile.EnabledOptions.Contains("hard-sectors"); ConvertReverse.IsChecked = profile.EnabledOptions.Contains("reverse"); ConvertTags.IsChecked = profile.EnabledOptions.Contains("tags");
        ConvertDiskDefsEnabled.IsChecked = profile.EnabledOptions.Contains("diskdefs");
        if (profile.Values.TryGetValue("tracks", out var tracks)) ConvertTracksValue.Text = tracks;
        if (profile.Values.TryGetValue("out-tracks", out var outTracks)) ConvertOutTracksValue.Text = outTracks;
        if (profile.Values.TryGetValue("adjust-speed", out var speed)) ConvertAdjustSpeedValue.Text = speed;
        if (profile.Values.TryGetValue("pll", out var pll)) ConvertPllValue.Text = pll;
        if (profile.Values.TryGetValue("diskdefs", out var diskdefs)) ConvertDiskDefsValue.Text = diskdefs;
        ConvertExpertArguments.Text = profile.Values.GetValueOrDefault("expert", "");
        foreach (var control in _conversionControls.ToArray())
        {
            var selected = profile.EnabledOptions.Contains("format:" + control.Format.Id);
            var explicitExtensions = profile.Values.TryGetValue("extensions:" + control.Format.Id, out var extensions) ? extensions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : [];
            control.SetState(selected && control.IsEnabled, explicitExtensions);
        }
        UpdateConvertCommand();
    }

    private void ResetConvertProfile_Click(object sender, RoutedEventArgs e) { if (ConvertProfileCombo.SelectedItem is OperationProfile profile) ApplyConvertProfile(profile); }

    private void SaveConvertProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProfileNameWindow { Owner = this }; if (dialog.ShowDialog() != true) return;
        var enabled = new HashSet<string>();
        if (ConvertTracksEnabled.IsChecked == true) enabled.Add("tracks"); if (ConvertOutTracksEnabled.IsChecked == true) enabled.Add("out-tracks"); if (ConvertAdjustSpeedEnabled.IsChecked == true) enabled.Add("adjust-speed"); if (ConvertPllEnabled.IsChecked == true) enabled.Add("pll"); if (ConvertHardSectors.IsChecked == true) enabled.Add("hard-sectors"); if (ConvertReverse.IsChecked == true) enabled.Add("reverse"); if (ConvertTags.IsChecked == true) enabled.Add("tags");
        if (ConvertDiskDefsEnabled.IsChecked == true) enabled.Add("diskdefs");
        foreach (var control in _conversionControls.Where(control => control.IsSelected)) enabled.Add("format:" + control.Format.Id);
        var values = new Dictionary<string, string> { ["tracks"] = ConvertTracksValue.Text, ["out-tracks"] = ConvertOutTracksValue.Text, ["adjust-speed"] = ConvertAdjustSpeedValue.Text, ["pll"] = ConvertPllValue.Text, ["diskdefs"] = ConvertDiskDefsValue.Text, ["expert"] = ConvertExpertArguments.Text };
        foreach (var control in _conversionControls.Where(control => control.ExplicitExtensions.Count > 0)) values["extensions:" + control.Format.Id] = string.Join(',', control.ExplicitExtensions);
        var profile = new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Convert, dialog.ProfileName, values, enabled);
        try { profile = _profiles.Save(profile); } catch (InvalidOperationException) { if (MessageBox.Show(LocExtension.Get("Profile.Replace"), LocExtension.Get("Profile.Title"), MessageBoxButton.YesNo) != MessageBoxResult.Yes) return; profile = _profiles.Save(profile, true); }
        RefreshConvertProfiles(profile.Id);
    }

    private void ConversionSelectionChanged(object? sender, EventArgs e)
    {
        if (sender is not ConversionFormatControl control) return;
        if (control.Parent is Panel oldParent) oldParent.Children.Remove(control);
        var destination = control.IsSelected ? ConvertPinnedPanel : control.Format.IsCommon ? ConvertCommonPanel : ConvertRarePanel;
        var index = destination.Children.OfType<ConversionFormatControl>().TakeWhile(x => string.Compare(x.Format.DisplayName, control.Format.DisplayName, StringComparison.CurrentCulture) < 0).Count();
        destination.Children.Insert(index, control); UpdateConvertCommand();
    }

    private void BrowseConvertSource_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Images de disquette|*.scp;*.adf;*.st;*.msa;*.ima;*.img;*.hfe;*.d64|Tous les fichiers|*.*", InitialDirectory = ReadFolder.Text };
        if (dialog.ShowDialog(this) != true) return;
        ConvertSourceText.Text = dialog.FileName; ConvertOutputName.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
        var detection = _formatDetector.Detect(dialog.FileName, new FileInfo(dialog.FileName).Length);
        ConvertSourceInfo.Text = detection.Format?.DisplayName ?? LocExtension.Get("Conversion.SourceAmbiguous");
        BuildConversionFormats(Path.GetExtension(dialog.FileName)); UpdateConvertCommand();
    }

    private void ConvertInput_Changed(object sender, RoutedEventArgs e) => UpdateConvertCommand();

    private IReadOnlyList<ConversionOutput> PlanConversions()
    {
        if (string.IsNullOrWhiteSpace(ConvertSourceText.Text)) return [];
        return new ConversionPlanner(_formatCatalog).Plan(ConvertSourceText.Text, Path.GetDirectoryName(ConvertSourceText.Text)!, ConvertOutputName.Text.Trim(), _conversionControls.Where(x => x.IsSelected).Select(x => x.ToSelection()), ConvertTags.IsChecked == true);
    }

    private EnabledOption[] GetConvertOptions()
    {
        var options = new List<EnabledOption>();
        if (ConvertTracksEnabled.IsChecked == true) options.Add(new("--tracks", ConvertTracksValue.Text.Trim()));
        if (ConvertOutTracksEnabled.IsChecked == true) options.Add(new("--out-tracks", ConvertOutTracksValue.Text.Trim()));
        if (ConvertAdjustSpeedEnabled.IsChecked == true) options.Add(new("--adjust-speed", ConvertAdjustSpeedValue.Text.Trim()));
        if (ConvertPllEnabled.IsChecked == true) options.Add(new("--pll", ConvertPllValue.Text.Trim()));
        if (ConvertHardSectors.IsChecked == true) options.Add(new("--hard-sectors"));
        if (ConvertReverse.IsChecked == true) options.Add(new("--reverse"));
        if (ConvertDiskDefsEnabled.IsChecked == true) options.Add(new("--diskdefs", ConvertDiskDefsValue.Text.Trim()));
        return options.ToArray();
    }

    private void UpdateConvertCommand()
    {
        if (CommandPreview is null || ConvertSourceText is null || MainTabs?.SelectedIndex != 2) return;
        try
        {
            var outputs = PlanConversions();
            if (outputs.Count == 0) { CommandPreview.Text = LocExtension.Get("Conversion.SelectOutput"); return; }
            var first = ConversionCommandBuilder.Build(_settings.GwExecutablePath ?? "gw.exe", ConvertSourceText.Text, outputs[0], GetConvertOptions(), ConvertExpertArguments.Text);
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
        try { outputs = PlanConversions(); } catch (Exception exception) { MessageBox.Show(exception.Message, LocExtension.Get("Conversion.Title"), MessageBoxButton.OK, MessageBoxImage.Warning); return; }
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
        _cancellation = new CancellationTokenSource(); ConvertExecuteButton.Content = LocExtension.Get("Common.Stop"); LogOutput.Clear(); BeginProgress(); var failures = new List<string>();
        var progress = new Progress<GwOutputLine>(ReportOutput);
        try
        {
            foreach (var planned in outputs)
            {
                if (_cancellation.IsCancellationRequested) break;
                BeginProgress();
                LogOutput.AppendText($"{Environment.NewLine}→ {Path.GetFileName(planned.OutputPath)}{Environment.NewLine}");
                var result = await _runner.RunAsync(ConversionCommandBuilder.Build(_settings.GwExecutablePath, ConvertSourceText.Text, planned, GetConvertOptions(), ConvertExpertArguments.Text), progress, _cancellation.Token);
                if (!result.IsSuccess) failures.Add(Path.GetFileName(planned.OutputPath));
            }
            LogOutput.AppendText(Environment.NewLine + LocExtension.Get("Conversion.Summary", outputs.Count - failures.Count, failures.Count) + (failures.Count > 0 ? LocExtension.Get("Conversion.Failures", string.Join(", ", failures)) : ""));
        }
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
        _settings.Conversion.AddTags = ConvertTags.IsChecked == true;
        _settings.Conversion.SelectedFormats = _conversionControls.Where(x => x.IsSelected).Select(x => x.Format.Id).ToHashSet();
        _settings.Conversion.ExplicitExtensions = _conversionControls.Where(x => x.ExplicitExtensions.Count > 0).ToDictionary(x => x.Format.Id, x => x.ExplicitExtensions.ToHashSet());
        _settings.Conversion.EnabledOptions = [];
        if (ConvertTracksEnabled.IsChecked == true) _settings.Conversion.EnabledOptions.Add("tracks"); if (ConvertOutTracksEnabled.IsChecked == true) _settings.Conversion.EnabledOptions.Add("out-tracks"); if (ConvertAdjustSpeedEnabled.IsChecked == true) _settings.Conversion.EnabledOptions.Add("adjust-speed"); if (ConvertPllEnabled.IsChecked == true) _settings.Conversion.EnabledOptions.Add("pll"); if (ConvertHardSectors.IsChecked == true) _settings.Conversion.EnabledOptions.Add("hard-sectors"); if (ConvertReverse.IsChecked == true) _settings.Conversion.EnabledOptions.Add("reverse"); if (ConvertDiskDefsEnabled.IsChecked == true) _settings.Conversion.EnabledOptions.Add("diskdefs");
        _settings.Conversion.OptionValues["tracks"] = ConvertTracksValue.Text; _settings.Conversion.OptionValues["out-tracks"] = ConvertOutTracksValue.Text; _settings.Conversion.OptionValues["adjust-speed"] = ConvertAdjustSpeedValue.Text; _settings.Conversion.OptionValues["pll"] = ConvertPllValue.Text; _settings.Conversion.OptionValues["diskdefs"] = ConvertDiskDefsValue.Text; _settings.Conversion.OptionValues["expert"] = ConvertExpertArguments.Text;
    }

    private void RestoreConversionSettings()
    {
        ConvertTracksEnabled.IsChecked = _settings.Conversion.EnabledOptions.Contains("tracks"); ConvertOutTracksEnabled.IsChecked = _settings.Conversion.EnabledOptions.Contains("out-tracks"); ConvertAdjustSpeedEnabled.IsChecked = _settings.Conversion.EnabledOptions.Contains("adjust-speed"); ConvertPllEnabled.IsChecked = _settings.Conversion.EnabledOptions.Contains("pll"); ConvertHardSectors.IsChecked = _settings.Conversion.EnabledOptions.Contains("hard-sectors"); ConvertReverse.IsChecked = _settings.Conversion.EnabledOptions.Contains("reverse"); ConvertDiskDefsEnabled.IsChecked = _settings.Conversion.EnabledOptions.Contains("diskdefs");
        if (_settings.Conversion.OptionValues.TryGetValue("tracks", out var tracks)) ConvertTracksValue.Text = tracks;
        if (_settings.Conversion.OptionValues.TryGetValue("out-tracks", out var outTracks)) ConvertOutTracksValue.Text = outTracks;
        if (_settings.Conversion.OptionValues.TryGetValue("adjust-speed", out var speed)) ConvertAdjustSpeedValue.Text = speed;
        if (_settings.Conversion.OptionValues.TryGetValue("pll", out var pll)) ConvertPllValue.Text = pll;
        if (_settings.Conversion.OptionValues.TryGetValue("diskdefs", out var diskdefs)) ConvertDiskDefsValue.Text = diskdefs;
        ConvertExpertArguments.Text = _settings.Conversion.OptionValues.GetValueOrDefault("expert", "");
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
        var items = _profiles.Get(OperationKind.Read);
        ReadProfileCombo.ItemsSource = items;
        ReadProfileCombo.SelectedItem = items.FirstOrDefault(x => x.Id == selectedId) ?? items[0];
    }

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
        ReadRevsEnabled.IsChecked = profile.EnabledOptions.Contains("revs");
        ReadRetriesEnabled.IsChecked = profile.EnabledOptions.Contains("retries");
        ReadTracksEnabled.IsChecked = profile.EnabledOptions.Contains("tracks");
        ReadSeekRetriesEnabled.IsChecked = profile.EnabledOptions.Contains("seek-retries"); ReadFakeIndexEnabled.IsChecked = profile.EnabledOptions.Contains("fake-index"); ReadHardSectors.IsChecked = profile.EnabledOptions.Contains("hard-sectors");
        ReadAdjustSpeedEnabled.IsChecked = profile.EnabledOptions.Contains("adjust-speed"); ReadPllEnabled.IsChecked = profile.EnabledOptions.Contains("pll"); ReadReverse.IsChecked = profile.EnabledOptions.Contains("reverse"); ReadDenselEnabled.IsChecked = profile.EnabledOptions.Contains("densel"); ReadTg43.IsChecked = profile.EnabledOptions.Contains("gen-tg43");
        ReadDiskDefsEnabled.IsChecked = profile.EnabledOptions.Contains("diskdefs");
        if (profile.Values.TryGetValue("revs", out var revs)) ReadRevsValue.Text = revs;
        if (profile.Values.TryGetValue("retries", out var retries)) ReadRetriesValue.Text = retries;
        if (profile.Values.TryGetValue("tracks", out var tracks)) ReadTracksValue.Text = tracks;
        if (profile.Values.TryGetValue("seek-retries", out var seekRetries)) ReadSeekRetriesValue.Text = seekRetries;
        if (profile.Values.TryGetValue("fake-index", out var fakeIndex)) ReadFakeIndexValue.Text = fakeIndex;
        if (profile.Values.TryGetValue("adjust-speed", out var speed)) ReadAdjustSpeedValue.Text = speed;
        if (profile.Values.TryGetValue("pll", out var pll)) ReadPllValue.Text = pll;
        if (profile.Values.TryGetValue("densel", out var densel)) ReadDenselValue.SelectedIndex = densel == "L" ? 1 : 0;
        if (profile.Values.TryGetValue("diskdefs", out var diskdefs)) ReadDiskDefsValue.Text = diskdefs;
        ReadExpertArguments.Text = profile.Values.GetValueOrDefault("expert", "");
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
        var enabled = new HashSet<string>();
        if (ReadRevsEnabled.IsChecked == true) enabled.Add("revs");
        if (ReadRetriesEnabled.IsChecked == true) enabled.Add("retries");
        if (ReadTracksEnabled.IsChecked == true) enabled.Add("tracks");
        if (ReadSeekRetriesEnabled.IsChecked == true) enabled.Add("seek-retries"); if (ReadFakeIndexEnabled.IsChecked == true) enabled.Add("fake-index"); if (ReadHardSectors.IsChecked == true) enabled.Add("hard-sectors");
        if (ReadAdjustSpeedEnabled.IsChecked == true) enabled.Add("adjust-speed"); if (ReadPllEnabled.IsChecked == true) enabled.Add("pll"); if (ReadReverse.IsChecked == true) enabled.Add("reverse"); if (ReadDenselEnabled.IsChecked == true) enabled.Add("densel"); if (ReadTg43.IsChecked == true) enabled.Add("gen-tg43");
        if (ReadDiskDefsEnabled.IsChecked == true) enabled.Add("diskdefs");
        var values = new Dictionary<string, string> { ["revs"] = ReadRevsValue.Text, ["retries"] = ReadRetriesValue.Text, ["tracks"] = ReadTracksValue.Text, ["seek-retries"] = ReadSeekRetriesValue.Text, ["fake-index"] = ReadFakeIndexValue.Text, ["adjust-speed"] = ReadAdjustSpeedValue.Text, ["pll"] = ReadPllValue.Text, ["densel"] = SelectedText(ReadDenselValue), ["diskdefs"] = ReadDiskDefsValue.Text, ["expert"] = ReadExpertArguments.Text };
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
        if (ReadSeekRetriesEnabled?.IsChecked == true) options.Add(new("--seek-retries", ReadSeekRetriesValue.Text.Trim()));
        if (ReadFakeIndexEnabled?.IsChecked == true) options.Add(new("--fake-index", ReadFakeIndexValue.Text.Trim()));
        if (ReadHardSectors?.IsChecked == true) options.Add(new("--hard-sectors"));
        if (ReadAdjustSpeedEnabled?.IsChecked == true) options.Add(new("--adjust-speed", ReadAdjustSpeedValue.Text.Trim()));
        if (ReadPllEnabled?.IsChecked == true) options.Add(new("--pll", ReadPllValue.Text.Trim()));
        if (ReadReverse?.IsChecked == true) options.Add(new("--reverse"));
        if (ReadDenselEnabled?.IsChecked == true) options.Add(new("--densel", SelectedText(ReadDenselValue)));
        if (ReadTg43?.IsChecked == true) options.Add(new("--gen-tg43"));
        if (ReadDiskDefsEnabled?.IsChecked == true) options.Add(new("--diskdefs", ReadDiskDefsValue.Text.Trim()));
        return ReadCommandBuilder.Build(new ReadRequest(
            _settings.GwExecutablePath ?? "gw.exe", target,
            RawScpRadio?.IsChecked == true ? ReadResultKind.RawScp : ReadResultKind.KnownFormat,
            (ReadFormatCombo?.SelectedItem as DiskFormat)?.Id, options,
            SelectedHardware()?.Port, SelectedHardware()?.Drive.Selection, ReadExpertArguments?.Text));
    }

    private static string SelectedText(ComboBox combo) => (combo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;

    private void ReadFakeIndex_Checked(object sender, RoutedEventArgs e) { if (ReadHardSectors is not null) ReadHardSectors.IsChecked = false; ReadInput_Changed(sender, e); }
    private void ReadHardSectors_Checked(object sender, RoutedEventArgs e) { if (ReadFakeIndexEnabled is not null) ReadFakeIndexEnabled.IsChecked = false; ReadInput_Changed(sender, e); }
    private void ReadDensel_Checked(object sender, RoutedEventArgs e) { if (ReadTg43 is not null) ReadTg43.IsChecked = false; ReadInput_Changed(sender, e); }
    private void ReadTg43_Checked(object sender, RoutedEventArgs e) { if (ReadDenselEnabled is not null) ReadDenselEnabled.IsChecked = false; ReadInput_Changed(sender, e); }
    private void WriteFakeIndex_Checked(object sender, RoutedEventArgs e) { if (WriteHardSectors is not null) WriteHardSectors.IsChecked = false; WriteInput_Changed(sender, e); }
    private void WriteHardSectors_Checked(object sender, RoutedEventArgs e) { if (WriteFakeIndexEnabled is not null) WriteFakeIndexEnabled.IsChecked = false; WriteInput_Changed(sender, e); }
    private void WriteDensel_Checked(object sender, RoutedEventArgs e) { if (WriteTg43 is not null) WriteTg43.IsChecked = false; WriteInput_Changed(sender, e); }
    private void WriteTg43_Checked(object sender, RoutedEventArgs e) { if (WriteDenselEnabled is not null) WriteDenselEnabled.IsChecked = false; WriteInput_Changed(sender, e); }

    private void BrowseReadDiskDefs_Click(object sender, RoutedEventArgs e) => BrowseDiskDefs(ReadDiskDefsValue, ReadDiskDefsEnabled, UpdateReadCommand);
    private void BrowseWriteDiskDefs_Click(object sender, RoutedEventArgs e) => BrowseDiskDefs(WriteDiskDefsValue, WriteDiskDefsEnabled, UpdateWriteCommand);
    private void BrowseConvertDiskDefs_Click(object sender, RoutedEventArgs e) => BrowseDiskDefs(ConvertDiskDefsValue, ConvertDiskDefsEnabled, UpdateConvertCommand);

    private void BrowseDiskDefs(TextBox target, CheckBox enabled, Action refresh)
    {
        var dialog = new OpenFileDialog { Filter = LocExtension.Get("Advanced.DiskDefsFilter"), FileName = target.Text };
        if (dialog.ShowDialog(this) != true) return;
        target.Text = dialog.FileName; enabled.IsChecked = true; refresh();
    }

    private bool ValidateDiskDefs(CheckBox enabled, TextBox path, string title)
    {
        if (enabled.IsChecked != true || File.Exists(path.Text)) return true;
        MessageBox.Show(LocExtension.Get("Advanced.DiskDefsMissing"), title, MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
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
                if (!long.TryParse(ReadSequenceValue.Text, out var next)) next = 1;
                var kind = ReadSequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
                var available = OutputConflictResolver.FindNextAvailableWithValue(ReadFolder.Text, ReadFileName.Text.Trim(), extension, kind, ReadSequenceWidth.SelectedIndex + 1, next);
                target = available.Path;
                ReadSequenceValue.Text = available.Value.ToString();
            }
        }
        var command = BuildReadCommand(target);
        _cancellation = new CancellationTokenSource();
        ReadExecuteButton.Content = LocExtension.Get("Common.Stop");
        LogOutput.Clear();
        BeginProgress();
        var output = new Progress<GwOutputLine>(ReportOutput);
        try
        {
            var result = await _runner.RunAsync(command, output, _cancellation.Token);
            LogOutput.AppendText(Environment.NewLine + LocExtension.Get("Operation.Finished", result.ExitCode, result.Duration.ToString("g")));
            if (result.IsSuccess && extension.Equals(".scp", StringComparison.OrdinalIgnoreCase)) { _lastScpPath = target; OpenScpBanner.Visibility = Visibility.Visible; }
            if (result.IsSuccess && ReadAutoNumber.IsChecked == true && long.TryParse(ReadSequenceValue.Text, out var value)) ReadSequenceValue.Text = (value + 1).ToString();
        }
        catch (Exception exception) { LogOutput.AppendText($"Erreur : {exception.Message}"); }
        finally { EndProgress(); ReadExecuteButton.Content = LocExtension.Get("Common.Execute"); _cancellation.Dispose(); _cancellation = null; }
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
        ReadSeekRetriesEnabled.IsChecked = _settings.Read.EnabledOptions.Contains("seek-retries"); ReadFakeIndexEnabled.IsChecked = _settings.Read.EnabledOptions.Contains("fake-index"); ReadHardSectors.IsChecked = _settings.Read.EnabledOptions.Contains("hard-sectors");
        ReadAdjustSpeedEnabled.IsChecked = _settings.Read.EnabledOptions.Contains("adjust-speed"); ReadPllEnabled.IsChecked = _settings.Read.EnabledOptions.Contains("pll"); ReadReverse.IsChecked = _settings.Read.EnabledOptions.Contains("reverse"); ReadDenselEnabled.IsChecked = _settings.Read.EnabledOptions.Contains("densel"); ReadTg43.IsChecked = _settings.Read.EnabledOptions.Contains("gen-tg43"); ReadDiskDefsEnabled.IsChecked = _settings.Read.EnabledOptions.Contains("diskdefs");
        if (_settings.Read.OptionValues.TryGetValue("revs", out var revs)) ReadRevsValue.Text = revs;
        if (_settings.Read.OptionValues.TryGetValue("retries", out var retries)) ReadRetriesValue.Text = retries;
        if (_settings.Read.OptionValues.TryGetValue("tracks", out var tracks)) ReadTracksValue.Text = tracks;
        if (_settings.Read.OptionValues.TryGetValue("seek-retries", out var seekRetries)) ReadSeekRetriesValue.Text = seekRetries;
        if (_settings.Read.OptionValues.TryGetValue("fake-index", out var fakeIndex)) ReadFakeIndexValue.Text = fakeIndex;
        if (_settings.Read.OptionValues.TryGetValue("adjust-speed", out var speed)) ReadAdjustSpeedValue.Text = speed;
        if (_settings.Read.OptionValues.TryGetValue("pll", out var pll)) ReadPllValue.Text = pll;
        if (_settings.Read.OptionValues.TryGetValue("densel", out var densel)) ReadDenselValue.SelectedIndex = densel == "L" ? 1 : 0;
        if (_settings.Read.OptionValues.TryGetValue("diskdefs", out var diskdefs)) ReadDiskDefsValue.Text = diskdefs;
        ReadExpertArguments.Text = _settings.Read.OptionValues.GetValueOrDefault("expert", "");
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
        if (ReadSeekRetriesEnabled.IsChecked == true) _settings.Read.EnabledOptions.Add("seek-retries"); if (ReadFakeIndexEnabled.IsChecked == true) _settings.Read.EnabledOptions.Add("fake-index"); if (ReadHardSectors.IsChecked == true) _settings.Read.EnabledOptions.Add("hard-sectors");
        if (ReadAdjustSpeedEnabled.IsChecked == true) _settings.Read.EnabledOptions.Add("adjust-speed"); if (ReadPllEnabled.IsChecked == true) _settings.Read.EnabledOptions.Add("pll"); if (ReadReverse.IsChecked == true) _settings.Read.EnabledOptions.Add("reverse"); if (ReadDenselEnabled.IsChecked == true) _settings.Read.EnabledOptions.Add("densel"); if (ReadTg43.IsChecked == true) _settings.Read.EnabledOptions.Add("gen-tg43"); if (ReadDiskDefsEnabled.IsChecked == true) _settings.Read.EnabledOptions.Add("diskdefs");
        _settings.Read.OptionValues["revs"] = ReadRevsValue.Text;
        _settings.Read.OptionValues["retries"] = ReadRetriesValue.Text;
        _settings.Read.OptionValues["tracks"] = ReadTracksValue.Text;
        _settings.Read.OptionValues["seek-retries"] = ReadSeekRetriesValue.Text; _settings.Read.OptionValues["fake-index"] = ReadFakeIndexValue.Text; _settings.Read.OptionValues["adjust-speed"] = ReadAdjustSpeedValue.Text; _settings.Read.OptionValues["pll"] = ReadPllValue.Text; _settings.Read.OptionValues["densel"] = SelectedText(ReadDenselValue); _settings.Read.OptionValues["diskdefs"] = ReadDiskDefsValue.Text; _settings.Read.OptionValues["expert"] = ReadExpertArguments.Text;
    }

    private void RestoreWriteSettings()
    {
        var enabled = _settings.Write.EnabledOptions;
        WriteNoVerify.IsChecked = enabled.Contains("no-verify"); WriteEraseEmpty.IsChecked = enabled.Contains("erase-empty"); WriteRetriesEnabled.IsChecked = enabled.Contains("retries"); WriteTracksEnabled.IsChecked = enabled.Contains("tracks"); WritePreErase.IsChecked = enabled.Contains("pre-erase"); WriteFakeIndexEnabled.IsChecked = enabled.Contains("fake-index"); WriteHardSectors.IsChecked = enabled.Contains("hard-sectors"); WritePrecompEnabled.IsChecked = enabled.Contains("precomp"); WriteReverse.IsChecked = enabled.Contains("reverse"); WriteDenselEnabled.IsChecked = enabled.Contains("densel"); WriteTg43.IsChecked = enabled.Contains("gen-tg43"); WriteDiskDefsEnabled.IsChecked = enabled.Contains("diskdefs");
        var values = _settings.Write.OptionValues;
        if (values.TryGetValue("retries", out var retries)) WriteRetriesValue.Text = retries; if (values.TryGetValue("tracks", out var tracks)) WriteTracksValue.Text = tracks; if (values.TryGetValue("fake-index", out var fakeIndex)) WriteFakeIndexValue.Text = fakeIndex; if (values.TryGetValue("precomp", out var precomp)) WritePrecompValue.Text = precomp; if (values.TryGetValue("densel", out var densel)) WriteDenselValue.SelectedIndex = densel == "L" ? 1 : 0; if (values.TryGetValue("diskdefs", out var diskdefs)) WriteDiskDefsValue.Text = diskdefs; WriteExpertArguments.Text = values.GetValueOrDefault("expert", "");
    }

    private void CaptureWriteSettings()
    {
        var enabled = _settings.Write.EnabledOptions = [];
        if (WriteNoVerify.IsChecked == true) enabled.Add("no-verify"); if (WriteEraseEmpty.IsChecked == true) enabled.Add("erase-empty"); if (WriteRetriesEnabled.IsChecked == true) enabled.Add("retries"); if (WriteTracksEnabled.IsChecked == true) enabled.Add("tracks"); if (WritePreErase.IsChecked == true) enabled.Add("pre-erase"); if (WriteFakeIndexEnabled.IsChecked == true) enabled.Add("fake-index"); if (WriteHardSectors.IsChecked == true) enabled.Add("hard-sectors"); if (WritePrecompEnabled.IsChecked == true) enabled.Add("precomp"); if (WriteReverse.IsChecked == true) enabled.Add("reverse"); if (WriteDenselEnabled.IsChecked == true) enabled.Add("densel"); if (WriteTg43.IsChecked == true) enabled.Add("gen-tg43"); if (WriteDiskDefsEnabled.IsChecked == true) enabled.Add("diskdefs");
        var values = _settings.Write.OptionValues;
        values["retries"] = WriteRetriesValue.Text; values["tracks"] = WriteTracksValue.Text; values["fake-index"] = WriteFakeIndexValue.Text; values["precomp"] = WritePrecompValue.Text; values["densel"] = SelectedText(WriteDenselValue); values["diskdefs"] = WriteDiskDefsValue.Text; values["expert"] = WriteExpertArguments.Text;
    }

    private async void Preferences_Click(object sender, RoutedEventArgs e)
    {
        CaptureProfiles();
        var dialog = new OptionsWindow(_settings) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _profiles = new InMemoryProfileStore(_settings.Profiles.Select(ToProfile));
            RefreshReadProfiles(); RefreshWriteProfiles(); RefreshConvertProfiles();
            ReadFolder.Text = _settings.DefaultImagesFolder;
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
        return MaintenanceCommandBuilder.Erase(new EraseRequest(_settings.GwExecutablePath ?? "gw.exe", options, hardware?.Port, hardware?.Drive.Selection, EraseExpertArguments.Text));
    }

    private GwCommand BuildCleanCommand() => MaintenanceCommandBuilder.Clean(new CleanRequest(_settings.GwExecutablePath ?? "gw.exe",
        CleanCylindersEnabled.IsChecked == true && int.TryParse(CleanCylindersValue.Text, out var cylinders) ? cylinders : null,
        CleanPassesEnabled.IsChecked == true && int.TryParse(CleanPassesValue.Text, out var passes) ? passes : null,
        CleanLingerEnabled.IsChecked == true && int.TryParse(CleanLingerValue.Text, out var linger) ? linger : null,
        SelectedHardware()?.Port, SelectedHardware()?.Drive.Selection, CleanExpertArguments.Text));

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
    private void HardwareSelector_Changed(object sender, SelectionChangedEventArgs e) { UpdateHardwareStatus(); UpdateReadCommand(); UpdateWriteCommand(); UpdateToolCommand(); }
    private void UpdateHardwareStatus()
    {
        if (HardwareStatusText is null) return;
        var selected = SelectedHardware();
        HardwareStatusText.Text = selected is null ? LocExtension.Get("Hardware.NotConfigured") : selected.Label;
        HardwareStatusLight.Fill = new SolidColorBrush(selected?.Available == true ? Color.FromRgb(63, 171, 91) : Color.FromRgb(136, 136, 136));
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
        try { var result = await _runner.RunAsync(command, progress, _cancellation.Token); LogOutput.AppendText($"{Environment.NewLine}Fin : code {result.ExitCode}."); }
        catch (Exception exception) { LogOutput.AppendText($"Erreur : {exception.Message}"); }
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
        ProgressStatusItem.Visibility = Visibility.Visible;
        OperationProgress.IsIndeterminate = true;
        OperationProgress.Value = 0;
        OperationProgressText.Text = "";
    }

    private void ReportOutput(GwOutputLine line)
    {
        LogOutput.AppendText(line.Text + Environment.NewLine);
        LogOutput.ScrollToEnd();
        var progress = _progressTracker.Accept(line.Text);
        if (progress is null) return;
        if (progress.TotalTracks is int total)
        {
            OperationProgress.IsIndeterminate = false;
            OperationProgress.Value = progress.Fraction.GetValueOrDefault() * 100;
            OperationProgressText.Text = LocExtension.Get("Status.TrackProgress", progress.Cylinder, progress.Head, progress.CompletedTracks, total);
        }
        else OperationProgressText.Text = LocExtension.Get("Status.TrackUnknown", progress.Cylinder, progress.Head, progress.CompletedTracks);
    }

    private void EndProgress()
    {
        OperationProgress.IsIndeterminate = false;
        OperationProgress.Value = 100;
        ProgressStatusItem.Visibility = Visibility.Collapsed;
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
        ProfileStatusItem.Visibility = name is null ? Visibility.Collapsed : Visibility.Visible;
        if (name is not null) ProfileStatusText.Text = LocExtension.Get("Status.Profile", name);
    }

    private async Task CheckHostToolsUpdateAsync()
    {
        if (_settings.LastHostToolsCheckUtc is DateTimeOffset checkedAt && DateTimeOffset.UtcNow - checkedAt < TimeSpan.FromDays(1))
        {
            ShowHostToolsUpdateIfNeeded(); return;
        }
        try
        {
            var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GW GUI", "host-tools");
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
        HostToolsUpdateItem.Visibility = newer ? Visibility.Visible : Visibility.Collapsed;
        if (newer) HostToolsUpdateButton.Content = LocExtension.Get("HostTools.UpdateAvailable", available!);
    }

    private void ToolCommand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string verb }) return;
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            MessageBox.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var hardware = SelectedHardware();
        new GwToolWindow(_settings.GwExecutablePath, verb, hardware?.Port, hardware?.Drive.Selection) { Owner = this }.ShowDialog();
    }
}

public sealed record HardwareChoice(DriveSettings Drive, string Port, bool Available, string Label);
