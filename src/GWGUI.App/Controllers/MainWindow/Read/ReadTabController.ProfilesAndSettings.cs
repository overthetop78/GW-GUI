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
using GWGUI.App.Constants.Controls.Visual;
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

internal sealed partial class ReadTabController
{
    internal void RefreshProfiles(string? selectedId = null)
        => profileController.Refresh(ProfileCombo, OperationKind.Read, selectedId);

    internal void ProfileChanged()
    {
        if (ProfileCombo.SelectedItem is not OperationProfile profile || view.AdvancedBlock.RevsEnabledCheckBox is null)
            return;

        ApplyProfile(profile);
        updateProfileStatus();
    }

    internal void ResetProfile()
    {
        if (ProfileCombo.SelectedItem is OperationProfile profile)
            ApplyProfile(profile);
    }

    internal void SaveProfile()
    {
        var enabled = viewModel.Read.CaptureEnabledOptions();
        var values = viewModel.Read.CaptureValues();
        values[ReadTabConstants.ResultProfileKey] = RawScpRadio.IsChecked == true
            ? ReadTabConstants.RawResultId : ReadTabConstants.KnownResultId;
        if (FormatCombo.SelectedItem is DiskFormat format) values[ReadTabConstants.FormatProfileKey] = format.Id;
        if (ExtensionCombo.SelectedItem is ImageExtension extension)
            values[ReadTabConstants.ExtensionProfileKey] = extension.Extension;
        if (!string.IsNullOrWhiteSpace(viewModel.Read.Folder))
            values[ReadTabConstants.FolderProfileKey] = viewModel.Read.Folder;

        var profile = profileController.Save(OperationKind.Read, name =>
            new OperationProfile(Guid.NewGuid().ToString("N"), OperationKind.Read, name, values, enabled));
        if (profile is not null)
            RefreshProfiles(profile.Id);
    }

    internal void InputChanged() => UpdateCommand();

    internal void ModeChanged()
    {
        KnownFormatPanel.Visibility = KnownFormatRadio.IsChecked == true
            ? Visibility.Visible
            : Visibility.Collapsed;
        UpdateExtension();
        UpdateCommand();
    }

    internal void FamilyChanged()
    {
        if (FormatCombo is null || FamilyCombo.SelectedItem is not string family) return;
        FormatCombo.ItemsSource = formatCatalog().Formats
            .Where(format => format.Family == family && format.SupportsPhysicalRead)
            .ToArray();
        FormatCombo.SelectedIndex = 0;
    }

    internal void FormatChanged()
    {
        if (ExtensionCombo is null) return;
        ExtensionCombo.ItemsSource = (FormatCombo.SelectedItem as DiskFormat)?.Extensions;
        var extensions = ExtensionCombo.ItemsSource as IReadOnlyList<ImageExtension>;
        ExtensionCombo.SelectedIndex = extensions is null
            ? -1
            : Math.Max(0, extensions.ToList().FindIndex(x => x.IsDefault));
        UpdateExtension();
        UpdateCommand();
    }

    internal void UpdateCommand()
    {
        var extension = GetExtension();
        var target = GetTarget(extension);
        NamePreview.Text = Path.GetFileName(target);
        if (UsesInternalPhysicalRead)
        {
            commandPreview.Text = LocExtension.Get("Read.InternalPreview", target);
            return;
        }

        try
        {
            commandPreview.Text = BuildCommand(target).ToDisplayString();
        }
        catch (ArgumentException)
        {
            commandPreview.Text = $"{ControlVisualConstants.WarningSymbol} {LocExtension.Get("Advanced.Invalid", LocExtension.Get("Common.Unknown"))}";
        }
    }

    internal string GetTarget(string extension)
        => viewModel.Read.BuildTarget(extension, ReadTabConstants.ExampleFallbackName);

    internal string GetExtension()
        => RawScpRadio.IsChecked == true
            ? ReadTabConstants.ScpExtension
            : (ExtensionCombo.SelectedItem as ImageExtension)?.Extension ?? string.Empty;

    internal GwCommand BuildCommand(string target)
    {
        return commandBuilder.BuildRead(new ReadRequest(
            settings().GwExecutablePath ?? ReadTabConstants.DefaultGreaseweazleExecutable,
            target,
            RawScpRadio.IsChecked == true ? ReadResultKind.RawScp : ReadResultKind.KnownFormat,
            (FormatCombo.SelectedItem as DiskFormat)?.Id,
            viewModel.Read.BuildOptions(),
            selectedDeviceArgument(),
            selectedDriveArgument(),
            viewModel.Read.ExpertArguments));
    }

    internal void CopyName()
    {
        var fileName = view.FileNameBlock.FileNameTextBox.Text;
        if (!string.IsNullOrEmpty(fileName))
            Clipboard.SetText(fileName);
    }

    internal void BrowseFolder()
    {
        var folder = view.FolderBlock.Input;
        var path = fileDialogs.SelectFolder(new(LocExtension.Get("Read.DestinationFolder"), folder.Text));
        if (path is null) return;

        viewModel.Read.Folder = path;
        UpdateCommand();
    }

    internal void EnableFakeIndex()
    {
        viewModel.Read.EnableFakeIndex();
        UpdateCommand();
    }

