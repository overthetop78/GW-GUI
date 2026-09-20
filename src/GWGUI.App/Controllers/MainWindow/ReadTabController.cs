using GWGUI.Infrastructure.Commands;
using GWGUI.Infrastructure.Commands.Building;
using GWGUI.Infrastructure.Commands.Execution;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.Infrastructure.Naming;
using GWGUI.App.Profiles;
using GWGUI.Infrastructure.Read;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Settings.Engines;
using GWGUI.App.Constants.Services.PhysicalDiskReading;
using GWGUI.App.Constants.Views.Read;
using GWGUI.App.Contracts.Services.Hardware;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Functions.Services.Conversion;
using GWGUI.App.Functions.Services.PhysicalDiskReading;
using GWGUI.App.Functions.Services.PhysicalDiskWriting;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Logging;
using GWGUI.App.Services.Operations;
using GWGUI.App.Services.PhysicalDiskReading;
using GWGUI.App.Services.Profiles;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Windows.Shell;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using GWGUI.Infrastructure.Processes;

namespace GWGUI.App.Controllers.MainWindow;

internal sealed partial class ReadTabController(
    ReadTabSection view,
    MainWindowViewModel viewModel,
    OperationProfileController profileController,
    Func<IImageFormatCatalog> formatCatalog,
    Func<AppSettings> settings,
    IGwCommandBuilder commandBuilder,
    IFileDialogService fileDialogs,
    IBusinessDialogService businessDialogs,
    IMessageDialogService dialogs,
    DiskDefinitionsController diskDefinitionsController,
    OperationRuntimeController operation,
    OperationProgressController progress,
    ConsoleLogSession consoleLog,
    IGreaseweazleRunner runner,
    DiskImageWorkspaceController diskImageWorkspace,
    TextBox commandPreview,
    TextBox logOutput,
    Func<string?> selectedDeviceArgument,
    Func<string?> selectedDriveArgument,
    Func<bool> ensureSelectedHardwareAvailable,
    Func<HardwareChoice?> selectedHardware,
    Action confirmAndRequestStop,
    Action updateProfileStatus,
    Action updateReadCommand,
    Func<string?, bool>? fileExists = null,
    Func<string, Exception?>? deleteCancelledOutput = null,
    Action? selectOutputName = null,
    Func<string, Task<GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection.ScpCaptureInfo>>? readCaptureInfo = null,
    Action<Exception, string>? logError = null,
    Func<InternalPhysicalDiskReader>? internalReaderFactory = null)
{
    private readonly Func<string?, bool> exists = fileExists ?? File.Exists;
    private readonly Func<string, Exception?> deleteOutput = deleteCancelledOutput ?? CancelledOutputCleaner.TryDelete;
    private readonly Func<string, Task<GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection.ScpCaptureInfo>> captureInfo = readCaptureInfo ?? (path => GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection.ScpCaptureInfoReader.ReadAsync(path));
    private readonly Action<Exception, string> reportError = logError ?? ((error, context) => ErrorLog.Write(error, context));
    private readonly Func<InternalPhysicalDiskReader> createInternalReader = internalReaderFactory ?? InternalPhysicalDiskReader.CreateDefault;
    private ComboBox ProfileCombo => view.ProfileBlock.ProfileCombo;
    private RadioButton RawScpRadio => view.ImageBlock.RawScpRadio;
    private RadioButton KnownFormatRadio => view.ImageBlock.KnownFormatRadio;
    private ComboBox FormatCombo => view.ImageBlock.FormatCombo;
    private ComboBox FamilyCombo => view.ImageBlock.FamilyCombo;
    private ComboBox ExtensionCombo => view.ImageBlock.ExtensionCombo;
    private Grid KnownFormatPanel => view.ImageBlock.KnownFormatPanel;
    private TextBox ExtensionText => view.FileNameBlock.ExtensionTextBox;
    private TextBlock NamePreview => view.AdvancedBlock.NamePreviewTextBlock;

    private bool UsesInternalPhysicalRead => settings().Engines.PhysicalRead == OperationEngine.Internal;
    private string? lastInternalReadProgressLine;

    internal void UpdateExtension()
    {
        ExtensionText.Text = RawScpRadio.IsChecked == true
            ? LocExtension.Get("Read.RawScp")
            : (ExtensionCombo.SelectedItem as ImageExtension)?.DisplayName ?? LocExtension.Get("Read.ChooseType");
    }

    private void ApplyProfile(OperationProfile profile)
    {
        viewModel.Read.ApplyOptions(profile.EnabledOptions, profile.Values);
        if (profile.IsSystem)
        {
            RawScpRadio.IsChecked = true;
        }
        else
        {
            if (profile.Values.GetValueOrDefault(ReadTabConstants.ResultProfileKey) == ReadTabConstants.RawResultId)
                RawScpRadio.IsChecked = true;
            else if (profile.Values.GetValueOrDefault(ReadTabConstants.ResultProfileKey) == ReadTabConstants.KnownResultId)
                KnownFormatRadio.IsChecked = true;
            SelectFormat(profile.Values.GetValueOrDefault(ReadTabConstants.FormatProfileKey),
                profile.Values.GetValueOrDefault(ReadTabConstants.ExtensionProfileKey));
            if (profile.Values.TryGetValue(ReadTabConstants.FolderProfileKey, out var folder)
                && !string.IsNullOrWhiteSpace(folder))
                viewModel.Read.Folder = folder;
        }

        updateReadCommand();
    }

    private void SelectFormat(string? formatId, string? imageExtension)
    {
        var format = formatCatalog().Formats.FirstOrDefault(item => item.Id == formatId);
        if (format is null) return;

        FamilyCombo.SelectedItem = format.Family;
        FormatCombo.SelectedItem = format;
        var extension = format.Extensions.FirstOrDefault(item =>
            item.Extension.Equals(imageExtension, StringComparison.OrdinalIgnoreCase));
        if (extension is not null)
            ExtensionCombo.SelectedItem = extension;
    }
}
