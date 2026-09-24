using GWGUI.Infrastructure.Commands.Building;
using GWGUI.Infrastructure.Commands.Execution;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Formats.Detection;
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
using GWGUI.MediaEngine.Images.Visualization;
using System.ComponentModel;
using GWGUI.MediaEngine.Contracts.Explorer;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.Infrastructure.Processes;
using GWGUI.App.Constants.Views.Shell;
namespace GWGUI.App.Views.Windows.Shell;

public partial class MainWindow : Window
{
    private void ConnectMainMenu()
    {
        ApplicationMenu.PreferencesRequested += Preferences_Click;
        ApplicationMenu.UpdatesRequested += (_, _) => _navigation.ShowUpdates();
        ApplicationMenu.EmulationPreferencesRequested += (_, _) => _navigation.ShowEmulationPreferences(_settings);
        ApplicationMenu.EmulationModuleRequested += moduleId => _navigation.ShowEmulationModuleOptions(_settings, moduleId);
        ApplicationMenu.LogHistoryRequested += (_, _) => _navigation.ShowLogHistory(_logsDirectory);
        ApplicationMenu.DocumentationRequested += Documentation_Click;
        ApplicationMenu.AboutRequested += (_, _) => _navigation.ShowAbout();
        ApplicationMenu.ToolRequested += (sender, verb) => ToolCommand_Click(sender, new RoutedEventArgs());
        ApplicationMenu.SetEmulationModules(EmulationModuleRegistry.Modules);

        RegisterName(MainWindowConstants.OptionsMenuItemName, ApplicationMenu.OptionsMenuItem);
        RegisterName(MainWindowConstants.EmulationMenuItemName, ApplicationMenu.EmulationMenuItem);
        RegisterName(MainWindowConstants.HelpMenuItemName, ApplicationMenu.HelpMenuItem);
        RegisterName(MainWindowConstants.AlignMenuItemName, ApplicationMenu.AlignMenuItem);
    }

    private void ConnectStatusBar()
    {
        StatusBarBlock.HardwareSelectionChanged += HardwareSelector_Changed;
        StatusBarBlock.HostToolsUpdateRequested += Preferences_Click;
        StatusBarBlock.ToggleConsoleRequested += (_, _) => _terminalPanel.Toggle();
        RegisterName(nameof(HardwareStatusText), HardwareStatusText);
        RegisterName(nameof(HardwareSelector), HardwareSelector);
        RegisterName(nameof(OperationProgress), OperationProgress);
    }

    private void ConnectReadComponents()
    {
        RawScpRadio.Checked += (_, _) => _readTab.ModeChanged();
        KnownFormatRadio.Checked += (_, _) => _readTab.ModeChanged();
        ReadFamilyCombo.SelectionChanged += (_, _) => _readTab.FamilyChanged();
        ReadFormatCombo.SelectionChanged += (_, _) => _readTab.FormatChanged();
        ReadExtensionCombo.SelectionChanged += (_, _) => _readTab.InputChanged();
        ReadProfileCombo.SelectionChanged += (_, _) => _readTab.ProfileChanged();
        ReadProfileBlock.SaveButton.Click += (_, _) => _readTab.SaveProfile();
        ReadProfileBlock.ResetButton.Click += (_, _) => _readTab.ResetProfile();
        ReadFolderBlock.BrowseButton.Click += (_, _) => _readTab.BrowseFolder();
        ReadFileName.TextChanged += ReadInput_Changed;
        ReadAdvancedBlock.InputChanged += ReadInput_Changed;
        ReadAdvancedBlock.FakeIndexChecked += (_, _) => _readTab.EnableFakeIndex();
        ReadAdvancedBlock.HardSectorsChecked += (_, _) => _readTab.EnableHardSectors();
        ReadAdvancedBlock.DenselChecked += (_, _) => _readTab.EnableDensel();
        ReadAdvancedBlock.Tg43Checked += (_, _) => _readTab.EnableTg43();
        ReadAdvancedBlock.SequenceKindChanged += (_, _) => _readTab.ChangeSequenceKind();
        ReadCompletionBlock.ExploreRequested += async path =>
        {
            MainTabs.SelectedIndex = MainWindowConstants.ExplorerTabIndex;
            await _diskImageWorkspace.LoadAsync(path);
        };
        ReadCompletionBlock.VisualizeRequested += async path =>
        {
            MainTabs.SelectedIndex = MainWindowConstants.VisualizerTabIndex;
            await _diskImageWorkspace.LoadAsync(path);
        };
        ReadTabBlock.ExecuteRequested += ExecuteRead_Click;
        TerminalBlock.CopyButton.Click += (_, _) => _terminalPanel.CopyToClipboard();

        RegisterName(nameof(RawScpRadio), RawScpRadio);
        RegisterName(nameof(KnownFormatRadio), KnownFormatRadio);
        RegisterName(nameof(KnownFormatPanel), KnownFormatPanel);
        RegisterName(nameof(ReadFamilyCombo), ReadFamilyCombo);
        RegisterName(nameof(ReadFormatCombo), ReadFormatCombo);
        RegisterName(nameof(ReadExtensionCombo), ReadExtensionCombo);
        RegisterName(nameof(ReadProfileCombo), ReadProfileCombo);
        RegisterName(nameof(ReadFolder), ReadFolder);
        RegisterName(nameof(ReadFileName), ReadFileName);
        RegisterName(nameof(ReadExtensionText), ReadExtensionText);
        RegisterName(nameof(ReadRevsEnabled), ReadRevsEnabled);
        RegisterName(nameof(ReadExecuteButton), ReadExecuteButton);
        RegisterName(nameof(CommandPreview), CommandPreview!);
        RegisterName(nameof(LogOutput), LogOutput!);
    }

