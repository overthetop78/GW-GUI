using GWGUI.Domain.Settings;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.App.Views.Controls.Common;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.Emulation;


namespace GWGUI.App.Views.Controls.Emulation.Machine;

public sealed partial class EmulationSection : UserControl
{
    private readonly ComboBox _module = new()
        { DisplayMemberPath = nameof(EmulationModuleListItem.DisplayName) };
    private readonly ComboBox _configuration = new()
        { DisplayMemberPath = nameof(EmulationConfigurationListItem.DisplayName) };
    private readonly Button _open = new() { MinWidth = 130 };
    private readonly TabControl _machines = new();
    private readonly TextBlock _configurationLabel = new();
    private readonly MainTabHeader _welcomeHeader = new();
    private readonly TextBlock _welcomeText = new();
    private readonly IReadOnlyList<IEmulationModule> _modules = EmulationModuleRegistry.Modules;
    private IReadOnlyList<EmulationConfigurationListItem> _configurations = [];
    private readonly Dictionary<(string ModuleId, Guid Id), TabItem> _openMachines = [];
    private AppSettings _settings = new();
    private Point _tabDragStart;
    private TabItem? _draggedMachineTab;
    private Point _tabDragOffset;
    private MachineTabDragAdorner? _tabDragAdorner;

    public EmulationSection()
    {
        AutomationProperties.SetName(_module,
            LocExtension.Get(ControlVisualConstants.MachinesResource));
        AutomationProperties.SetName(_configuration,
            LocExtension.Get(ControlVisualConstants.ConfigurationResource));
        AutomationProperties.SetName(_open,
            LocExtension.Get(ControlVisualConstants.OpenMachineResource));
        AutomationProperties.SetName(_machines,
            LocExtension.Get(ControlVisualConstants.MachinesResource));
        _open.Content = LocExtension.Get(ControlVisualConstants.OpenMachineResource);
        _open.Click += OpenSelectedMachine;
        _module.SelectionChanged += ModuleSelectionChanged;
        _machines.AllowDrop = true;
        _machines.PreviewMouseLeftButtonDown += MachineTabMouseDown;
        _machines.PreviewMouseMove += MachineTabMouseMove;
        _machines.DragOver += MachineTabDragOver;
        _machines.Drop += MachineTabDrop;
        OptionsEmulationSection.ConfigurationSaved += ConfigurationSaved;
        OptionsEmulationSection.VideoConfigurationChanged += VideoConfigurationChanged;
        Content = BuildContent();
        Loaded += async (_, _) => await ReloadConfigurationsAsync();
    }

    public void Configure(AppSettings settings) => _settings = settings;

    internal void RefreshLocalizedContent()
    {
        var configurationText = LocExtension.Get(ControlVisualConstants.ConfigurationResource);
        _configurationLabel.Text = configurationText;
        AutomationProperties.SetName(_module,
            LocExtension.Get(ControlVisualConstants.MachinesResource));
        AutomationProperties.SetName(_configuration, configurationText);
        var openText = LocExtension.Get(ControlVisualConstants.OpenMachineResource);
        _open.Content = openText;
        AutomationProperties.SetName(_open, openText);
        _welcomeHeader.Text = LocExtension.Get(ControlVisualConstants.WelcomeTabResource);
        _welcomeText.Text = LocExtension.Get(ControlVisualConstants.WelcomeResource);
        foreach (var item in _openMachines)
        {
            if (item.Value.Header is not StackPanel header) continue;
            var title = header.Children.OfType<TextBlock>().FirstOrDefault(text =>
                !Equals(text.FontFamily, ControlVisualConstants.IconFont));
            var machine = item.Value.Tag as EmulationMachineRuntime;
            if (title is not null && machine is not null) title.Text = RuntimeDisplayName(machine);
        }
        _ = ReloadConfigurationsAsync();
    }
}
