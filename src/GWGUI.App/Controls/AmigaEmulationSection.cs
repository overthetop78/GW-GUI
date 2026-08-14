using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Controls;

public sealed class AmigaEmulationSection : UserControl
{
    private readonly ComboBox _configuration = new() { DisplayMemberPath = nameof(ConfigurationItem.DisplayName) };
    private readonly Button _open = new() { MinWidth = 130 };
    private readonly TabControl _machines = new();
    private readonly Dictionary<Guid, TabItem> _openMachines = [];

    public AmigaEmulationSection()
    {
        AutomationProperties.SetName(_configuration, "Amiga configuration");
        AutomationProperties.SetName(_open, "Open selected Amiga configuration");
        AutomationProperties.SetName(_machines, "Open emulated machines");
        _open.Content = LocExtension.Get("Emulation.OpenMachine");
        _open.Click += OpenSelectedMachine;

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

        _machines.Items.Add(new TabItem
        {
            Header = LocExtension.Get("Emulation.WelcomeTab"),
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
            }
        });
        Grid.SetRow(_machines, 1);
        root.Children.Add(_machines);
        Content = root;
        Loaded += async (_, _) => await ReloadConfigurationsAsync();
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
            var corePath = await AmigaCoreProvider.EnsureAvailableAsync();
            var engine = new AmigaEngine(StoragePaths.AmigaSessionsDirectory, corePath,
                () => new WasapiAudioOutput(),
                configuration => Path.Combine(StoragePaths.AmigaConfigurationsDirectory,
                    configuration.Id.ToString("N"), "Saves"), Environment.ProcessPath);
            var machine = engine.CreateAmigaMachine(selected.Configuration);
            var view = new AmigaMachineView(machine);
            var tab = new TabItem { Header = selected.DisplayName, Content = view };
            _openMachines.Add(selected.Configuration.Id, tab);
            view.CloseRequested += async (_, _) =>
            {
                await view.StopAsync();
                _openMachines.Remove(selected.Configuration.Id);
                _machines.Items.Remove(tab);
            };
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
