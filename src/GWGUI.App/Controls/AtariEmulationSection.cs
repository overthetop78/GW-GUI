using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.IO;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

public sealed class AtariEmulationSection : UserControl
{
    private readonly AtariConfigurationStore _store = new(StoragePaths.AtariConfigurationsDirectory,
        StoragePaths.DataDirectory);
    private readonly ComboBox _configuration = new() { DisplayMemberPath = nameof(ConfigurationItem.DisplayName) };
    private readonly Button _open = new() { MinWidth = 130 };
    private readonly TabControl _machines = new();
    private readonly Dictionary<Guid, TabItem> _openMachines = [];
    private AppSettings _settings = new();

    public AtariEmulationSection()
    {
        AutomationProperties.SetName(_configuration, L(AtariEmulationConstants.ConfigurationResource));
        AutomationProperties.SetName(_open, L(AtariEmulationConstants.OpenResource));
        AutomationProperties.SetName(_machines, L(AtariEmulationConstants.MachinesAutomationResource));
        _open.Content = L(AtariEmulationConstants.OpenResource);
        _open.Click += OpenSelectedMachine;
        OptionsEmulationSection.AtariConfigurationSaved += ConfigurationSaved;
        Content = BuildContent();
        Loaded += async (_, _) => await ReloadConfigurationsAsync();
    }

    public void Configure(AppSettings settings) => _settings = settings;

    private UIElement BuildContent()
    {
        var root = new Grid { Margin = new Thickness(16) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var selector = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        selector.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        selector.ColumnDefinitions.Add(new ColumnDefinition());
        selector.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        selector.Children.Add(new TextBlock
        {
            Text = L(AtariEmulationConstants.ConfigurationResource),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 12, 5),
            FontWeight = FontWeights.SemiBold
        });
        _configuration.Margin = new Thickness(0, 4, 8, 4);
        Grid.SetColumn(_configuration, 1);
        selector.Children.Add(_configuration);
        _open.Margin = new Thickness(0, 4, 0, 4);
        Grid.SetColumn(_open, 2);
        selector.Children.Add(_open);
        var selectorCard = new Border { Child = selector };
        selectorCard.SetResourceReference(StyleProperty, AtariEmulationConstants.CardStyleResource);
        root.Children.Add(selectorCard);
        var welcome = new TabItem
        {
            Header = new MainTabHeader
            {
                Icon = AtariEmulationConstants.HomeGlyph,
                Text = L(AtariEmulationConstants.WelcomeTabResource)
            },
            Content = new TextBlock
            {
                Text = L(AtariEmulationConstants.WelcomeResource), TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 680, TextAlignment = TextAlignment.Center, FontSize = 18, Margin = new Thickness(32)
            },
            Padding = new Thickness(18, 9, 18, 9)
        };
        welcome.SetResourceReference(StyleProperty, AtariEmulationConstants.MainTabItemStyleResource);
        _machines.Items.Add(welcome);
        Grid.SetRow(_machines, 1);
        root.Children.Add(_machines);
        return root;
    }

    private async void ConfigurationSaved(object? sender, AtariMachineConfiguration configuration)
    {
        await ReloadConfigurationsAsync();
        if (_openMachines.TryGetValue(configuration.Id, out var tab) && tab.Content is AtariMachineView view)
            view.ApplyVideoRenderer(configuration.VideoRenderer);
    }

    public async Task ReloadConfigurationsAsync()
    {
        var selectedId = (_configuration.SelectedItem as ConfigurationItem)?.Configuration.Id;
        var modelNames = AtariConfigurationCatalogFunctions.Models().ToDictionary(item => item.Model);
        var configurations = await _store.LoadAllAsync();
        _configuration.ItemsSource = configurations.Select(configuration => new ConfigurationItem(configuration,
            AtariEmulationFunctions.DisplayName(configuration, modelNames[configuration.Model].DisplayName))).ToArray();
        _configuration.SelectedItem = _configuration.Items.OfType<ConfigurationItem>()
            .FirstOrDefault(item => item.Configuration.Id == selectedId)
            ?? _configuration.Items.OfType<ConfigurationItem>().FirstOrDefault();
        _open.IsEnabled = _configuration.SelectedItem is not null;
    }

