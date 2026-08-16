using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Controls;

public sealed class AmigaEmulationSection : UserControl
{
    private readonly ComboBox _configuration = new() { DisplayMemberPath = nameof(ConfigurationItem.DisplayName) };
    private readonly Button _open = new() { MinWidth = 130 };
    private readonly TabControl _machines = new();
    private readonly Dictionary<Guid, TabItem> _openMachines = [];
    private AppSettings _settings = new();

    public AmigaEmulationSection()
    {
        AutomationProperties.SetName(_configuration, LocExtension.Get("Emulation.Configuration"));
        AutomationProperties.SetName(_open, LocExtension.Get("Emulation.OpenMachine"));
        AutomationProperties.SetName(_machines, LocExtension.Get("Emulation.MachinesTab"));
        _open.Content = LocExtension.Get("Emulation.OpenMachine");
        _open.Click += OpenSelectedMachine;
        OptionsEmulationSection.ConfigurationSaved += ConfigurationSaved;

        var root = new Grid { Margin = new Thickness(16) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());

        var selector = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        selector.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        selector.ColumnDefinitions.Add(new ColumnDefinition());
        selector.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var label = new TextBlock
        {
            Text = LocExtension.Get("Emulation.Configuration"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 12, 5),
            FontWeight = FontWeights.SemiBold
        };
        selector.Children.Add(label);
        _configuration.Margin = new Thickness(0, 4, 8, 4);
        Grid.SetColumn(_configuration, 1);
        selector.Children.Add(_configuration);
        _open.Margin = new Thickness(0, 4, 0, 4);
        Grid.SetColumn(_open, 2);
        selector.Children.Add(_open);
        var selectorCard = new Border { Child = selector };
        selectorCard.SetResourceReference(StyleProperty, "Card");
        root.Children.Add(selectorCard);

        var welcomeTab = new TabItem
        {
            Header = new MainTabHeader { Icon = "\uE80F", Text = LocExtension.Get("Emulation.WelcomeTab") },
            Content = new TextBlock
            {
                Text = LocExtension.Get("Emulation.WelcomeText"),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 680,
                TextAlignment = TextAlignment.Center,
                FontSize = 18,
                Margin = new Thickness(32)
            },
            Padding = new Thickness(18, 9, 18, 9)
        };
        welcomeTab.SetResourceReference(StyleProperty, "MainTabItemStyle");
        _machines.Items.Add(welcomeTab);
        Grid.SetRow(_machines, 1);
        root.Children.Add(_machines);
        Content = root;
        Loaded += async (_, _) => await ReloadConfigurationsAsync();
    }

    public void Configure(AppSettings settings) => _settings = settings;

    private async void ConfigurationSaved(object? sender, AmigaMachineConfiguration configuration)
    {
        await ReloadConfigurationsAsync();
        if (_openMachines.TryGetValue(configuration.Id, out var tab) && tab.Content is AmigaMachineView view)
            view.ApplyVideoRenderer(configuration.VideoRenderer);
    }

    public async Task ReloadConfigurationsAsync()
    {
        var selectedId = (_configuration.SelectedItem as ConfigurationItem)?.Configuration.Id;
        var configurations = await new AmigaConfigurationStore(StoragePaths.AmigaConfigurationsDirectory,
            StoragePaths.DataDirectory).LoadAllAsync();
        _configuration.ItemsSource = configurations.Select(configuration => new ConfigurationItem(configuration)).ToArray();
        _configuration.SelectedItem = _configuration.Items.OfType<ConfigurationItem>()
            .FirstOrDefault(item => item.Configuration.Id == selectedId)
            ?? _configuration.Items.OfType<ConfigurationItem>().FirstOrDefault();
        _open.IsEnabled = _configuration.SelectedItem is not null;
    }

