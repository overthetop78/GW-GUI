using GWGUI.Infrastructure.Commands.Building;
using GWGUI.Infrastructure.Commands.Execution;
using GWGUI.MediaEngine.Formats;
using GWGUI.MediaEngine.Formats.Detection;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.HostTools;
using GWGUI.App.Profiles;
using GWGUI.Infrastructure.Settings;
using GWGUI.App.Contracts.Services.Hardware;
using GWGUI.App.Contracts.Dialogs;
using GWGUI.App.Contracts.Visualization;
using GWGUI.App.Controllers.MainWindow;
using GWGUI.App.Dictionaries.Options;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Navigation;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Presenters.Conversion;
using GWGUI.App.Services.Dialogs;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Documentation;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Services.Hardware;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Maintenance;
using GWGUI.App.Services.Operations;
using GWGUI.App.Services.Profiles;
using GWGUI.App.Services.Storage;
using GWGUI.App.Services.Terminal;
using GWGUI.App.Services.Visualization;
using GWGUI.App.Services.Windows;
using GWGUI.App.Services.Updates;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Common;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.App.Views.Controls.Options;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.App.Views.Controls.Write;
using GWGUI.App.Views.Dialogs.Common;
using GWGUI.MediaEngine.Visualization;
using System.ComponentModel;
using GWGUI.MediaEngine.Exploration.Results;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Processes;
using GWGUI.App.Constants.Views.Shell;
namespace GWGUI.App.Views.Windows.Shell;

