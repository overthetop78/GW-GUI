using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

public sealed class AtariConfigurationCatalogSection : UserControl
{
    private readonly AtariConfigurationCatalogController _controller;
    private readonly ObservableCollection<AtariConfigurationItem> _configurations = [];
    private readonly IReadOnlyList<AtariModelItem> _models = AtariConfigurationCatalogFunctions.Models();
    private readonly ListBox _list = new() { MinWidth = 260, DisplayMemberPath = nameof(AtariConfigurationItem.DisplayName) };
    private readonly AtariGeneralSettingsSection _general = new();
    private Button? _save;
    private Button? _delete;
    private AtariMachineConfiguration? _current;
    private bool _loading;

    public AtariConfigurationCatalogSection() : this(new AtariConfigurationStore(
        StoragePaths.AtariConfigurationsDirectory, StoragePaths.DataDirectory)) { }

    internal AtariConfigurationCatalogSection(AtariConfigurationStore store)
    {
        _controller = new AtariConfigurationCatalogController(store);
        _list.ItemsSource = _configurations;
        _list.SelectionChanged += ConfigurationSelected;
        Content = BuildContent();
        Loaded += async (_, _) => await ReloadAsync();
    }

    public event EventHandler<AtariMachineConfiguration>? ConfigurationSaved;

    public void ConfigureActiveCheck(Func<Guid, bool>? isActive)
    {
        _controller.ConfigureActiveCheck(isActive);
        UpdateEditorAvailability();
    }

    public async Task ReloadAsync()
    {
        var selectedId = _current?.Id;
        var configurations = await _controller.LoadAsync();
        _loading = true;
        try
        {
            _configurations.Clear();
            foreach (var configuration in configurations)
            {
                var model = _models.Single(item => item.Model == configuration.Model);
                _configurations.Add(new AtariConfigurationItem(configuration,
                    AtariConfigurationCatalogFunctions.DisplayName(configuration, model.DisplayName)));
            }
            _list.SelectedItem = _configurations.FirstOrDefault(item => item.Configuration.Id == selectedId);
            if (_list.SelectedItem is null) _current = null;
        }
        finally
        {
            _loading = false;
            UpdateEditorAvailability();
        }
    }

    private UIElement BuildContent()
    {
        var root = new Grid { Margin = new Thickness(14) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(AtariConfigurationCatalogConstants.ConfigurationsDescriptionResource,
                AtariConfigurationCatalogConstants.AtariTitle),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        });
        var body = new Grid();
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
        body.ColumnDefinitions.Add(new ColumnDefinition());
        body.Children.Add(_list);
        Grid.SetColumn(_general, 1);
        body.Children.Add(_general);
        Grid.SetRow(body, 1);
        root.Children.Add(body);
        var actions = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
        actions.Children.Add(Button(AtariConfigurationCatalogConstants.NewResource, NewConfiguration));
        _save = Button(AtariConfigurationCatalogConstants.SaveResource, SaveConfiguration);
        _delete = Button(AtariConfigurationCatalogConstants.DeleteResource, DeleteConfiguration);
        actions.Children.Add(_save);
        actions.Children.Add(_delete);
        actions.Children.Add(Button(AtariConfigurationCatalogConstants.RefreshResource, async () => await ReloadAsync()));
        Grid.SetRow(actions, 2);
        root.Children.Add(actions);
        return root;
    }

    private static Button Button(string resource, Func<Task> action)
    {
        var button = new Button
        {
            Content = LocExtension.Get(resource), MinWidth = 110,
            Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(12, 7, 12, 7)
        };
        button.Click += async (_, _) => await ExecuteAsync(button, action);
        return button;
    }

    private static async Task ExecuteAsync(FrameworkElement owner, Func<Task> action)
    {
        try { await action(); }
        catch (Exception error) { ControlErrorPresenter.ShowUnexpected(owner, error,
            ControlErrorContexts.AtariConfiguration, AtariConfigurationCatalogConstants.AtariTitle); }
    }

    private async void ConfigurationSelected(object sender, SelectionChangedEventArgs args)
    {
        if (_loading || _list.SelectedItem is not AtariConfigurationItem selected) return;
        _current = selected.Configuration;
        await _general.LoadAsync(_current);
        UpdateEditorAvailability();
    }

    private async Task NewConfiguration()
    {
        _current = new AtariMachineConfiguration(AtariMachineModel.St);
        _list.SelectedItem = null;
        await _general.LoadAsync(_current);
        UpdateEditorAvailability();
    }

    private async Task SaveConfiguration()
    {
        if (_current is null || _controller.IsActive(_current.Id)) return;
        var configuration = _general.BuildConfiguration();
        await _controller.SaveAsync(configuration);
        _current = configuration;
        ConfigurationSaved?.Invoke(this, configuration);
        await ReloadAsync();
    }

    private async Task DeleteConfiguration()
    {
        if (_current is null) return;
        if (_controller.IsActive(_current.Id)) return;
        if (MessageBox.Show(Window.GetWindow(this),
                $"{LocExtension.Get(AtariConfigurationCatalogConstants.DeleteResource)}?",
                AtariConfigurationCatalogConstants.AtariTitle, MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _controller.Delete(_current.Id);
        _current = null;
        await ReloadAsync();
    }

    private void UpdateEditorAvailability()
    {
        var editable = _current is null || !_controller.IsActive(_current.Id);
        _general.IsEnabled = editable;
        if (_save is not null) _save.IsEnabled = editable;
        if (_delete is not null) _delete.IsEnabled = _current is not null && editable;
    }
}
