using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed class AtariGeneralSettingsSection : UserControl
{
    private readonly IReadOnlyList<AtariModelItem> _models = AtariConfigurationCatalogFunctions.Models();
    private readonly ComboBox _model = new() { MinWidth = 260 };
    private readonly TextBlock _error = new() { TextWrapping = TextWrapping.Wrap, Visibility = Visibility.Collapsed };
    private readonly AtariCoreManagementSection _coreManagement = new();
    private Border _foldersCard = null!;
    private readonly Dictionary<string, TextBox> _folders = new(StringComparer.Ordinal);
    private AtariMachineConfiguration? _configuration;
    private bool _loading;

    internal AtariGeneralSettingsSection()
    {
        AtariAccessibilityFunctions.Configure(_model,
            L(AtariConfigurationCatalogConstants.ModelResource), tabIndex: AtariAccessibilityConstants.ModelTabIndex);
        AtariAccessibilityFunctions.Configure(_error, AtariConfigurationCatalogConstants.AtariTitle);
        _model.ItemsSource = _models;
        _model.DisplayMemberPath = nameof(AtariModelItem.DisplayName);
        _model.SelectionChanged += async (_, _) => await ModelChangedAsync();
        Content = BuildContent();
    }

    internal event EventHandler? Changed;
    internal event EventHandler<AtariMachineConfiguration>? ModelChanged;
    internal event EventHandler? SaveRequested;

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
            HardDisks = AtariGeneralSettingsFunctions.SupportsHardDiskFolder(selected.Model)
                ? Folder(nameof(AtariFolderConfiguration.HardDisks))
                : existingFolders.HardDisks
        };
        return AtariGeneralSettingsFunctions.ReplaceGeneral(_configuration, selected.Model, folders,
            _configuration.Firmwares, _configuration.Options);
    }

    private UIElement BuildContent()
    {
        var root = new StackPanel { Margin = new Thickness(12) };
        var configuration = new Grid { Margin = new Thickness(14, 10, 14, 10) };
        configuration.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        configuration.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
        configuration.ColumnDefinitions.Add(new ColumnDefinition());
        configuration.Children.Add(new TextBlock
        {
            Text = L(AtariConfigurationCatalogConstants.ModelResource),
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        });
        _model.Height = 36;
        _model.Margin = new Thickness(0);
        Grid.SetColumn(_model, 1);
        configuration.Children.Add(_model);
        var save = new Button
        {
            Content = L(AtariConfigurationCatalogConstants.SaveResource),
            MinWidth = 110,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        save.Click += (_, _) => SaveRequested?.Invoke(this, EventArgs.Empty);
        Grid.SetColumn(save, 2);
        configuration.Children.Add(save);
        var configurationCard = new Border
        {
            Child = configuration,
            CornerRadius = new CornerRadius(9),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 12)
        };
        ControlUiFactory.ApplyCardAppearance(configurationCard);
        root.Children.Add(configurationCard);
        root.Children.Add(_error);
        _coreManagement.Margin = new Thickness(0, 0, 0, 12);
        root.Children.Add(_coreManagement);
        _foldersCard = BuildFoldersCard();
        root.Children.Add(_foldersCard);
        return EmulationSettingsLayout.ScrollPage(root);
    }

    private Border BuildFoldersCard()
    {
        var key = nameof(AtariFolderConfiguration.HardDisks);
        var label = L(AtariGeneralSettingsConstants.HardDisksResource);
        var value = new TextBox();
        AtariAccessibilityFunctions.Configure(value, label);
        value.TextChanged += (_, _) => { if (!_loading) Changed?.Invoke(this, EventArgs.Empty); };
        _folders[key] = value;
        return EmulationSettingsLayout.DefaultFoldersCard(
            L(AtariGeneralSettingsConstants.FoldersResource),
            new EmulationDefaultFolderRow(label, value, () => BrowseFolderAsync(value)));
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
        _foldersCard.Visibility = AtariGeneralSettingsFunctions.SupportsHardDiskFolder(model)
            ? Visibility.Visible : Visibility.Collapsed;
        try
        {
            await _coreManagement.SetModelAsync(model);
        }
        catch (Exception error)
        {
            _error.Text = ControlErrorPresenter.Describe(error, ControlErrorContexts.AtariConfiguration);
            _error.Visibility = Visibility.Visible;
        }
    }

    private void LoadFolders(AtariFolderConfiguration value)
    {
        _folders[nameof(value.HardDisks)].Text = value.HardDisks ?? string.Empty;
    }

    private static Task BrowseFolderAsync(TextBox target)
    {
        var dialog = new OpenFolderDialog { Title = AtariGeneralSettingsConstants.FolderDialogDescription, InitialDirectory = target.Text };
        if (dialog.ShowDialog() == true) target.Text = dialog.FolderName;
        return Task.CompletedTask;
    }

    private string Folder(string name) => _folders[name].Text;
    private static string L(string resource) => LocExtension.Get(resource);
}