    internal void ChangeSequenceKind()
    {
        var sequenceKind = view.AdvancedBlock.SequenceKindComboBox;
        var sequenceValue = view.AdvancedBlock.SequenceValueTextBox;
        var targetKind = sequenceKind.SelectedIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;
        var sourceKind = targetKind == SequenceKind.Alphabetic ? SequenceKind.Numeric : SequenceKind.Alphabetic;
        if (SequenceFormatter.TryParse(sequenceValue.Text, sourceKind, out var value))
            viewModel.Read.SequenceValue = targetKind == SequenceKind.Numeric
                ? (value + 1).ToString()
                : SequenceFormatter.Format(Math.Max(0, value - 1), targetKind, 1);
        UpdateCommand();
    }

    internal void EnableHardSectors()
    {
        viewModel.Read.EnableHardSectors();
        UpdateCommand();
    }

    internal void EnableDensel()
    {
        viewModel.Read.EnableDensel();
        UpdateCommand();
    }

    internal void EnableTg43()
    {
        viewModel.Read.EnableTg43();
        UpdateCommand();
    }

    internal void RestoreSettings()
    {
        var readSettings = settings().Read;
        KnownFormatRadio.IsChecked = readSettings.UseKnownFormat;
        RawScpRadio.IsChecked = !readSettings.UseKnownFormat;
        SelectFormat(readSettings.FormatId, readSettings.ImageExtension);
        viewModel.Read.AutoNumber = readSettings.AutoNumber;
        viewModel.Read.SequenceKindIndex = readSettings.SequenceKind == ReadTabConstants.AlphabeticSequenceId ? 1 : 0;
        viewModel.Read.SequenceWidthIndex = Math.Clamp(readSettings.SequenceWidth - 1, 0, 2);
        viewModel.Read.SequenceValue = readSettings.SequenceKind == ReadTabConstants.AlphabeticSequenceId
            ? SequenceFormatter.Format(readSettings.NextSequence, SequenceKind.Alphabetic, 1)
            : readSettings.NextSequence.ToString();
        viewModel.Read.ApplyOptions(readSettings.EnabledOptions, readSettings.OptionValues);
    }

    internal void CaptureSettings()
    {
        var readSettings = settings().Read;
        readSettings.UseKnownFormat = KnownFormatRadio.IsChecked == true;
        readSettings.FormatId = (FormatCombo.SelectedItem as DiskFormat)?.Id;
        readSettings.ImageExtension = (ExtensionCombo.SelectedItem as ImageExtension)?.Extension;
        readSettings.AutoNumber = viewModel.Read.AutoNumber;
        readSettings.SequenceKind = viewModel.Read.SequenceKind == SequenceKind.Alphabetic
            ? ReadTabConstants.AlphabeticSequenceId : ReadTabConstants.NumericSequenceId;
        readSettings.SequenceWidth = viewModel.Read.SequenceWidthIndex + 1;
        if (SequenceFormatter.TryParse(viewModel.Read.SequenceValue, viewModel.Read.SequenceKind, out var sequence))
            readSettings.NextSequence = sequence;
        readSettings.EnabledOptions = viewModel.Read.CaptureEnabledOptions();
        readSettings.OptionValues = viewModel.Read.CaptureValues();
    }

    internal PhysicalDiskReadOptions CreateInternalOptions(HardwareChoice hardware)
    {
        var selection = GreaseweazleDriveSelectionFunctions.Resolve(hardware.Drive.Selection);
        var tracks = PhysicalDiskTrackSelectionParser.Parse(
            viewModel.Read.Tracks.Enabled ? viewModel.Read.Tracks.Value : ReadTabConstants.DefaultTrackSelection);
        var revolutions = viewModel.Read.Revs.Enabled
            ? int.Parse(viewModel.Read.Revs.Value)
            : PhysicalDiskReadDefaults.Revolutions;
        var retries = viewModel.Read.Retries.Enabled
            ? int.Parse(viewModel.Read.Retries.Value)
            : PhysicalDiskReadDefaults.FluxOverflowRetries;
        var seekRetries = viewModel.Read.SeekRetries.Enabled
            ? int.Parse(viewModel.Read.SeekRetries.Value)
            : PhysicalDiskReadDefaults.SeekRetries;
        TimeSpan? fakeIndex = viewModel.Read.FakeIndex.Enabled
            ? PhysicalDiskIndexPeriodParser.Parse(viewModel.Read.FakeIndex.Value)
            : null;
        return new PhysicalDiskReadOptions(
            hardware.Port,
            selection.BusType,
            selection.Unit,
            tracks,
            ScpCaptureDiskTypeFunctions.Resolve(hardware.Drive.Density),
            revolutions,
            retries,
            seekRetries,
            fakeIndex,
            viewModel.Read.HardSectors.Enabled);
    }

    internal bool HasUnsupportedInternalOptions() =>
        viewModel.Read.AdjustSpeed.Enabled ||
        viewModel.Read.Pll.Enabled ||
        viewModel.Read.Reverse.Enabled ||
        viewModel.Read.Densel.Enabled ||
        viewModel.Read.Tg43.Enabled ||
        viewModel.Read.DiskDefs.Enabled ||
        !string.IsNullOrWhiteSpace(viewModel.Read.ExpertArguments);

}
