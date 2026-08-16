using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.App.Controls;

internal sealed class AtariGeneralSettingsSection : UserControl
{
    private static readonly HttpClient Client = new();
    private readonly IReadOnlyList<AtariModelItem> _models = AtariConfigurationCatalogFunctions.Models();
    private readonly ComboBox _model = new() { MinWidth = 260 };
    private readonly TextBlock _core = new();
    private readonly TextBlock _error = new() { TextWrapping = TextWrapping.Wrap, Visibility = Visibility.Collapsed };
    private readonly AtariCoreManagementSection _coreManagement = new();
    private readonly StackPanel _firmware = new();
    private readonly StackPanel _options = new();
    private readonly Dictionary<string, TextBox> _folders = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ComboBox> _optionEditors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CheckBox> _firmwareEditors = new(StringComparer.OrdinalIgnoreCase);
    private readonly ObservableCollection<AtariScannedFirmware> _firmwareItems = [];
    private readonly AtariCoreReleaseService _releaseService = new(Client, StoragePaths.AtariCoreDirectory);
    private AtariMachineConfiguration? _configuration;
    private bool _loading;

    internal AtariGeneralSettingsSection()
    {
        _model.ItemsSource = _models;
        _model.DisplayMemberPath = nameof(AtariModelItem.DisplayName);
        _model.SelectionChanged += async (_, _) => await ModelChangedAsync();
        _coreManagement.InstallationChanged += async (_, _) => await LoadCoreOptionsAsync();
        Content = BuildContent();
    }

    internal event EventHandler? Changed;

    internal async Task LoadAsync(AtariMachineConfiguration configuration)
    {
        _loading = true;
        _configuration = configuration;
        _model.SelectedItem = _models.Single(item => item.Model == configuration.Model);
        LoadFolders(AtariGeneralSettingsFunctions.CompleteFolders(configuration.Folders));
        _loading = false;
        await RefreshModelAsync(configuration.Model);
    }

    internal AtariMachineConfiguration BuildConfiguration()
    {
        if (_configuration is null || _model.SelectedItem is not AtariModelItem selected)
            throw new InvalidOperationException(nameof(AtariGeneralSettingsSection));
        var folders = new AtariFolderConfiguration(
            Folder(nameof(AtariFolderConfiguration.Shared)), Folder(nameof(AtariFolderConfiguration.Floppies)),
            Folder(nameof(AtariFolderConfiguration.Cassettes)), Folder(nameof(AtariFolderConfiguration.Cartridges)),
            Folder(nameof(AtariFolderConfiguration.CompactDiscs)), Folder(nameof(AtariFolderConfiguration.HardDisks)),
            Folder(nameof(AtariFolderConfiguration.States)), Folder(nameof(AtariFolderConfiguration.Captures)));
        var displayed = _optionEditors.Where(item => item.Value.SelectedValue is string)
            .Select(item => KeyValuePair.Create(item.Key, (string)item.Value.SelectedValue));
        var options = AtariGeneralSettingsFunctions.MergeOptions(_configuration.Options, displayed);
        var firmwares = _firmwareItems.Where(item => item.Definition?.Kind is not null
                && item.Compatibility != AtariFirmwareCompatibility.Incompatible)
            .Where(item => _firmwareEditors.TryGetValue(item.Path, out var editor) && editor.IsChecked == true)
            .GroupBy(item => item.Definition!.Kind)
            .Select(group => AtariGeneralSettingsFunctions.FirmwareConfiguration(group.First()))
            .ToArray();
        return AtariGeneralSettingsFunctions.ReplaceGeneral(_configuration, selected.Model, folders, firmwares, options);
    }

