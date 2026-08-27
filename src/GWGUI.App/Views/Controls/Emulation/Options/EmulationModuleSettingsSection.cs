using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Constants.Storage;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Contracts.Emulation.Machine;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Controllers.Emulation.Firmware;
using GWGUI.App.Controllers.Emulation.Input;
using GWGUI.App.Controllers.Emulation.Options;
using GWGUI.App.Controllers.Emulation.Storage;
using GWGUI.App.Functions.Views.Emulation.Machine;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Services.Audio;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Services.Storage;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using GWGUI.Emulation;
using Microsoft.Win32;
using System.IO;


namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationModuleSettingsSection : UserControl
{
    private readonly IEmulationModule _module;
    private readonly ComboBox _machines = new() { MinWidth = 300 };
    private readonly Dictionary<string, FrameworkElement> _fieldControls = new(StringComparer.Ordinal);
    private readonly Dictionary<FrameworkElement, Func<Task>> _userChangeHandlers = [];
    private readonly EmulationEmulatorManagementController? _emulatorManagement;
    private readonly EmulationFirmwareManagementController? _firmwareManagement;
    private readonly EmulationInputSettingsController? _inputSettings;
    private readonly EmulationStorageSettingsController? _storageSettings;
    private IReadOnlyList<IEmulationConfiguration> _saved = [];
    private IEmulationConfiguration _configuration;
    private bool _loading;
    private readonly SemaphoreSlim _saveInputGate = new(1, 1);
    private EmulationMachineTab _selectedTab = EmulationMachineTab.General;

    internal EmulationModuleSettingsSection(IEmulationModule module)
    {
        FlowDirection = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
        _module = module;
        _machines.ItemTemplate = EmulationMachineChoiceLayout.CreateTemplate();
        var choices = module.Machines.Select(machine => new EmulationMachineChoice(machine,
            LocExtension.Get(machine.DisplayResourceKey), false)).ToArray();
        _machines.ItemsSource = choices;
        _machines.SelectedIndex = 0;
        _configuration = module.CreateConfiguration(choices[0].Definition.Id);
        if (module is IEmulationEmulatorManager manager)
            _emulatorManagement = new EmulationEmulatorManagementController(manager, CurrentMachineId);
        if (module is IEmulationFirmwareManager firmwareManager)
        {
            _firmwareManagement = new EmulationFirmwareManagementController(firmwareManager,
                () => _configuration, SetConfiguration);
            _firmwareManagement.ConfigurationChanged += async (_, _) => await ExecuteUserChangeAsync();
        }
        if (module is IEmulationInputSettingsManager inputManager)
        {
            _inputSettings = new EmulationInputSettingsController(inputManager);
            _inputSettings.SettingsChanged += async (_, _) => await ExecuteUserChangeAsync();
        }
        if (module is IEmulationStorageSettingsManager storageManager)
        {
            _storageSettings = new EmulationStorageSettingsController(storageManager, DefaultFolder);
            _storageSettings.SettingsChanged += async (_, _) => await ExecuteUserChangeAsync();
        }
        _machines.SelectionChanged += MachineChanged;
        Content = BuildEditor();
        Loaded += async (_, _) => await ExecuteAsync(ReloadAsync);
    }

    internal event EventHandler<EmulationConfigurationSavedEventArgs>? ConfigurationSaved;
    internal event EventHandler<EmulationMachineEditingContext>? EditingContextChanged;

    internal Task ReloadWhenOpenedAsync() => ExecuteAsync(ReloadAsync);

    private async Task ReloadAsync()
    {
        _saved = await _module.LoadConfigurationsAsync();
        var machineId = _configuration.MachineId;
        var selected = _saved.FirstOrDefault(item => item.Id == _configuration.Id)
            ?? _saved.FirstOrDefault(item => item.MachineId == machineId);
        _configuration = selected
            ?? (EmulationConfigurationDraftStore.TryGet(_module.Id, machineId, out var draft)
                ? draft : _module.CreateConfiguration(machineId));
        var choices = _module.Machines.Select(machine => new EmulationMachineChoice(machine,
            LocExtension.Get(machine.DisplayResourceKey),
            _saved.Any(configuration => configuration.MachineId == machine.Id))).ToArray();
        _machines.ItemsSource = choices;
        SelectMachine(machineId);
        RebuildEditor();
        if (_emulatorManagement is not null) await _emulatorManagement.RefreshAsync();
        NotifyEditingContextChanged();
    }

    private UIElement BuildEditor()
    {
        return BuildMachineTabs();
    }

    private UIElement BuildGeneralHeader()
    {
        var heading = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
        heading.ColumnDefinitions.Add(new ColumnDefinition());
        heading.Children.Add(new TextBlock
        {
            Text = LocExtension.Get("Emulation.Model"),
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        });
        Grid.SetColumn(_machines, 1);
        heading.Children.Add(_machines);
        var save = new Button
        {
            Content = LocExtension.Get("Common.Create"),
            MinWidth = 110,
            HorizontalAlignment = HorizontalAlignment.Right,
            Visibility = _saved.Any(configuration => configuration.MachineId == _configuration.MachineId)
                ? Visibility.Collapsed : Visibility.Visible
        };
        save.Click += async (_, _) => await ExecuteAsync(SaveAsync);
        Grid.SetColumn(save, 2);
        heading.Children.Add(save);
        return heading;
    }

    private UIElement BuildMachineTabs()
    {
        _fieldControls.Clear();
        _userChangeHandlers.Clear();
        var settings = _module.Describe(_configuration.MachineId, _configuration);
        var tabs = EmulationMachineTabs.Create(tab => settings.Visibility.Tabs.GetValueOrDefault(tab)
            ? BuildTab(settings, tab)
            : null, $"{_module.Id}:{_configuration.MachineId}", TabActivatedAsync, _selectedTab);
        AttachUserChangeHandlers();
        return tabs;
    }

    private UIElement BuildTab(EmulationMachineSettings settings, EmulationMachineTab tab)
    {
        if (tab == EmulationMachineTab.Cpu) return BuildCpuSettingsTab(settings);
        if (tab == EmulationMachineTab.Ram) return BuildMemorySettingsTab(settings);
        if (tab is (EmulationMachineTab.Keyboard or EmulationMachineTab.Mouse
            or EmulationMachineTab.Controllers) && _inputSettings is not null)
            return BuildInputSettingsTab(settings, tab);
        var panel = new StackPanel { Margin = new Thickness(12) };
        if (tab == EmulationMachineTab.General) panel.Children.Add(BuildGeneralHeader());
        if (tab == EmulationMachineTab.General && _emulatorManagement is not null)
            panel.Children.Add(_emulatorManagement.CreateView());
        AddBlocks(panel, settings, tab);
        ApplySettingsRules(settings);
        if (_storageSettings is not null && tab == EmulationMachineTab.Storage)
            panel.Children.Insert(0, _storageSettings.CreateContent(_configuration));
        if (tab == EmulationMachineTab.Rom && _firmwareManagement is not null)
            return _firmwareManagement.CreateView(panel);
        return EmulationSettingsLayout.ScrollPage(panel);
    }

    private void AddBlocks(Panel panel, EmulationMachineSettings settings, EmulationMachineTab tab)
    {
        foreach (var block in settings.Blocks.Where(block => block.Tab == tab && block.IsVisible))
        {
            var fields = block.Fields.Where(field => field.IsVisible)
                .Select(field => (LocExtension.Get(field.LabelResourceKey), CreateField(field))).ToArray();
            if (fields.Length == 0) continue;
            var form = EmulationSettingsLayout.CompactForm(Math.Max(1, block.Columns), fields);
            panel.Children.Add(EmulationSettingsLayout.IconCard(form,
                LocExtension.Get(block.TitleResourceKey), block.Icon ?? "\uE713"));
        }
    }

    private FrameworkElement CreateField(EmulationSettingsField field)
    {
        FrameworkElement control = field.Editor switch
        {
            EmulationSettingsEditor.Selection => CreateSelection(field),
            EmulationSettingsEditor.Toggle => CreateToggle(field),
            EmulationSettingsEditor.Path => CreatePath(field),
            EmulationSettingsEditor.DirectoryPath => CreateDirectoryPath(field),
            EmulationSettingsEditor.Information => new TextBlock
            {
                Text = field.Value,
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

    private ComboBox CreateSelection(EmulationSettingsField field)
    {
        var choices = SelectionChoices(field);
        var selection = new ComboBox
        {
            ItemsSource = choices,
            SelectedItem = choices.FirstOrDefault(choice => choice.Choice.Id == field.Value)
                ?? choices.FirstOrDefault()
        };
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

    private static IReadOnlyList<EmulationSettingsChoiceView> SelectionChoices(EmulationSettingsField field)
    {
        var declared = field.Choices?.Select(ChoiceView)
            ?? Enumerable.Empty<EmulationSettingsChoiceView>();
        if (field.ChoiceSource != EmulationSettingsChoiceSource.AudioOutputDevices)
            return declared.ToArray();
        var devices = WasapiAudioOutput.GetOutputDevices().Select(device => new EmulationSettingsChoiceView(
            new EmulationSettingsChoice(device.Id, string.Empty, device.Name), device.Name));
        return declared.Concat(devices).DistinctBy(choice => choice.Choice.Id).ToArray();
    }

    private static EmulationSettingsChoiceView ChoiceView(EmulationSettingsChoice choice) =>
        new(choice, choice.InvariantDisplayValue ?? LocExtension.Get(choice.DisplayResourceKey));

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
        var browse = new Button { Content = LocExtension.Get("Common.Browse"), MinWidth = 90 };
        browse.Click += async (_, _) =>
        {
            var currentDirectory = File.Exists(path.Text) ? Path.GetDirectoryName(path.Text)
                : Directory.Exists(path.Text) ? path.Text : null;
            var initialDirectory = currentDirectory ?? DefaultFolder(field.DefaultFolderCategory);
            var dialog = new OpenFileDialog
            {
                InitialDirectory = initialDirectory,
                FileName = File.Exists(path.Text) ? Path.GetFileName(path.Text) : string.Empty
            };
            if (dialog.ShowDialog() == true)
            {
                path.Text = dialog.FileName;
                await ExecuteUserChangeAsync();
            }
        };
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(path);
        Grid.SetColumn(browse, 1);
        row.Children.Add(browse);
        row.Tag = path;
        return row;
    }

    private FrameworkElement CreateDirectoryPath(EmulationSettingsField field)
    {
        var path = new TextBox { Text = string.IsNullOrWhiteSpace(field.Value)
            ? DefaultFolder(field.DefaultFolderCategory) : field.Value };
        path.LostKeyboardFocus += async (_, _) => await ExecuteUserChangeAsync();
        var browse = new Button
        {
            Content = LocExtension.Get(ControlVisualConstants.BrowseResource),
            MinWidth = 90
        };
        browse.Click += async (_, _) =>
        {
            var dialog = new OpenFolderDialog { InitialDirectory = path.Text };
            if (dialog.ShowDialog() == true)
            {
                path.Text = dialog.FolderName;
                await ExecuteUserChangeAsync();
            }
        };
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(path);
        Grid.SetColumn(browse, 1);
        row.Children.Add(browse);
        row.Tag = path;
        return row;
    }

    private string DefaultFolder(EmulationDefaultFolderCategory? category)
    {
        var moduleFolder = _module.Id.Length == 0 ? _module.Id
            : char.ToUpperInvariant(_module.Id[0]) + _module.Id[1..];
        return category switch
        {
            EmulationDefaultFolderCategory.Floppy => Path.Combine(StoragePaths.EmulationStorageDirectory,
                StoragePathConstants.FloppiesDirectory, moduleFolder),
            EmulationDefaultFolderCategory.CompactDisc => Path.Combine(StoragePaths.EmulationStorageDirectory,
                StoragePathConstants.CompactDiscsDirectory, moduleFolder),
            EmulationDefaultFolderCategory.HardDisk => Path.Combine(StoragePaths.EmulationStorageDirectory,
                StoragePathConstants.HardDisksDirectory, moduleFolder),
            EmulationDefaultFolderCategory.Cartridge => Path.Combine(StoragePaths.EmulationStorageDirectory,
                StoragePathConstants.CartridgesDirectory, moduleFolder),
            EmulationDefaultFolderCategory.Cassette => Path.Combine(StoragePaths.EmulationStorageDirectory,
                StoragePathConstants.CassettesDirectory, moduleFolder),
            EmulationDefaultFolderCategory.State => Path.Combine(StoragePaths.EmulationStateDirectory, moduleFolder),
            EmulationDefaultFolderCategory.Capture => Path.Combine(StoragePaths.EmulationCaptureDirectory, moduleFolder),
            EmulationDefaultFolderCategory.Firmware => _firmwareManagement?.GetFirmwareDirectory()
                ?? Path.Combine(StoragePaths.EmulationStorageDirectory, "ROM", moduleFolder),
            _ => StoragePaths.EmulationStorageDirectory
        };
    }

    private async void MachineChanged(object sender, SelectionChangedEventArgs args)
    {
        if (_loading || _machines.SelectedItem is not EmulationMachineChoice selected) return;
        await ExecuteAsync(async () =>
        {
            _configuration = _saved.FirstOrDefault(item => item.MachineId == selected.Definition.Id)
                ?? (EmulationConfigurationDraftStore.TryGet(_module.Id, selected.Definition.Id, out var draft)
                    ? draft : _module.CreateConfiguration(selected.Definition.Id));
            RebuildEditor();
            if (_emulatorManagement is not null) await _emulatorManagement.RefreshAsync();
            NotifyEditingContextChanged();
        });
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        try { await action(); }
        catch (Exception error)
        {
            ControlErrorPresenter.ShowEmulation(this, error,
                ControlErrorContexts.EmulationConfigurationManagement,
                LocExtension.Get(_module.DisplayResourceKey));
        }
    }

    private Task ExecuteUserChangeAsync() => ExecuteAsync(ApplyUserChangeAsync);

    private async Task ApplyUserChangeAsync()
    {
        CaptureEditorValues();
        if (!_saved.Any(configuration => configuration.MachineId == _configuration.MachineId))
        {
            EmulationConfigurationDraftStore.Set(_module.Id, _configuration);
            return;
        }
        await _saveInputGate.WaitAsync();
        try
        {
            CaptureEditorValues();
            var configuration = _configuration;
            await _module.SaveConfigurationAsync(configuration);
            ConfigurationSaved?.Invoke(this, new EmulationConfigurationSavedEventArgs(configuration));
        }
        finally
        {
            _saveInputGate.Release();
        }
    }

    private async Task SaveAsync()
    {
        var values = _fieldControls.ToDictionary(item => item.Key, item => ReadValue(item.Value),
            StringComparer.Ordinal);
        _configuration = _module.ApplySettings(_configuration, values);
        if (_inputSettings is not null)
            _configuration = _inputSettings.Apply(_configuration);
        if (_storageSettings is not null)
            _configuration = _storageSettings.Apply(_configuration);
        await _module.SaveConfigurationAsync(_configuration);
        EmulationConfigurationDraftStore.Remove(_module.Id, _configuration.MachineId);
        ConfigurationSaved?.Invoke(this, new EmulationConfigurationSavedEventArgs(_configuration));
        await ReloadAsync();
    }

    private static string? ReadValue(FrameworkElement control) => control switch
    {
        ComboBox { SelectedItem: EmulationSettingsChoiceView selected } => selected.Choice.Id,
        CheckBox { Tag: EmulationSettingsField field } toggle => toggle.IsChecked == true
            ? field.EnabledValue : field.DisabledValue,
        TextBox text => text.Text,
        Grid { Tag: TextBox path } => path.Text,
        _ => null
    };

    private void SelectMachine(string machineId)
    {
        _loading = true;
        _machines.SelectedItem = _machines.Items.Cast<EmulationMachineChoice>()
            .First(item => item.Definition.Id == machineId);
        _loading = false;
    }

    private void RebuildEditor()
    {
        if (_machines.Parent is Panel parent)
            parent.Children.Remove(_machines);
        Content = null;
        Content = BuildEditor();
    }

    private void SetConfiguration(IEmulationConfiguration configuration)
    {
        _configuration = configuration;
        RebuildEditor();
    }

    private string CurrentMachineId() => _configuration.MachineId;

    private void NotifyEditingContextChanged()
    {
        var machine = (_machines.SelectedItem as EmulationMachineChoice)?.DisplayName
            ?? _configuration.MachineId;
        EditingContextChanged?.Invoke(this, new EmulationMachineEditingContext(
            LocExtension.Get(_module.DisplayResourceKey), machine));
    }

    private Task TabActivatedAsync(EmulationMachineTab tab) =>
        RememberTabAndActivateAsync(tab);

    private async Task RememberTabAndActivateAsync(EmulationMachineTab tab)
    {
        _selectedTab = tab;
        if (tab == EmulationMachineTab.Rom && _firmwareManagement is not null)
            await _firmwareManagement.RefreshAsync();
    }

    internal void RefreshLocalizedContent()
    {
        CaptureEditorValues();
        var choices = _module.Machines.Select(machine => new EmulationMachineChoice(machine,
            LocExtension.Get(machine.DisplayResourceKey),
            _saved.Any(configuration => configuration.MachineId == machine.Id))).ToArray();
        _machines.ItemsSource = choices;
        SelectMachine(_configuration.MachineId);
        FlowDirection = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        RebuildEditor();
        NotifyEditingContextChanged();
    }

    private void CaptureEditorValues()
    {
        if (_fieldControls.Count != 0)
            _configuration = _module.ApplySettings(_configuration,
                _fieldControls.ToDictionary(item => item.Key, item => ReadValue(item.Value),
                    StringComparer.Ordinal));
        if (_inputSettings is not null) _configuration = _inputSettings.Apply(_configuration);
        if (_storageSettings is not null) _configuration = _storageSettings.Apply(_configuration);
    }
}
