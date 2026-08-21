using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public sealed partial class EmulationSection : UserControl
{
    private readonly ComboBox _configuration = new()
        { DisplayMemberPath = nameof(EmulationConfigurationListItem.DisplayName) };
    private readonly Button _open = new() { MinWidth = 130 };
    private readonly TabControl _machines = new();
    private readonly IReadOnlyList<IEmulationModule> _modules = EmulationModuleRegistry.Modules;
    private readonly Dictionary<(string ModuleId, Guid Id), TabItem> _openMachines = [];
    private AppSettings _settings = new();
    private Point _tabDragStart;
    private TabItem? _draggedMachineTab;
    private Point _tabDragOffset;
    private MachineTabDragAdorner? _tabDragAdorner;

    public EmulationSection()
    {
        AutomationProperties.SetName(_configuration,
            LocExtension.Get(ControlVisualConstants.ConfigurationResource));
        AutomationProperties.SetName(_open,
            LocExtension.Get(ControlVisualConstants.OpenMachineResource));
        AutomationProperties.SetName(_machines,
            LocExtension.Get(ControlVisualConstants.MachinesResource));
        _open.Content = LocExtension.Get(ControlVisualConstants.OpenMachineResource);
        _open.Click += OpenSelectedMachine;
        _machines.AllowDrop = true;
        _machines.PreviewMouseLeftButtonDown += MachineTabMouseDown;
        _machines.PreviewMouseMove += MachineTabMouseMove;
        _machines.DragOver += MachineTabDragOver;
        _machines.Drop += MachineTabDrop;
        OptionsEmulationSection.ConfigurationSaved += ConfigurationSaved;
        Content = BuildContent();
        Loaded += async (_, _) => await ReloadConfigurationsAsync();
    }

    public void Configure(AppSettings settings) => _settings = settings;
}
