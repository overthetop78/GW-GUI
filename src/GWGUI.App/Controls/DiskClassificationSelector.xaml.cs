using System.Windows;
using System.Windows.Controls;
using GWGUI.Domain.Formats;

namespace GWGUI.App.Controls;

public partial class DiskClassificationSelector : UserControl
{
    private DiskClassificationCatalog _catalog = new([]);
    private IReadOnlySet<string> _detectedFormatIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyDictionary<string, string> _detectedFormatByMachine = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private bool _updating;

    public DiskClassificationSelector() => InitializeComponent();

    public event EventHandler? ValueChanged;
    public bool ShowAutomatic
    {
        get => Automatic.Visibility == Visibility.Visible;
        set => Automatic.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }
    public bool AutomaticDetection => Automatic.IsChecked == true;
    public string? SelectedMachine => (Machine.SelectedItem as DiskMachineChoice)?.DisplayName;
    public string? SelectedFormatId => (Format.SelectedItem as DiskFormatChoice)?.Format.Id;
    public string? SelectedProtectionId => Protection.SelectedItem is DiskProtection protection && !string.IsNullOrWhiteSpace(protection.Id)
        ? protection.Id : null;

    public void SetAutomaticDetection(bool enabled)
    {
        if (Automatic.IsChecked == enabled) return;
        _updating = true;
        Automatic.IsChecked = enabled;
        _updating = false;
    }

    public void SetCatalog(IEnumerable<DiskFormat> formats)
    {
        var machine = SelectedMachine;
        var format = SelectedFormatId;
        var protection = SelectedProtectionId;
        _catalog = new DiskClassificationCatalog(formats);
        _updating = true;
        RefreshMachines(machine);
        RefreshFormats(format);
        RefreshProtections(protection);
        _updating = false;
    }

    public void ApplyDetection(string? detectedFormatId, string? detectedProtectionId)
        => ApplyDetection(detectedFormatId, detectedProtectionId, detectedFormatId is null ? [] : [detectedFormatId]);

    public void ApplyDetection(string? detectedFormatId, string? detectedProtectionId, IEnumerable<string> detectedFormatIds)
    {
        if (!AutomaticDetection)
        {
            return;
        }

        var resolved = detectedFormatIds
            .Select(_catalog.ResolveFormat)
            .Where(format => format is not null)
            .Cast<DiskFormat>()
            .DistinctBy(format => format.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _detectedFormatIds = resolved.Select(format => format.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _detectedFormatByMachine = resolved
            .GroupBy(format => format.Family, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Id, StringComparer.OrdinalIgnoreCase);
        var format = _catalog.ResolveFormat(detectedFormatId) ?? resolved.FirstOrDefault();
        _updating = true;
        RefreshMachines(format?.Family);
        if (format is null)
        {
            Format.ItemsSource = Array.Empty<DiskFormat>();
            Format.SelectedItem = null;
            Protection.ItemsSource = Array.Empty<DiskProtection>();
            Protection.SelectedItem = null;
            ProtectionPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            RefreshFormats(format.Id);
            RefreshProtections(detectedProtectionId);
        }
        _updating = false;
    }

    private void RefreshFormats(string? selectedId = null)
    {
        var formats = _catalog.FormatsFor(SelectedMachine).Select(format => new DiskFormatChoice(format, _detectedFormatIds.Contains(format.Id))).ToArray();
        Format.ItemsSource = formats;
        Format.SelectedItem = formats.FirstOrDefault(item => item.Format.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase))
            ?? formats.FirstOrDefault(item => item.Format.IsCommon)
            ?? formats.FirstOrDefault();
    }

    private void RefreshMachines(string? selectedMachine = null)
    {
        var machines = _catalog.Machines.Select(machine => new DiskMachineChoice(machine, _detectedFormatByMachine.ContainsKey(machine))).ToArray();
        Machine.ItemsSource = machines;
        Machine.SelectedItem = machines.FirstOrDefault(item => item.DisplayName.Equals(selectedMachine, StringComparison.OrdinalIgnoreCase));
    }

    private void RefreshProtections(string? selectedId = null)
    {
        var compatible = _catalog.ProtectionsFor(SelectedMachine, SelectedFormatId);
        var protections = compatible.Count == 0
            ? []
            : new[] { new DiskProtection("", SelectedMachine ?? "", new HashSet<string>(), "—") }.Concat(compatible).ToArray();
        Protection.ItemsSource = protections;
        Protection.SelectedItem = protections.FirstOrDefault(item => item.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase))
            ?? protections.FirstOrDefault();
        ProtectionPanel.Visibility = compatible.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Automatic_Changed(object sender, RoutedEventArgs e) { if (!_updating) ValueChanged?.Invoke(this, EventArgs.Empty); }
    private void Machine_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_updating) return;
        var detected = SelectedMachine is { } machine ? _detectedFormatByMachine.GetValueOrDefault(machine) : null;
        _updating = true; RefreshFormats(detected); RefreshProtections(); _updating = false;
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
    private void Format_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_updating) return;
        _updating = true; RefreshProtections(); _updating = false;
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
    private void Protection_Changed(object sender, SelectionChangedEventArgs e) { if (!_updating) ValueChanged?.Invoke(this, EventArgs.Empty); }
}
