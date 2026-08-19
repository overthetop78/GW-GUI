using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
    private Point _tabDragStart;
    private TabItem? _draggedMachineTab;
    private Point _tabDragOffset;
    private MachineTabDragAdorner? _tabDragAdorner;

    public EmulationSection()
    {
        AutomationProperties.SetName(_configuration, LocExtension.Get("Emulation.Configuration"));
        AutomationProperties.SetName(_open, LocExtension.Get("Emulation.Machine.Open"));
        AutomationProperties.SetName(_machines, LocExtension.Get("Emulation.Tab.Machines"));
        _open.Content = LocExtension.Get("Emulation.Machine.Open");
        _open.Click += OpenSelectedMachine;
        _machines.AllowDrop = true;
        _machines.PreviewMouseLeftButtonDown += MachineTabMouseDown;
        _machines.PreviewMouseMove += MachineTabMouseMove;
        _machines.DragOver += MachineTabDragOver;
        _machines.Drop += MachineTabDrop;
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
            && tab.Content is AmigaMachineController view) view.ApplyVideoRenderer(configuration.VideoRenderer);
    }

    private async void AtariConfigurationSaved(object? sender, AtariMachineConfiguration configuration)
    {
        await ReloadConfigurationsAsync();
        if (_openMachines.TryGetValue((MachineFamily.Atari, configuration.Id), out var tab)
            && tab.Content is AtariMachineController view) view.ApplyVideoRenderer(configuration.VideoRenderer);
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
        IAmigaMachine CreateMachine(AmigaMachineConfiguration value) => engine.CreateAmigaMachine(value);
        var view = new AmigaMachineController(CreateMachine(runtime), CreateMachine, runtime, configuration.Input,
            _settings.EmulationShortcuts,
            Path.Combine(_settings.EmulationStateFolder, $"amiga-{configuration.Id:N}.gwas"),
            _settings.EmulationCaptureFolder, _settings.EmulationMediaFolders);
        await AddMachineAsync(selected, view, view.StopAsync);
    }

    private async Task OpenAtariAsync(ConfigurationItem selected, AtariMachineConfiguration configuration)
    {
        AtariEmulationFunctions.ValidateConfiguration(configuration);
        var corePath = await AtariCoreProvider.GetInstalledPathAsync(configuration.Core);
        var engine = new AtariEngine(StoragePaths.AtariSessionsDirectory, corePath, Environment.ProcessPath!,
            () => new WasapiAudioOutput(), value => Path.Combine(StoragePaths.AtariStatesDirectory,
                value.Id.ToString(AtariEmulationConstants.IdentifierFormat)));
        IAtariMachine CreateMachine(AtariMachineConfiguration value) => engine.CreateAtariMachine(value);
        var view = new AtariMachineController(CreateMachine(configuration), CreateMachine, configuration,
            _settings.EmulationShortcuts,
            AtariMachineViewFunctions.QuickStatePath(_settings.EmulationStateFolder, configuration.Id),
            _settings.EmulationCaptureFolder, _settings.EmulationMediaFolders);
        await AddMachineAsync(selected, view, view.StopAsync);
    }

    private Task AddMachineAsync(ConfigurationItem selected, FrameworkElement view, Func<Task> stop)
    {
        var key = (selected.Family, selected.Id);
        var tab = new TabItem { Content = view, Padding = new Thickness(18, 9, 14, 9) };
        tab.SetResourceReference(StyleProperty, AtariEmulationConstants.MainTabItemStyleResource);
        tab.Header = CreateMachineTabHeader(MachineTitle(selected), selected.DisplayName,
            () => CloseMachineAsync(key, tab, stop));
        _openMachines.Add(key, tab);
        _machines.Items.Add(tab);
        _machines.SelectedItem = tab;
        return Task.CompletedTask;
    }

    private async Task CloseMachineAsync((MachineFamily Family, Guid Id) key, TabItem tab, Func<Task> stop)
    {
        if (!_openMachines.ContainsKey(key)) return;
        await stop();
        _openMachines.Remove(key);
        _machines.Items.Remove(tab);
    }

    private static FrameworkElement CreateMachineTabHeader(string title, string description, Func<Task> close)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            ToolTip = description
        };
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

    private static string MachineTitle(ConfigurationItem selected) => selected switch
    {
        { Amiga: { } amiga } => AmigaModelCatalog.Get(amiga.Model).DisplayName,
        { Atari: { } atari } => AtariConfigurationCatalogFunctions.Models()
            .Single(model => model.Model == atari.Model).DisplayName,
        _ => selected.DisplayName
    };

    private void MachineTabMouseDown(object sender, MouseButtonEventArgs args)
    {
        _tabDragStart = args.GetPosition(_machines);
        _draggedMachineTab = Ancestor<TabItem>(args.OriginalSource as DependencyObject);
        if (_draggedMachineTab is null || _machines.Items.IndexOf(_draggedMachineTab) <= 0
            || Ancestor<Button>(args.OriginalSource as DependencyObject) is not null)
            _draggedMachineTab = null;
        else
            _tabDragOffset = args.GetPosition(_draggedMachineTab);
    }

    private void MachineTabMouseMove(object sender, MouseEventArgs args)
    {
        if (_draggedMachineTab is null || args.LeftButton != MouseButtonState.Pressed) return;
        var position = args.GetPosition(_machines);
        if (Math.Abs(position.X - _tabDragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(position.Y - _tabDragStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        var tab = _draggedMachineTab;
        _draggedMachineTab = null;
        var layer = AdornerLayer.GetAdornerLayer(_machines);
        if (layer is not null)
        {
            _tabDragAdorner = new MachineTabDragAdorner(_machines, tab, _tabDragOffset);
            layer.Add(_tabDragAdorner);
        }
        try { DragDrop.DoDragDrop(_machines, tab, DragDropEffects.Move); }
        finally
        {
            if (_tabDragAdorner is not null) layer?.Remove(_tabDragAdorner);
            _tabDragAdorner = null;
        }
    }

    private void MachineTabDragOver(object sender, DragEventArgs args)
    {
        var dragged = args.Data.GetData(typeof(TabItem)) as TabItem;
        var target = Ancestor<TabItem>(args.OriginalSource as DependencyObject);
        args.Effects = dragged is not null && target is not null
            && _machines.Items.IndexOf(dragged) > 0 && _machines.Items.IndexOf(target) > 0
            ? DragDropEffects.Move : DragDropEffects.None;
        var position = args.GetPosition(_machines);
        double? insertionX = null;
        if (args.Effects == DragDropEffects.Move && target is not null)
        {
            var bounds = target.TransformToAncestor(_machines)
                .TransformBounds(new Rect(new Point(), target.RenderSize));
            insertionX = position.X > bounds.Left + bounds.Width / 2 ? bounds.Right : bounds.Left;
        }
        _tabDragAdorner?.Update(position, insertionX);
        args.Handled = true;
    }

    private void MachineTabDrop(object sender, DragEventArgs args)
    {
        var dragged = args.Data.GetData(typeof(TabItem)) as TabItem;
        var target = Ancestor<TabItem>(args.OriginalSource as DependencyObject);
        if (dragged is null || target is null || ReferenceEquals(dragged, target)) return;
        MoveMachineTab(dragged, target, args.GetPosition(_machines).X);
        _machines.SelectedItem = dragged;
        args.Handled = true;
    }

    private void MoveMachineTab(TabItem dragged, TabItem target, double pointerX)
    {
        var sourceIndex = _machines.Items.IndexOf(dragged);
        var targetIndex = _machines.Items.IndexOf(target);
        if (sourceIndex <= 0 || targetIndex <= 0) return;
        var bounds = target.TransformToAncestor(_machines)
            .TransformBounds(new Rect(new Point(), target.RenderSize));
        var insertionIndex = targetIndex + (pointerX > bounds.Left + bounds.Width / 2 ? 1 : 0);
        if (sourceIndex < insertionIndex) insertionIndex--;
        if (sourceIndex == insertionIndex) return;
        _machines.Items.RemoveAt(sourceIndex);
        _machines.Items.Insert(Math.Clamp(insertionIndex, 1, _machines.Items.Count), dragged);
        _machines.SelectedItem = dragged;
    }

    private static T? Ancestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private sealed class MachineTabDragAdorner : Adorner
    {
        private readonly VisualBrush _tabBrush;
        private readonly Size _tabSize;
        private readonly Point _pointerOffset;
        private Point _pointer;
        private double? _insertionX;

        internal MachineTabDragAdorner(UIElement adornedElement, TabItem tab, Point pointerOffset)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            _tabBrush = new VisualBrush(tab) { Opacity = 0.78, Stretch = Stretch.None };
            _tabSize = tab.RenderSize;
            _pointerOffset = pointerOffset;
            _pointer = pointerOffset;
        }

        internal void Update(Point pointer, double? insertionX)
        {
            _pointer = pointer;
            _insertionX = insertionX;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var origin = new Point(_pointer.X - _pointerOffset.X, _pointer.Y - _pointerOffset.Y);
            drawingContext.PushOpacity(0.88);
            drawingContext.DrawRoundedRectangle(_tabBrush, new Pen(Brushes.DodgerBlue, 1.5),
                new Rect(origin, _tabSize), 9, 9);
            drawingContext.Pop();
            if (_insertionX is { } x)
                drawingContext.DrawLine(new Pen(Brushes.DodgerBlue, 3), new Point(x, 2),
                    new Point(x, Math.Max(2, _tabSize.Height - 2)));
        }
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
            if (tab.Content is AmigaMachineController amiga) await amiga.StopAsync();
            else if (tab.Content is AtariMachineController atari) await atari.StopAsync();
        }
        _openMachines.Clear();
    }

    private enum MachineFamily { Amiga, Atari }

    private sealed record ConfigurationItem(MachineFamily Family, Guid Id, string DisplayName,
        AmigaMachineConfiguration? Amiga, AtariMachineConfiguration? Atari);
}