    private async void OpenSelectedMachine(object sender, RoutedEventArgs e)
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
            ValidateConfiguration(selected.Configuration);
            var runtimeConfiguration = await AmigaMachineView.PrepareRuntimeConfigurationAsync(selected.Configuration);
            var corePath = await AmigaCoreProvider.EnsureAvailableAsync();
            var audio = selected.Configuration.Audio ?? new AmigaAudioConfiguration();
            var engine = new AmigaEngine(StoragePaths.AmigaSessionsDirectory, corePath,
                () => new WasapiAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
                configuration => Path.Combine(StoragePaths.AmigaConfigurationsDirectory,
                    configuration.Id.ToString("N"), "Saves"), Environment.ProcessPath);
            IAmigaMachine CreateMachine() => engine.CreateAmigaMachine(runtimeConfiguration);
            var machine = CreateMachine();
            var view = new AmigaMachineView(machine, CreateMachine, runtimeConfiguration, selected.Configuration.Input,
                _settings.EmulationShortcuts,
                Path.Combine(_settings.EmulationStateFolder, $"amiga-{selected.Configuration.Id:N}.gwas"),
                _settings.EmulationCaptureFolder);
            var tab = new TabItem { Content = view, Padding = new Thickness(18, 9, 14, 9) };
            tab.SetResourceReference(StyleProperty, "MainTabItemStyle");
            tab.Header = CreateMachineTabHeader(selected.DisplayName,
                () => CloseMachineAsync(selected.Configuration.Id, tab, view));
            _openMachines.Add(selected.Configuration.Id, tab);
            view.CloseRequested += async (_, _) => await CloseMachineAsync(selected.Configuration.Id, tab, view);
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
            var logPath = ErrorLog.Write(error, "Opening an Amiga configuration");
            var detail = logPath is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", logPath);
            MessageBox.Show(Window.GetWindow(this), LocExtension.Get("Error.Unexpected", detail), "Amiga",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { _open.IsEnabled = _configuration.SelectedItem is not null; }
    }

    private async Task CloseMachineAsync(Guid id, TabItem tab, AmigaMachineView view)
    {
        if (!_openMachines.ContainsKey(id)) return;
        await view.StopAsync();
        _openMachines.Remove(id);
        _machines.Items.Remove(tab);
    }

    private static FrameworkElement CreateMachineTabHeader(string title, Func<Task> close)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(new TextBlock
        {
            Text = "\uE7FC", FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"), FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 7, 0)
        });
        panel.Children.Add(new TextBlock
        {
            Text = title, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 7, 0)
        });
        var button = new Button
        {
            Content = new TextBlock
            {
                Text = "\uE8BB", FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"), FontSize = 9,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            },
            ToolTip = LocExtension.Get("Common.Close"), Width = 18, Height = 18, MinWidth = 0, MinHeight = 0,
            Padding = new Thickness(0), Margin = new Thickness(0), VerticalAlignment = VerticalAlignment.Center
        };
        button.SetResourceReference(StyleProperty, "StatusIconButton");
        button.Click += async (_, e) =>
        {
            e.Handled = true;
            button.IsEnabled = false;
            try { await close(); }
            finally { button.IsEnabled = true; }
        };
        panel.Children.Add(button);
        return panel;
    }

    private static void ValidateConfiguration(AmigaMachineConfiguration configuration)
    {
        if (!File.Exists(configuration.KickstartPath))
            throw new FileNotFoundException("Kickstart", configuration.KickstartPath);
        var media = configuration.Media?.FirstOrDefault()?.Path ?? configuration.InitialDiskPath;
        if (!string.IsNullOrWhiteSpace(media) && !File.Exists(media) && !Directory.Exists(media))
            throw new FileNotFoundException("Amiga media", media);
    }

    public async Task StopAllAsync()
    {
        foreach (var view in _openMachines.Values.Select(item => item.Content).OfType<AmigaMachineView>().ToArray())
            await view.StopAsync();
        _openMachines.Clear();
    }

    private sealed record ConfigurationItem(AmigaMachineConfiguration Configuration)
    {
        public string DisplayName => $"{Configuration.Model} · {Configuration.Id.ToString("N")[..8]}";
    }
}