    private UIElement BuildContent()
    {
        var root = new StackPanel { Margin = new Thickness(18, 0, 0, 0) };
        root.Children.Add(Label(AtariConfigurationCatalogConstants.ModelResource));
        root.Children.Add(_model);
        root.Children.Add(_error);
        root.Children.Add(Label(AtariGeneralSettingsConstants.CoreResource));
        root.Children.Add(_core);
        root.Children.Add(_coreManagement);
        root.Children.Add(Group(AtariGeneralSettingsConstants.FoldersResource, BuildFolders()));
        root.Children.Add(Group(AtariGeneralSettingsConstants.FirmwareResource, BuildFirmware()));
        root.Children.Add(Group(AtariGeneralSettingsConstants.CoreOptionsResource, _options));
        return new ScrollViewer { Content = root, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    private UIElement BuildFolders()
    {
        var panel = new StackPanel();
        AddFolder(panel, nameof(AtariFolderConfiguration.Shared), AtariGeneralSettingsConstants.SharedFolderResource);
        AddFolder(panel, nameof(AtariFolderConfiguration.Floppies), AtariGeneralSettingsConstants.FloppiesResource);
        AddFolder(panel, nameof(AtariFolderConfiguration.Cassettes), AtariGeneralSettingsConstants.CassettesResource);
        AddFolder(panel, nameof(AtariFolderConfiguration.Cartridges), AtariGeneralSettingsConstants.CartridgesResource);
        AddFolder(panel, nameof(AtariFolderConfiguration.CompactDiscs), AtariGeneralSettingsConstants.CompactDiscsResource);
        AddFolder(panel, nameof(AtariFolderConfiguration.HardDisks), AtariGeneralSettingsConstants.HardDisksResource);
        AddFolder(panel, nameof(AtariFolderConfiguration.States), AtariGeneralSettingsConstants.StatesResource);
        AddFolder(panel, nameof(AtariFolderConfiguration.Captures), AtariGeneralSettingsConstants.CapturesResource);
        return panel;
    }

    private UIElement BuildFirmware()
    {
        var root = new StackPanel();
        var refresh = new Button { Content = L(AtariGeneralSettingsConstants.RefreshResource), HorizontalAlignment = HorizontalAlignment.Left };
        refresh.Click += async (_, _) => await RefreshFirmwareAsync();
        root.Children.Add(refresh);
        root.Children.Add(_firmware);
        return root;
    }

    private async Task ModelChangedAsync()
    {
        if (_loading || _model.SelectedItem is not AtariModelItem selected || _configuration is null) return;
        _configuration = AtariConfigurationCatalogFunctions.ChangeModel(_configuration, selected.Model);
        await RefreshModelAsync(selected.Model);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private async Task RefreshModelAsync(AtariMachineModel model)
    {
        _error.Visibility = Visibility.Collapsed;
        try
        {
            var definition = AtariCompatibilityCatalog.Get(model);
            _core.Text = AtariCoreCatalog.Get(definition.Core).LibraryName;
            await _coreManagement.SetModelAsync(model);
            await ScanFirmwareAsync();
            await LoadCoreOptionsAsync();
        }
        catch (Exception error)
        {
            _error.Text = ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariConfiguration);
            _error.Visibility = Visibility.Visible;
        }
    }

    private async Task ScanFirmwareAsync()
    {
        if (_model.SelectedItem is not AtariModelItem selected) return;
        _firmwareItems.Clear();
        _firmware.Children.Clear();
        _firmwareEditors.Clear();
        var expected = AtariFirmwareCatalog.ForModel(selected.Model);
        if (expected.All(item => item.Kind is null || item.Provision == AtariFirmwareProvision.NotUsed))
        {
            _firmware.Children.Add(new TextBlock { Text = L(AtariGeneralSettingsConstants.NoFirmwareResource) });
            return;
        }
        var scanner = new AtariFirmwareScanner(StoragePaths.AtariFirmwareDirectory);
        var scanned = await scanner.ScanAsync(selected.Model);
        foreach (var definition in expected.Where(item => item.Kind is not null && item.ExpectedFileName is not null))
        {
            if (scanned.Any(item => item.Definition?.Id == definition.Id)) continue;
            _firmware.Children.Add(new CheckBox
            {
                Content = definition.ExpectedFileName,
                IsEnabled = false,
                IsChecked = false
            });
        }
        foreach (var item in scanned)
        {
            _firmwareItems.Add(item);
            var configured = _configuration?.Firmwares.Any(value =>
                value.Path.Equals(item.Path, StringComparison.OrdinalIgnoreCase)) == true;
            var editor = new CheckBox
            {
                Content = FirmwareText(item),
                IsChecked = configured,
                IsEnabled = item.Definition?.Kind is not null
                    && item.Compatibility != AtariFirmwareCompatibility.Incompatible
            };
            editor.Checked += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
            editor.Unchecked += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
            _firmwareEditors[item.Path] = editor;
            _firmware.Children.Add(editor);
        }
    }

    private async Task RefreshFirmwareAsync()
    {
        _error.Visibility = Visibility.Collapsed;
        try { await ScanFirmwareAsync(); }
        catch (Exception error)
        {
            _error.Text = ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariConfiguration);
            _error.Visibility = Visibility.Visible;
        }
    }

    private async Task LoadCoreOptionsAsync()
    {
        _options.Children.Clear();
        _optionEditors.Clear();
        if (_configuration is null || _model.SelectedItem is not AtariModelItem selected) return;
        var paths = await _releaseService.GetActiveInstallationAsync(AtariCompatibilityCatalog.Get(selected.Model).Core);
        if (paths is null) return;
        IReadOnlyList<AtariCoreOption> options;
        try { options = AtariCoreOptionProbe.Inspect(paths.LibraryPath, selected.Model == _configuration.Model ? _configuration.Core : AtariCompatibilityCatalog.Get(selected.Model).Core); }
        catch (Exception error)
        {
            _options.Children.Add(new TextBlock
            {
                Text = ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariCoreOptions),
                TextWrapping = TextWrapping.Wrap
            });
            return;
        }
        foreach (var option in options.Where(item => item.IsVisible))
        {
            var label = new TextBlock
            {
                Text = AtariGeneralSettingsFunctions.OptionHeading(option),
                ToolTip = option.CategorizedDescription ?? option.Description
            };
            var editor = new ComboBox { ItemsSource = option.Values, DisplayMemberPath = nameof(AtariCoreOptionValue.Label), SelectedValuePath = nameof(AtariCoreOptionValue.Value) };
            editor.SelectedValue = _configuration.Options.TryGetValue(option.Key, out var value) ? value : option.CurrentValue;
            editor.SelectionChanged += (_, _) => { if (!_loading) Changed?.Invoke(this, EventArgs.Empty); };
            _options.Children.Add(label);
            _options.Children.Add(editor);
            _optionEditors[option.Key] = editor;
        }
    }

