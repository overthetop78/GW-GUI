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
    private readonly StackPanel _options = new();
    private readonly GroupBox _optionsGroup;
    private readonly Dictionary<string, TextBox> _folders = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ComboBox> _optionEditors = new(StringComparer.Ordinal);
    private readonly AtariCoreReleaseService _releaseService = new(Client, StoragePaths.AtariCoreDirectory);
    private AtariMachineConfiguration? _configuration;
    private bool _loading;

    internal AtariGeneralSettingsSection()
    {
        _optionsGroup = Group(AtariGeneralSettingsConstants.CoreOptionsResource, _options);
        _optionsGroup.Visibility = Visibility.Collapsed;
        AtariAccessibilityFunctions.Configure(_model,
            L(AtariConfigurationCatalogConstants.ModelResource), tabIndex: AtariAccessibilityConstants.ModelTabIndex);
        AtariAccessibilityFunctions.Configure(_core, L(AtariGeneralSettingsConstants.CoreResource));
        AtariAccessibilityFunctions.Configure(_error, AtariConfigurationCatalogConstants.AtariTitle);
        _model.ItemsSource = _models;
        _model.DisplayMemberPath = nameof(AtariModelItem.DisplayName);
        _model.SelectionChanged += async (_, _) => await ModelChangedAsync();
        _coreManagement.InstallationChanged += async (_, _) => await LoadCoreOptionsAsync();
        Content = BuildContent();
    }

    internal event EventHandler? Changed;
    internal event EventHandler<AtariMachineConfiguration>? ModelChanged;

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
        var existingFolders = _configuration.Folders;
        var folders = existingFolders with
        {
            HardDisks = Folder(nameof(AtariFolderConfiguration.HardDisks))
        };
        var displayed = _optionEditors.Where(item => item.Value.SelectedValue is string)
            .Select(item => KeyValuePair.Create(item.Key, (string)item.Value.SelectedValue));
        var options = AtariGeneralSettingsFunctions.MergeOptions(_configuration.Options, displayed);
        return AtariGeneralSettingsFunctions.ReplaceGeneral(_configuration, selected.Model, folders,
            _configuration.Firmwares, options);
    }

    private UIElement BuildContent()
    {
        var root = new StackPanel();
        root.Children.Add(Label(AtariConfigurationCatalogConstants.ModelResource));
        root.Children.Add(_model);
        root.Children.Add(_error);
        root.Children.Add(Label(AtariGeneralSettingsConstants.CoreResource));
        root.Children.Add(_core);
        root.Children.Add(_coreManagement);
        root.Children.Add(Group(AtariGeneralSettingsConstants.FoldersResource, BuildFolders()));
        root.Children.Add(_optionsGroup);
        return root;
    }

    private UIElement BuildFolders()
    {
        var panel = new StackPanel();
        AddFolder(panel, nameof(AtariFolderConfiguration.HardDisks), AtariGeneralSettingsConstants.HardDisksResource);
        return panel;
    }

    private async Task ModelChangedAsync()
    {
        if (_loading || _model.SelectedItem is not AtariModelItem selected || _configuration is null) return;
        _configuration = AtariConfigurationCatalogFunctions.ChangeModel(_configuration, selected.Model);
        await RefreshModelAsync(selected.Model);
        ModelChanged?.Invoke(this, _configuration);
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
            await LoadCoreOptionsAsync();
        }
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
        _optionsGroup.Visibility = Visibility.Collapsed;
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
            AtariAccessibilityFunctions.Configure(editor, AtariGeneralSettingsFunctions.OptionHeading(option),
                option.CategorizedDescription ?? option.Description);
            editor.SelectedValue = _configuration.Options.TryGetValue(option.Key, out var value) ? value : option.CurrentValue;
            editor.SelectionChanged += (_, _) => { if (!_loading) Changed?.Invoke(this, EventArgs.Empty); };
            _options.Children.Add(label);
            _options.Children.Add(editor);
            _optionEditors[option.Key] = editor;
        }
        _optionsGroup.Visibility = _optionEditors.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void AddFolder(Panel panel, string key, string resource)
    {
        var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var label = L(resource);
        row.Children.Add(new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        var value = new TextBox();
        AtariAccessibilityFunctions.Configure(value, label);
        value.TextChanged += (_, _) => { if (!_loading) Changed?.Invoke(this, EventArgs.Empty); };
        _folders[key] = value;
        Grid.SetColumn(value, 1);
        row.Children.Add(value);
        var browse = new Button { Content = L(AtariGeneralSettingsConstants.BrowseResource), Margin = new Thickness(6, 0, 0, 0) };
        AtariAccessibilityFunctions.Configure(browse,
            $"{L(AtariGeneralSettingsConstants.BrowseResource)} — {label}");
        browse.Click += (_, _) => BrowseFolder(value);
        Grid.SetColumn(browse, 2);
        row.Children.Add(browse);
        panel.Children.Add(row);
    }

    private void LoadFolders(AtariFolderConfiguration value)
    {
        _folders[nameof(value.HardDisks)].Text = value.HardDisks ?? string.Empty;
    }

    private static void BrowseFolder(TextBox target)
    {
        var dialog = new OpenFolderDialog { Title = AtariGeneralSettingsConstants.FolderDialogDescription, InitialDirectory = target.Text };
        if (dialog.ShowDialog() == true) target.Text = dialog.FolderName;
    }

    private string Folder(string name) => _folders[name].Text;
    private static GroupBox Group(string resource, UIElement content) => new() { Header = L(resource), Content = content, Margin = new Thickness(0, 10, 0, 0) };
    private static TextBlock Label(string resource) => new() { Text = L(resource), Margin = new Thickness(0, 7, 0, 4) };
    private static string L(string resource) => LocExtension.Get(resource);
}