    private async void OpenSelectedMachine(object sender, RoutedEventArgs args)
    {
        if (_configuration.SelectedItem is not ConfigurationItem selected) return;
        if (_openMachines.TryGetValue(selected.Configuration.Id, out var existing))
        {
            _machines.SelectedItem = existing;
            return;
        }
        try
        {
            _open.IsEnabled = false;
            AtariEmulationFunctions.ValidateConfiguration(selected.Configuration);
            var corePath = await AtariCoreProvider.GetInstalledPathAsync(selected.Configuration.Core);
            var engine = new AtariEngine(StoragePaths.AtariSessionsDirectory, corePath, Environment.ProcessPath!,
                () => new WasapiAudioOutput(),
                configuration => Path.Combine(StoragePaths.AtariStatesDirectory,
                    configuration.Id.ToString(AtariEmulationConstants.IdentifierFormat)));
            IAtariMachine CreateMachine() => engine.CreateAtariMachine(selected.Configuration);
            var machine = CreateMachine();
            var view = new AtariMachineView(machine, CreateMachine, selected.Configuration,
                _settings.EmulationShortcuts,
                AtariMachineViewFunctions.QuickStatePath(_settings.EmulationStateFolder,
                    selected.Configuration.Id),
                _settings.EmulationCaptureFolder);
            var tab = new TabItem { Content = view, Padding = new Thickness(18, 9, 14, 9) };
            tab.SetResourceReference(StyleProperty, AtariEmulationConstants.MainTabItemStyleResource);
            tab.Header = CreateMachineTabHeader(selected.DisplayName,
                () => CloseMachineAsync(selected.Configuration.Id, tab, view));
            _openMachines.Add(selected.Configuration.Id, tab);
            _machines.Items.Add(tab);
            _machines.SelectedItem = tab;
            try { await view.StartAsync(); }
            catch
            {
                await view.StopAsync();
                _openMachines.Remove(selected.Configuration.Id);
                _machines.Items.Remove(tab);
                throw;
            }
        }
        catch (Exception error)
        {
            ControlErrorPresenter.ShowDetailed(this, error,
                AtariErrorLocalizationFunctions.Describe(error),
                AtariEmulationConstants.ConfigurationOpeningContext, AtariEmulationConstants.AtariTitle);
        }
        finally { _open.IsEnabled = _configuration.SelectedItem is not null; }
    }

    private async Task CloseMachineAsync(Guid configurationId, TabItem tab, AtariMachineView view)
    {
        if (!_openMachines.ContainsKey(configurationId)) return;
        await view.StopAsync();
        _openMachines.Remove(configurationId);
        _machines.Items.Remove(tab);
    }

    private static FrameworkElement CreateMachineTabHeader(string title, Func<Task> close)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(new TextBlock
        {
            Text = ControlVisualConstants.GameControllerGlyph, FontFamily = ControlVisualConstants.IconFont,
            FontSize = 16, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 7, 0)
        });
        panel.Children.Add(new TextBlock
        {
            Text = title, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 7, 0)
        });
        var button = new Button
        {
            Content = new TextBlock
            {
                Text = ControlVisualConstants.CloseGlyph, FontFamily = ControlVisualConstants.IconFont,
                FontSize = 9, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            },
            ToolTip = L(AtariEmulationConstants.CloseResource), Width = 18, Height = 18,
            MinWidth = 0, MinHeight = 0, Padding = new Thickness(0), Margin = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center
        };
        button.SetResourceReference(StyleProperty, AtariEmulationConstants.StatusIconButtonStyleResource);
        button.Click += async (_, args) =>
        {
            args.Handled = true;
            await ButtonAsyncAction.RunAsync(button, close);
        };
        panel.Children.Add(button);
        return panel;
    }

    public async Task StopAllAsync()
    {
        foreach (var tab in _openMachines.Values.ToArray())
            if (tab.Content is AtariMachineView view) await view.StopAsync();
        _openMachines.Clear();
    }

    private static string L(string key) => LocExtension.Get(key);
    private sealed record ConfigurationItem(AtariMachineConfiguration Configuration, string DisplayName);
}