    private void AddFolder(Panel panel, string key, string resource)
    {
        var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(new TextBlock { Text = L(resource), VerticalAlignment = VerticalAlignment.Center });
        var value = new TextBox();
        value.TextChanged += (_, _) => { if (!_loading) Changed?.Invoke(this, EventArgs.Empty); };
        _folders[key] = value;
        Grid.SetColumn(value, 1);
        row.Children.Add(value);
        var browse = new Button { Content = L(AtariGeneralSettingsConstants.BrowseResource), Margin = new Thickness(6, 0, 0, 0) };
        browse.Click += (_, _) => BrowseFolder(value);
        Grid.SetColumn(browse, 2);
        row.Children.Add(browse);
        panel.Children.Add(row);
    }

    private void LoadFolders(AtariFolderConfiguration value)
    {
        _folders[nameof(value.Shared)].Text = value.Shared ?? string.Empty;
        _folders[nameof(value.Floppies)].Text = value.Floppies ?? string.Empty;
        _folders[nameof(value.Cassettes)].Text = value.Cassettes ?? string.Empty;
        _folders[nameof(value.Cartridges)].Text = value.Cartridges ?? string.Empty;
        _folders[nameof(value.CompactDiscs)].Text = value.CompactDiscs ?? string.Empty;
        _folders[nameof(value.HardDisks)].Text = value.HardDisks ?? string.Empty;
        _folders[nameof(value.States)].Text = value.States ?? string.Empty;
        _folders[nameof(value.Captures)].Text = value.Captures ?? string.Empty;
    }

    private static void BrowseFolder(TextBox target)
    {
        var dialog = new OpenFolderDialog { Title = AtariGeneralSettingsConstants.FolderDialogDescription, InitialDirectory = target.Text };
        if (dialog.ShowDialog() == true) target.Text = dialog.FolderName;
    }

    private string FirmwareText(AtariScannedFirmware item) =>
        (item.Definition?.Version ?? L(AtariGeneralSettingsConstants.UnknownFirmwareResource))
        + AtariGeneralSettingsConstants.FirmwareDetailSeparator
        + item.Compatibility;

    private string Folder(string name) => _folders[name].Text;
    private static GroupBox Group(string resource, UIElement content) => new() { Header = L(resource), Content = content, Margin = new Thickness(0, 10, 0, 0) };
    private static TextBlock Label(string resource) => new() { Text = L(resource), Margin = new Thickness(0, 7, 0, 4) };
    private static string L(string resource) => LocExtension.Get(resource);
}