    private void ConnectWriteComponents()
    {
        WriteProfileCombo.SelectionChanged += (_, _) => _writeTab.ProfileChanged();
        WriteProfileBlock.SaveButton.Click += (_, _) => _writeTab.SaveProfile();
        WriteProfileBlock.ResetButton.Click += (_, _) => _writeTab.ResetProfile();
        WriteSourceBlock.BrowseButton.Click += async (_, _) => await _writeTab.BrowseSourceAsync();
        WriteFormatBlock.ModifyButton.Click += (_, _) => _writeTab.ToggleFormat();
        WriteFormatBlock.VisualizeTracksButton.Click += async (_, _) => await _writeTab.VisualizeSourceAsync();
        WriteFormatCombo.SelectionChanged += WriteInput_Changed;
        WriteAdvancedBlock.InputChanged += WriteInput_Changed;
        WriteAdvancedBlock.FakeIndexChecked += (_, _) => _writeTab.EnableFakeIndex();
        WriteAdvancedBlock.HardSectorsChecked += (_, _) => _writeTab.EnableHardSectors();
        WriteAdvancedBlock.DenselChecked += (_, _) => _writeTab.EnableDensel();
        WriteAdvancedBlock.Tg43Checked += (_, _) => _writeTab.EnableTg43();
        WriteTabBlock.ExecuteRequested += ExecuteWrite_Click;
        RegisterName(nameof(WriteProfileCombo), WriteProfileCombo);
        RegisterName(nameof(WriteSourceText), WriteSourceText);
        RegisterName(nameof(WriteDetectionText), WriteDetectionText);
        RegisterName(nameof(WriteFormatCombo), WriteFormatCombo);
        RegisterName(nameof(WriteNoVerify), WriteNoVerify);
        RegisterName(nameof(WriteDiskDefsEnabled), WriteDiskDefsEnabled);
        RegisterName(nameof(WriteDiskDefsValue), WriteDiskDefsValue);
        RegisterName(nameof(WriteExecuteButton), WriteExecuteButton);
    }

    private void ConnectConvertComponents()
    {
        ConvertProfileCombo.SelectionChanged += (_, _) => _conversionTab.ProfileChanged();
        ConvertProfileBlock.SaveButton.Click += (_, _) => _conversionTab.SaveProfile();
        ConvertProfileBlock.ResetButton.Click += (_, _) => _conversionTab.ResetProfile();
        ConvertSourceBlock.BrowseButton.Click += async (_, _) => await _conversionTab.BrowseSourceAsync();
        ConvertSourceBlock.ActionButton.Click += async (_, _) => await _conversionTab.VisualizeSourceAsync();
        ConvertOutputBlock.ValueChanged += (_, _) => _conversionTab.UpdateCommand();
        ConvertFormatsBlock.ValueChanged += (sender, _) => _conversionTab.SelectionChanged(sender);
        ConvertAdvancedBlock.InputChanged += (_, _) => _conversionTab.UpdateCommand();
        ConvertTabBlock.ExecuteRequested += ExecuteConvert_Click;
        ConvertTabBlock.MigrationRequested += (_, _) => _conversionTab.OpenMigration();
        RegisterName(nameof(ConvertProfileCombo), ConvertProfileCombo);
        RegisterName(nameof(ConvertSourceText), ConvertSourceText);
        RegisterName(nameof(ConvertOutputName), ConvertOutputName);
        RegisterName(nameof(ConvertTags), ConvertTags);
        RegisterName(nameof(ConvertSourceInfo), ConvertSourceInfo);
        RegisterName(nameof(ConvertTracksEnabled), ConvertTracksEnabled);
        RegisterName(nameof(ConvertDiskDefsEnabled), ConvertDiskDefsEnabled);
        RegisterName(nameof(ConvertDiskDefsValue), ConvertDiskDefsValue);
        RegisterName(nameof(ConvertFormatsBlock), ConvertFormatsBlock);
        RegisterName(nameof(ConvertExecuteButton), ConvertExecuteButton);
    }

    private void ConnectToolsComponents()
    {
        ToolsTabBlock.ToolSelectionChanged += (_, _) => _maintenanceTools.UpdateSelection();
        ToolsTabBlock.InputChanged += (_, _) => _maintenanceTools.UpdatePreview();
        ToolsTabBlock.EraseRequested += async (_, _) => await _maintenanceTools.ExecuteEraseAsync();
        ToolsTabBlock.CleanRequested += async (_, _) => await _maintenanceTools.ExecuteCleanAsync();
        RegisterName(nameof(EraseExecuteButton), EraseExecuteButton);
        RegisterName(nameof(CleanExecuteButton), CleanExecuteButton);
    }

    private void ConnectExplorerComponent()
    {
        DiskExplorer.OpenRequested += async (_, _) =>
        {
            var path = _diskImageWorkspace.SelectExplorerImage();
            if (path is not null) await _diskImageWorkspace.LoadAsync(path);
        };
        DiskExplorer.ReadDiskRequested += async (_, _) => await _explorerRead.ExecuteAsync();
        DiskExplorer.FormatChanged += async (_, _) =>
        {
            await _diskImageWorkspace.SelectExplorerRepresentationAsync();
        };
    }

}
