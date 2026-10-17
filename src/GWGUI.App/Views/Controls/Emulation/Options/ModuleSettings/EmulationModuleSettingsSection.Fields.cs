using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Storage;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Audio;
using GWGUI.App.Services.Storage;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationModuleSettingsSection
{
    private EmulationSettingsControlField CreateControlField(EmulationSettingsField field) =>
        new(
            LocExtension.GetForModule(_module, field.LabelResourceKey),
            CreateField(field),
            field.ExplanationResourceKey is null ? null : LocExtension.GetForModule(_module, field.ExplanationResourceKey),
            field.DetailedExplanationResourceKey is null ? null : LocExtension.GetForModule(_module, field.DetailedExplanationResourceKey),
            field);

    private FrameworkElement CreateField(EmulationSettingsField field)
    {
        FrameworkElement control = field.Editor switch
        {
            EmulationSettingsEditor.Selection => CreateSelection(field),
            EmulationSettingsEditor.Toggle => CreateToggle(field),
            EmulationSettingsEditor.Path => CreatePath(field),
            EmulationSettingsEditor.DirectoryPath => CreateDirectoryPath(field),
            EmulationSettingsEditor.OpenDirectory => CreateOpenDirectory(field),
            EmulationSettingsEditor.Slider => CreateSlider(field),
            EmulationSettingsEditor.Information => new TextBlock
            {
                Text = EmulationSettingsValuePresentationFunctions.DisplayValue(field, _module),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            },
            _ => new TextBox { Text = field.Value ?? string.Empty }
        };
        control.IsEnabled = field.IsEnabled;
        if (field.Editor is (EmulationSettingsEditor.Text or EmulationSettingsEditor.Number
                or EmulationSettingsEditor.Percentage) && control is TextBox input)
            input.LostKeyboardFocus += async (_, _) => await ExecuteUserChangeAsync();
        _fieldControls[field.Id] = control;
        return control;
    }

    private FrameworkElement CreateSelection(EmulationSettingsField field)
    {
        var choices = SelectionChoices(field);
        if (choices.Count == 2 && field.ChoiceSource == EmulationSettingsChoiceSource.Declared)
        {
            var radios = new WrapPanel();
            var group = Guid.NewGuid().ToString();
            var selected = choices.FirstOrDefault(choice => choice.Choice.Id == field.Value) ?? choices[0];
            foreach (var choice in choices)
            {
                var radio = new RadioButton { Content = new TextBlock { Text = choice.DisplayName, TextWrapping = TextWrapping.Wrap }, Tag = choice,
                    GroupName = group, IsChecked = choice.Choice.Id == selected.Choice.Id,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 4, 18, 4) };
                System.Windows.Automation.AutomationProperties.SetName(radio, choice.DisplayName);
                radio.Checked += async (_, _) =>
                {
                    if (_applyingSettingsRule) return;
                    if (field.RefreshSettingsOnChange) RefreshSettingsDescription();
                    await ExecuteUserChangeAsync();
                };
                radios.Children.Add(radio);
            }
            radios.Tag = (Func<string>)(() => radios.Children.OfType<RadioButton>()
                .Where(radio => radio.IsChecked == true).Select(radio => ((EmulationSettingsChoiceView)radio.Tag).Choice.Id).FirstOrDefault() ?? field.Value ?? string.Empty);
            return radios;
        }
        var selection = new ComboBox
        {
            ItemsSource = choices,
            SelectedItem = choices.FirstOrDefault(choice => choice.Choice.Id == field.Value) ?? choices.FirstOrDefault()
        };
        var choiceText = new FrameworkElementFactory(typeof(TextBlock));
        choiceText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(EmulationSettingsChoiceView.DisplayName)));
        choiceText.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        selection.ItemTemplate = new DataTemplate { VisualTree = choiceText };
        _userChangeHandlers[selection] = async () =>
        {
            if (field.RefreshSettingsOnChange)
            {
                CaptureEditorValues();
                RebuildEditor();
            }
            await ExecuteUserChangeAsync();
        };
        return selection;
    }

    private IReadOnlyList<EmulationSettingsChoiceView> SelectionChoices(EmulationSettingsField field)
    {
        var declared = field.Choices?.Select(choice => ChoiceView(choice, field.ValueUnitResourceKey)) ?? Enumerable.Empty<EmulationSettingsChoiceView>();
        if (field.ChoiceSource != EmulationSettingsChoiceSource.AudioOutputDevices)
            return declared.ToArray();
        var devices = WasapiAudioOutput.GetOutputDevices().Select(device => new EmulationSettingsChoiceView(new EmulationSettingsChoice(device.Id, string.Empty, device.Name), device.Name));
        return declared.Concat(devices).DistinctBy(choice => choice.Choice.Id).ToArray();
    }

    private EmulationSettingsChoiceView ChoiceView(EmulationSettingsChoice choice, string? unitResourceKey = null) => new(choice,
        EmulationSettingsValuePresentationFunctions.WithUnit(choice.InvariantDisplayValue ?? LocExtension.GetForModule(_module, choice.DisplayResourceKey), unitResourceKey));

    private CheckBox CreateToggle(EmulationSettingsField field)
    {
        var toggle = new CheckBox { IsChecked = field.Value == field.EnabledValue, Tag = field };
        _userChangeHandlers[toggle] = async () =>
        {
            if (field.RefreshSettingsOnChange) RefreshSettingsDescription();
            await ExecuteUserChangeAsync();
        };
        return toggle;
    }

    private void RefreshSettingsDescription()
    {
        CaptureEditorValues();
        RebuildEditor();
    }

    private FrameworkElement CreatePath(EmulationSettingsField field)
    {
        var path = new TextBox { Text = field.Value ?? string.Empty };
        path.LostKeyboardFocus += async (_, _) => await ExecuteUserChangeAsync();
        var browse = new Button { Content = LocExtension.Get(ControlVisualConstants.BrowseResource), MinWidth = 90 };
        browse.Click += async (_, _) =>
        {
            var currentDirectory = File.Exists(path.Text) ? Path.GetDirectoryName(path.Text) : Directory.Exists(path.Text) ? path.Text : null;
            var dialog = new OpenFileDialog
            {
                InitialDirectory = currentDirectory ?? DefaultFolder(field.DefaultFolderCategory),
                FileName = File.Exists(path.Text) ? Path.GetFileName(path.Text) : string.Empty
            };
            if (dialog.ShowDialog() == true)
            {
                path.Text = dialog.FileName;
                await ExecuteUserChangeAsync();
            }
        };
        return PathRow(path, browse);
    }

    private FrameworkElement CreateDirectoryPath(EmulationSettingsField field)
    {
        var path = new TextBox
        {
            Text = string.IsNullOrWhiteSpace(field.Value) ? DefaultFolder(field.DefaultFolderCategory) : field.Value
        };
        path.LostKeyboardFocus += async (_, _) => await ExecuteUserChangeAsync();
        var browse = new Button { Content = LocExtension.Get(ControlVisualConstants.BrowseResource), MinWidth = 90 };
        browse.Click += async (_, _) =>
        {
            var dialog = new OpenFolderDialog { InitialDirectory = path.Text };
            if (dialog.ShowDialog() == true)
            {
                path.Text = dialog.FolderName;
                await ExecuteUserChangeAsync();
            }
        };
        return PathRow(path, browse);
    }

    private FrameworkElement CreateOpenDirectory(EmulationSettingsField field)
    {
        var directory = DefaultFolder(field.DefaultFolderCategory);
        Directory.CreateDirectory(directory);
        var open = new Button
        {
            Content = LocExtension.Get("Common.OpenFolder"), Tag = directory,
            HorizontalAlignment = HorizontalAlignment.Left, MinWidth = 140
        };
        open.Click += async (_, _) => await ExecuteAsync(() =>
        {
            System.Diagnostics.Process? shellRequest = null;
            try
            {
                Directory.CreateDirectory(directory);
                shellRequest = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(directory) { UseShellExecute = true });
            }
            finally
            {
                // Explorer belongs to the desktop shell; release only our request handle.
                shellRequest?.Dispose();
            }
            return Task.CompletedTask;
        });
        return open;
    }

    private static Grid PathRow(TextBox path, Button browse)
    {
        var row = new Grid { Tag = path };
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(path);
        Grid.SetColumn(browse, 1);
        row.Children.Add(browse);
        return row;
    }

    private string DefaultFolder(EmulationDefaultFolderCategory? category)
    {
        var moduleFolder = _module.Id.Length == 0 ? _module.Id : char.ToUpperInvariant(_module.Id[0]) + _module.Id[1..];
        return category switch
        {
            EmulationDefaultFolderCategory.Floppy => Path.Combine(StoragePaths.EmulationStorageDirectory, StoragePathConstants.FloppiesDirectory, moduleFolder),
            EmulationDefaultFolderCategory.CompactDisc => Path.Combine(StoragePaths.EmulationStorageDirectory, StoragePathConstants.CompactDiscsDirectory, moduleFolder),
            EmulationDefaultFolderCategory.HardDisk => Path.Combine(StoragePaths.EmulationStorageDirectory, StoragePathConstants.HardDisksDirectory, moduleFolder),
            EmulationDefaultFolderCategory.Cartridge => Path.Combine(StoragePaths.EmulationStorageDirectory, StoragePathConstants.CartridgesDirectory, moduleFolder),
            EmulationDefaultFolderCategory.Cassette => Path.Combine(StoragePaths.EmulationStorageDirectory, StoragePathConstants.CassettesDirectory, moduleFolder),
            EmulationDefaultFolderCategory.State => Path.Combine(StoragePaths.EmulationStateDirectory, moduleFolder),
            EmulationDefaultFolderCategory.Capture => Path.Combine(StoragePaths.EmulationCaptureDirectory, moduleFolder),
            EmulationDefaultFolderCategory.Cheats => Path.Combine(StoragePaths.EmulationStorageDirectory, "Cheats", moduleFolder, _configuration.MachineId),
            EmulationDefaultFolderCategory.Firmware => _firmwareManagement?.GetFirmwareDirectory()
                ?? Path.Combine(StoragePaths.EmulationStorageDirectory, StoragePathConstants.RomsDirectory, moduleFolder),
            _ => StoragePaths.EmulationStorageDirectory
        };
    }
}
