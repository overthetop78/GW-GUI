using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
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
    private readonly AtariHardwareSettingsSection _hardware;
    private Button? _new;
    private Button? _save;
    private Button? _delete;
    private Button? _refresh;
    private AtariMachineConfiguration? _current;
    private bool _loading;

    internal UIElement CatalogContent { get; }

    public AtariConfigurationCatalogSection() : this(new AtariConfigurationStore(
        StoragePaths.AtariConfigurationsDirectory, StoragePaths.DataDirectory)) { }

    internal AtariConfigurationCatalogSection(AtariConfigurationStore store)
    {
        _hardware = new AtariHardwareSettingsSection(_general);
        _general.ModelChanged += async (_, configuration) => await _hardware.LoadAsync(configuration);
        _controller = new AtariConfigurationCatalogController(store);
        _list.ItemsSource = _configurations;
        AtariAccessibilityFunctions.Configure(_list,
            LocExtension.Get(AtariAccessibilityConstants.ConfigurationListResource),
            tabIndex: AtariAccessibilityConstants.ConfigurationListTabIndex);
        _list.SelectionChanged += ConfigurationSelected;
        CatalogContent = BuildCatalogContent();
        Content = _hardware;
        AtariAccessibilityFunctions.ConfigureFlowDirection(this);
        PreviewKeyDown += CatalogPreviewKeyDown;
        Loaded += async (_, _) =>
        {
            await ReloadAsync();
            if (_configurations.Count > 0) _list.Focus();
            else _new?.Focus();
        };
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
        AtariMachineConfiguration? configurationToLoad;
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
            _list.SelectedItem = _configurations.FirstOrDefault(item => item.Configuration.Id == selectedId)
                ?? _configurations.FirstOrDefault();
            configurationToLoad = (_list.SelectedItem as AtariConfigurationItem)?.Configuration;
            _current = configurationToLoad;
        }
        finally
        {
            _loading = false;
        }

        if (configurationToLoad is null)
        {
            await NewConfiguration();
            return;
        }

        await _general.LoadAsync(configurationToLoad);
        await _hardware.LoadAsync(configurationToLoad);
        UpdateEditorAvailability();
    }

    private UIElement BuildCatalogContent()
    {
        var root = new Grid { Margin = new Thickness(14) };
        KeyboardNavigation.SetTabNavigation(root, KeyboardNavigationMode.Continue);
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
        AtariAccessibilityFunctions.ConfigureFlowDirection(root);
        Grid.SetRow(_list, 1);
        root.Children.Add(_list);
        var actions = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
        _new = Button(AtariConfigurationCatalogConstants.NewResource, NewConfiguration,
            AtariAccessibilityConstants.NewConfigurationAccelerator);
        actions.Children.Add(_new);
        _save = Button(AtariConfigurationCatalogConstants.SaveResource, SaveConfiguration);
        _delete = Button(AtariConfigurationCatalogConstants.DeleteResource, DeleteConfiguration,
            AtariAccessibilityConstants.DeleteConfigurationAccelerator);
        actions.Children.Add(_save);
        actions.Children.Add(_delete);
        AutomationProperties.SetAcceleratorKey(_save, AtariAccessibilityConstants.SaveConfigurationAccelerator);
        _refresh = Button(AtariConfigurationCatalogConstants.RefreshResource, async () => await ReloadAsync(),
            AtariAccessibilityConstants.RefreshConfigurationAccelerator);
        actions.Children.Add(_refresh);
        Grid.SetRow(actions, 2);
        root.Children.Add(actions);
        return root;
    }

    private static Button Button(string resource, Func<Task> action, string? accelerator = null)
    {
        var label = LocExtension.Get(resource);
        var button = new Button
        {
            Content = label, MinWidth = 110,
            Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(12, 7, 12, 7)
        };
        AtariAccessibilityFunctions.Configure(button, label);
        if (accelerator is not null) AutomationProperties.SetAcceleratorKey(button, accelerator);
        button.Click += async (_, _) => await ExecuteAsync(button, action);
        return button;
    }

    private async void CatalogPreviewKeyDown(object sender, KeyEventArgs args)
    {
        var control = Keyboard.Modifiers.HasFlag(AtariAccessibilityConstants.CommandModifier);
        if (control && args.Key == AtariAccessibilityConstants.NewConfigurationKey)
        {
            args.Handled = true;
            await ExecuteAsync((FrameworkElement?)_new ?? this, NewConfiguration);
        }
        else if (control && args.Key == AtariAccessibilityConstants.SaveConfigurationKey && _save?.IsEnabled == true)
        {
            args.Handled = true;
            await ExecuteAsync(_save, SaveConfiguration);
        }
        else if (args.Key == AtariAccessibilityConstants.RefreshConfigurationKey)
        {
            args.Handled = true;
            await ExecuteAsync((FrameworkElement?)_refresh ?? this, ReloadAsync);
        }
        else if (args.Key == AtariAccessibilityConstants.DeleteConfigurationKey
                 && _list.IsKeyboardFocusWithin && _delete?.IsEnabled == true)
        {
            args.Handled = true;
            await ExecuteAsync(_delete, DeleteConfiguration);
        }
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
        await _hardware.LoadAsync(_current);
        UpdateEditorAvailability();
    }

    private async Task NewConfiguration()
    {
        _current = new AtariMachineConfiguration(AtariMachineModel.St);
        _list.SelectedItem = null;
        await _general.LoadAsync(_current);
        await _hardware.LoadAsync(_current);
        UpdateEditorAvailability();
    }

    private async Task SaveConfiguration()
    {
        if (_current is null || _controller.IsActive(_current.Id)) return;
        var configuration = _hardware.Apply(_general.BuildConfiguration());
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
        _hardware.IsEnabled = editable;
        if (_save is not null) _save.IsEnabled = editable;
        if (_delete is not null) _delete.IsEnabled = _current is not null && editable;
    }
}