public partial class MainWindow : Window
{
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _lifecycle.LoadAsync();
        await ((App)Application.Current).CompleteUpdateStartupAsync(this);
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabs?.SelectedIndex == MainWindowConstants.WriteTabIndex) UpdateWriteCommand();
        else if (MainTabs?.SelectedIndex == MainWindowConstants.ReadTabIndex) UpdateReadCommand();
        else if (MainTabs?.SelectedIndex == MainWindowConstants.ConvertTabIndex) UpdateConvertCommand();
        else if (MainTabs?.SelectedIndex == MainWindowConstants.ToolsTabIndex) UpdateToolCommand();
        UpdateProfileStatus();
    }

    private void RefreshWriteProfiles(string? selectedId = null)
        => _writeTab.RefreshProfiles(selectedId);

    private void RefreshConvertProfiles(string? selectedId = null)
        => _conversionTab.RefreshProfiles(selectedId);

    private void WriteInput_Changed(object sender, RoutedEventArgs e) => _writeTab.UpdateCommand();

    private void UpdateWriteCommand() => _writeTab.UpdateCommand();

    private async void ExecuteWrite_Click(object sender, RoutedEventArgs e)
        => await _writeTab.ExecuteAsync();

    private void BuildConversionFormats(string? sourceExtension, DetectedImageFormat? detection = null)
        => _conversionTab.BuildFormats(sourceExtension, detection);

    private void UpdateConvertCommand() => _conversionTab.UpdateCommand();

    private async void ExecuteConvert_Click(object sender, RoutedEventArgs e)
        => await _conversionTab.ExecuteAsync();

    private void CaptureConversionSettings() => _conversionTab.CaptureSettings();

    private void RestoreConversionSettings() => _conversionTab.RestoreSettings();

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        _lifecycle.Closing(e);
        if (!e.Cancel) _scpInspectorController.Dispose();
    }

    private void RefreshReadProfiles(string? selectedId = null)
        => _readTab.RefreshProfiles(selectedId);

    private void CaptureProfiles()
    {
        _settings.Profiles = _profiles.Capture();
    }

    private void LoadProfileStores()
    {
        _profiles.Reset(_settings.Profiles);
    }

    private void Documentation_Click(object sender, RoutedEventArgs e)
    {
        var language = System.Globalization.CultureInfo.CurrentUICulture.Name;
        var url = UserGuideLocator.GetUrl(language);
        try { _openDocumentation(url); }
        catch (Exception exception)
        {
            ShowLoggedError(exception, LocExtension.Get("Menu.Documentation"), "App.Title");
        }
    }

    private void ReadInput_Changed(object sender, RoutedEventArgs e) => _readTab.InputChanged();

    private void UpdateReadExtension() => _readTab.UpdateExtension();

    private void UpdateReadCommand() => _readTab.UpdateCommand();

    private static string SelectedText(ComboBox combo) => (combo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;


    private void RebuildFormatCatalog()
    {
        _formatWorkspace.SetCapabilities(_gwCapabilities);
        SynchronizeFormatWorkspace();
    }

    private void SynchronizeFormatWorkspace()
    {
        _gwCapabilities = _formatWorkspace.Capabilities;
        _formatCatalog = _formatWorkspace.Catalog;
        _formatDetector = _formatWorkspace.Detector;
    }

    private void RefreshFormatSelectors()
    {
        var selectedReadId = (ReadFormatCombo.SelectedItem as DiskFormat)?.Id;
        var selectedFamily = (ReadFormatCombo.SelectedItem as DiskFormat)?.Family ?? ReadFamilyCombo.SelectedItem as string;
        var readableFormats = _formatCatalog.Formats.Where(format =>
            format.Family != MainWindowConstants.RawFormatFamily
            && format.SupportsPhysicalRead).ToArray();
        var families = readableFormats.Select(format => format.Family).Distinct().Order().ToArray();
        ReadFamilyCombo.ItemsSource = families;
        ReadFamilyCombo.SelectedItem = selectedFamily is not null && families.Contains(selectedFamily) ? selectedFamily : families.FirstOrDefault();
        if (selectedReadId is not null)
            ReadFormatCombo.SelectedItem = readableFormats.FirstOrDefault(format => format.Id == selectedReadId);

        if (WriteFormatCombo.ItemsSource is not null)
        {
            var selectedWriteId = (WriteFormatCombo.SelectedItem as DiskFormat)?.Id;
        var writableFormats = _formatCatalog.Formats.Where(format =>
            format.Family != MainWindowConstants.RawFormatFamily
            && format.SupportsPhysicalWrite).ToArray();
            WriteFormatCombo.ItemsSource = writableFormats;
            WriteFormatCombo.SelectedItem = writableFormats.FirstOrDefault(format => format.Id == selectedWriteId);
        }

        DetectedImageFormat? detection = null;
        var sourceExtension = string.IsNullOrWhiteSpace(ConvertSourceText.Text) ? null : Path.GetExtension(ConvertSourceText.Text);
        if (File.Exists(ConvertSourceText.Text)) detection = _formatDetector.Detect(ConvertSourceText.Text, new FileInfo(ConvertSourceText.Text).Length);
        BuildConversionFormats(sourceExtension, detection);
    }

    private async void ExecuteRead_Click(object sender, RoutedEventArgs e)
        => await _readTab.ExecuteAsync();

    private void AppendAnalysisFailure(Exception exception, string context)
    {
        _writeError(exception, context);
        var detail = ExceptionDescriptionFunctions.Describe(exception);
        _operation.AppendText(Environment.NewLine);
        _operation.AppendText(LocExtension.Get("Error.Unexpected", detail));
        _operation.AppendText(Environment.NewLine);
    }

    private void ShowAdvancedValidation(Exception exception, string title)
    {
        _diskDefinitionsController.ShowInvalid(exception, title);
    }

    private void BrowseReadFolder_Click(object sender, RoutedEventArgs e) => _readTab.BrowseFolder();

    private void SaveReadProfile_Click(object sender, RoutedEventArgs e) => _readTab.SaveProfile();

    private void SaveWriteProfile_Click(object sender, RoutedEventArgs e) => _writeTab.SaveProfile();

    private void LogHistory_Click(object sender, RoutedEventArgs e) => _navigation.ShowLogHistory(_logsDirectory);

    private void About_Click(object sender, RoutedEventArgs e) => _navigation.ShowAbout();

    private void RestoreReadSettings() => _readTab.RestoreSettings();

    private void CaptureReadSettings() => _readTab.CaptureSettings();

    private void RestoreWriteSettings()
        => _writeTab.RestoreSettings();

    private void CaptureWriteSettings()
        => _writeTab.CaptureSettings();

    private async void Preferences_Click(object sender, RoutedEventArgs e)
        => await _lifecycle.ShowPreferencesAsync();

    internal void RefreshLocalizedContent()
    {
        var readProfile = (ReadProfileCombo.SelectedItem as OperationProfile)?.Id;
        var writeProfile = (WriteProfileCombo.SelectedItem as OperationProfile)?.Id;
        var convertProfile = (ConvertProfileCombo.SelectedItem as OperationProfile)?.Id;
        RebuildFormatCatalog();
        RefreshReadProfiles(readProfile);
        RefreshWriteProfiles(writeProfile);
        RefreshConvertProfiles(convertProfile);
        RefreshExplorerFormats();
        RefreshHardwareSelector();
        UpdateReadExtension();
        UpdateProfileStatus();
        UpdateReadCommand();
        UpdateWriteCommand();
        UpdateConvertCommand();
        UpdateToolCommand();
        if (!_operation.IsRunning) _operation.SetState("Status.ReadyShort", Color.FromRgb(136, 136, 136));
        ShowHostToolsUpdateIfNeeded();
        ApplicationMenu.SetEmulationModules(EmulationModuleRegistry.Modules);
        EmulationBlock.RefreshLocalizedContent();
    }

    private void RefreshExplorerFormats()
    {
        var selectedId = DiskExplorer.SelectedFormatId;
        DiskExplorer.SetFormats(_formatCatalog.Formats, selectedId);
        VisualizerHeader.SetFormats(_formatCatalog.Formats);
    }

    private void ShowLoggedError(Exception exception, string context, string titleKey, string messageKey = "Error.Unexpected")
    {
        _writeError(exception, context);
        if (messageKey == "Explorer.SelectedFormatUnsupported")
        {
            var unsupported = exception as DiskImageWorkspaceController.SelectedFormatUnsupportedException;
            var formatName = unsupported?.FormatName ?? LocExtension.Get("Explorer.Unknown");
            var fileName = unsupported?.FileName ?? LocExtension.Get("Explorer.Unknown");
            CommonErrorDialog.Show(this, new CommonErrorDialogContent(
                LocExtension.Get(titleKey),
                LocExtension.Get(messageKey, formatName, fileName),
                CommonErrorDialog.InformationIcon,
                (Brush)FindResource("AccentBrush")));
            return;
        }
        var detail = ExceptionDescriptionFunctions.Describe(exception);
        _dialogs.Show(LocExtension.Get(messageKey, detail), LocExtension.Get(titleKey), icon: UserDialogIcon.Error);
    }

    private void CaptureWindowSettings()
    {
        _windowPlacement.Capture(
            this,
            _settings,
            _terminalPanel.IsVisible,
            _terminalPanel.ActualHeight);
    }

    private void RestoreWindowPlacement() => _windowPlacement.Restore(this, _settings.Window);

    private void ConstrainToCurrentWorkArea() => _windowPlacement.ConstrainToCurrentWorkArea(this);

    private void RefreshHardwareSelector() => _hardwareSelection.Refresh();
    private HardwareChoice? SelectedHardware() => _hardwareSelection.Selected;
    private string? SelectedDeviceArgument() => _hardwareSelection.DeviceArgument();
    private string? SelectedDriveArgument() => _hardwareSelection.DriveArgument();
    private void HardwareSelector_Changed(object sender, SelectionChangedEventArgs e) => _hardwareSelection.OnSelectionChanged();
    private bool EnsureSelectedHardwareAvailable() => _hardwareSelection.EnsureAvailable();

    private void UpdateToolCommand() => _maintenanceTools.UpdatePreview();

    private void ConfirmAndRequestStop()
    {
        if (_dialogs.Show(LocExtension.Get("Operation.StopConfirm"), LocExtension.Get("Operation.StopTitle"), UserDialogButtons.YesNo, UserDialogIcon.Warning) == UserDialogResult.Yes)
            _operation.RequestCancellation();
    }

    private void UpdateProfileStatus()
    {
        if (ProfileStatusItem is null || MainTabs is null) return;
        string? name = MainTabs.SelectedIndex switch
        {
            MainWindowConstants.ReadTabIndex =>
                (ReadProfileCombo?.SelectedItem as OperationProfile)?.Name,
            MainWindowConstants.WriteTabIndex =>
                (WriteProfileCombo?.SelectedItem as OperationProfile)?.Name,
            MainWindowConstants.ConvertTabIndex =>
                (ConvertProfileCombo?.SelectedItem as OperationProfile)?.Name,
            _ => null
        };
        _viewModel.ProfileVisibility = name is null ? Visibility.Collapsed : Visibility.Visible;
        if (name is not null) _viewModel.ProfileText = LocExtension.Get("Status.Profile", name);
    }

    private Task CheckHostToolsUpdateAsync() => _hostToolsUpdate.CheckAsync();

    private void ShowHostToolsUpdateIfNeeded() => _hostToolsUpdate.Refresh();

    private static string DecoderName(string id) => LocExtension.Get("Visual.DecoderName." + id);
    private void ToolCommand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string verb }) return;
        if (_runner.IsRunning)
        {
            _dialogs.Show(LocExtension.Get("Operation.Busy"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information);
            return;
        }
        if (!EnsureSelectedHardwareAvailable()) return;
        if (string.IsNullOrWhiteSpace(_settings.GwExecutablePath) || !File.Exists(_settings.GwExecutablePath))
        {
            _dialogs.Show(LocExtension.Get("App.GwNotConfigured"), LocExtension.Get("App.Title"), icon: UserDialogIcon.Information);
            return;
        }
        _navigation.ShowGwTool(new(_settings.GwExecutablePath, verb, SelectedDeviceArgument(), SelectedDriveArgument(), _logsDirectory, _settings.Logging));
    }
}
