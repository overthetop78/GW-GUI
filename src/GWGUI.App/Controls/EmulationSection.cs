using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

public sealed class EmulationSection : UserControl
{
    private readonly ComboBox _configuration = new() { DisplayMemberPath = nameof(ConfigurationItem.DisplayName) };
    private readonly Button _open = new() { MinWidth = 130 };
    private readonly TabControl _machines = new();
    private readonly Dictionary<(MachineFamily Family, Guid Id), TabItem> _openMachines = [];
    private AppSettings _settings = new();

    public EmulationSection()
    {
        AutomationProperties.SetName(_configuration, LocExtension.Get("Emulation.Configuration"));
        AutomationProperties.SetName(_open, LocExtension.Get("Emulation.Machine.Open"));
        AutomationProperties.SetName(_machines, LocExtension.Get("Emulation.Tab.Machines"));
        _open.Content = LocExtension.Get("Emulation.Machine.Open");
        _open.Click += OpenSelectedMachine;
        OptionsEmulationSection.ConfigurationSaved += AmigaConfigurationSaved;
        OptionsEmulationSection.AtariConfigurationSaved += AtariConfigurationSaved;
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
            Text = LocExtension.Get("Emulation.Configuration"), VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 12, 5), FontWeight = FontWeights.SemiBold
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
                Text = LocExtension.Get(AtariEmulationConstants.WelcomeTabResource)
            },
            Content = new TextBlock
            {
                Text = LocExtension.Get(AtariEmulationConstants.WelcomeResource), TextWrapping = TextWrapping.Wrap,
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

    private async void AmigaConfigurationSaved(object? sender, AmigaMachineConfiguration configuration)
    {
        await ReloadConfigurationsAsync();
        if (_openMachines.TryGetValue((MachineFamily.Amiga, configuration.Id), out var tab)
            && tab.Content is AmigaMachineView view) view.ApplyVideoRenderer(configuration.VideoRenderer);
    }

    private async void AtariConfigurationSaved(object? sender, AtariMachineConfiguration configuration)
    {
        await ReloadConfigurationsAsync();
        if (_openMachines.TryGetValue((MachineFamily.Atari, configuration.Id), out var tab)
            && tab.Content is AtariMachineView view) view.ApplyVideoRenderer(configuration.VideoRenderer);
    }

    public async Task ReloadConfigurationsAsync()
    {
        var selected = _configuration.SelectedItem as ConfigurationItem;
        var amiga = await new AmigaConfigurationStore(StoragePaths.AmigaConfigurationsDirectory,
            StoragePaths.DataDirectory).LoadAllAsync();
        var atari = await new AtariConfigurationStore(StoragePaths.AtariConfigurationsDirectory,
            StoragePaths.DataDirectory).LoadAllAsync();
        var atariModels = AtariConfigurationCatalogFunctions.Models().ToDictionary(item => item.Model);
        _configuration.ItemsSource = amiga.Select(configuration => new ConfigurationItem(
                MachineFamily.Amiga, configuration.Id,
                EmulationConfigurationDisplayFunctions.Amiga(configuration), configuration, null))
            .Concat(atari.Select(configuration => new ConfigurationItem(
                MachineFamily.Atari, configuration.Id,
                AtariEmulationFunctions.DisplayName(configuration, atariModels[configuration.Model].DisplayName),
                null, configuration))).ToArray();
        _configuration.SelectedItem = _configuration.Items.OfType<ConfigurationItem>()
            .FirstOrDefault(item => item.Family == selected?.Family && item.Id == selected.Id)
            ?? _configuration.Items.OfType<ConfigurationItem>().FirstOrDefault();
        _open.IsEnabled = _configuration.SelectedItem is not null;
    }

    private async void OpenSelectedMachine(object sender, RoutedEventArgs args)
    {
        if (_configuration.SelectedItem is not ConfigurationItem selected) return;
        var key = (selected.Family, selected.Id);
        if (_openMachines.TryGetValue(key, out var existing))
        {
            _machines.SelectedItem = existing;
            return;
        }
        try
        {
            _open.IsEnabled = false;
            if (selected.Amiga is not null) await OpenAmigaAsync(selected, selected.Amiga);
            else if (selected.Atari is not null) await OpenAtariAsync(selected, selected.Atari);
        }
        catch (Exception error)
        {
            var isAtari = selected.Family == MachineFamily.Atari;
            if (isAtari)
                ControlErrorPresenter.ShowDetailed(this, error,
                    AtariErrorLocalizationFunctions.Describe(error),
                    AtariEmulationConstants.ConfigurationOpeningContext,
                    AtariEmulationConstants.AtariTitle);
            else
                ControlErrorPresenter.ShowUnexpected(this, error,
                    ControlErrorContexts.AmigaConfigurationOpening, ControlVisualConstants.AmigaTitle);
        }
        finally { _open.IsEnabled = _configuration.SelectedItem is not null; }
    }

    private async Task OpenAmigaAsync(ConfigurationItem selected, AmigaMachineConfiguration configuration)
    {
        ValidateAmigaConfiguration(configuration);
        var runtime = await AmigaRuntimeMedia.PrepareConfigurationAsync(configuration);
        var corePath = await AmigaCoreProvider.EnsureAvailableAsync();
        var audio = configuration.Audio ?? new AmigaAudioConfiguration();
        var engine = new AmigaEngine(StoragePaths.AmigaSessionsDirectory, corePath,
            () => new WasapiAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(StoragePaths.AmigaConfigurationsDirectory, value.Id.ToString("N"), "Saves"),
            Environment.ProcessPath);
        IAmigaMachine CreateMachine() => engine.CreateAmigaMachine(runtime);
        var view = new AmigaMachineView(CreateMachine(), CreateMachine, runtime, configuration.Input,
            _settings.EmulationShortcuts,
            Path.Combine(_settings.EmulationStateFolder, $"amiga-{configuration.Id:N}.gwas"),
            _settings.EmulationCaptureFolder);
        await AddMachineAsync(selected, view, view.StartAsync, view.StopAsync);
    }

    private async Task OpenAtariAsync(ConfigurationItem selected, AtariMachineConfiguration configuration)
    {
        AtariEmulationFunctions.ValidateConfiguration(configuration);
        var corePath = await AtariCoreProvider.GetInstalledPathAsync(configuration.Core);
        var engine = new AtariEngine(StoragePaths.AtariSessionsDirectory, corePath, Environment.ProcessPath!,
            () => new WasapiAudioOutput(), value => Path.Combine(StoragePaths.AtariStatesDirectory,
                value.Id.ToString(AtariEmulationConstants.IdentifierFormat)));
        IAtariMachine CreateMachine() => engine.CreateAtariMachine(configuration);
        var view = new AtariMachineView(CreateMachine(), CreateMachine, configuration,
            _settings.EmulationShortcuts,
            AtariMachineViewFunctions.QuickStatePath(_settings.EmulationStateFolder, configuration.Id),
            _settings.EmulationCaptureFolder);
        await AddMachineAsync(selected, view, view.StartAsync, view.StopAsync);
    }

    private async Task AddMachineAsync(ConfigurationItem selected, FrameworkElement view,
        Func<Task> start, Func<Task> stop)
    {
        var key = (selected.Family, selected.Id);
        var tab = new TabItem { Content = view, Padding = new Thickness(18, 9, 14, 9) };
        tab.SetResourceReference(StyleProperty, AtariEmulationConstants.MainTabItemStyleResource);
        tab.Header = CreateMachineTabHeader(selected.DisplayName, () => CloseMachineAsync(key, tab, stop));
        _openMachines.Add(key, tab);
        _machines.Items.Add(tab);
        _machines.SelectedItem = tab;
        try { await start(); }
        catch
        {
            await stop();
            _openMachines.Remove(key);
            _machines.Items.Remove(tab);
            throw;
        }
    }

    private async Task CloseMachineAsync((MachineFamily Family, Guid Id) key, TabItem tab, Func<Task> stop)
    {
        if (!_openMachines.ContainsKey(key)) return;
        await stop();
        _openMachines.Remove(key);
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
                Text = ControlVisualConstants.CloseGlyph, FontFamily = ControlVisualConstants.IconFont, FontSize = 9
            },
            ToolTip = LocExtension.Get(AtariEmulationConstants.CloseResource), Width = 18, Height = 18,
            MinWidth = 0, MinHeight = 0, Padding = new Thickness(0), Margin = new Thickness(0)
        };
        button.SetResourceReference(StyleProperty, AtariEmulationConstants.StatusIconButtonStyleResource);
        button.Click += async (_, eventArgs) =>
        {
            eventArgs.Handled = true;
            await ButtonAsyncAction.RunAsync(button, close);
        };
        panel.Children.Add(button);
        return panel;
    }

    private static void ValidateAmigaConfiguration(AmigaMachineConfiguration configuration)
    {
        if (!File.Exists(configuration.KickstartPath))
            throw new FileNotFoundException("Kickstart", configuration.KickstartPath);
        var media = configuration.Media?.FirstOrDefault()?.Path ?? configuration.InitialDiskPath;
        if (!string.IsNullOrWhiteSpace(media) && !File.Exists(media) && !Directory.Exists(media))
            throw new FileNotFoundException("Amiga media", media);
    }

    public async Task StopAllAsync()
    {
        foreach (var tab in _openMachines.Values.ToArray())
        {
            if (tab.Content is AmigaMachineView amiga) await amiga.StopAsync();
            else if (tab.Content is AtariMachineView atari) await atari.StopAsync();
        }
        _openMachines.Clear();
    }

    private enum MachineFamily { Amiga, Atari }

    private sealed record ConfigurationItem(MachineFamily Family, Guid Id, string DisplayName,
        AmigaMachineConfiguration? Amiga, AtariMachineConfiguration? Atari);
}
