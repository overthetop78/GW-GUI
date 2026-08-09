using System.Windows;
using System.Windows.Controls;
using GWGUI.Domain.Formats;

namespace GWGUI.App.Controls;

public partial class DiskClassificationSelector : UserControl
{
    private DiskClassificationCatalog _catalog = new([]);
    private bool _updating;

    public DiskClassificationSelector() => InitializeComponent();

    public event EventHandler? ValueChanged;
    public bool AutomaticDetection => Automatic.IsChecked == true;
    public string? SelectedMachine => Machine.SelectedItem as string;
    public string? SelectedFormatId => (Format.SelectedItem as DiskFormat)?.Id;
    public string? SelectedProtectionId => Protection.SelectedItem is DiskProtection protection && !string.IsNullOrWhiteSpace(protection.Id)
        ? protection.Id : null;

    public void SetCatalog(IEnumerable<DiskFormat> formats)
    {
        var machine = SelectedMachine;
        var format = SelectedFormatId;
        var protection = SelectedProtectionId;
        _catalog = new DiskClassificationCatalog(formats);
        _updating = true;
        Machine.ItemsSource = _catalog.Machines;
        Machine.SelectedItem = _catalog.Machines.FirstOrDefault(item => item.Equals(machine, StringComparison.OrdinalIgnoreCase));
        RefreshFormats(format);
        RefreshProtections(protection);
        _updating = false;
    }

    public void ApplyDetection(string? detectedFormatId, string? detectedProtectionId)
    {
        if (!AutomaticDetection) return;
        var format = _catalog.ResolveFormat(detectedFormatId);
        if (format is null) return;
        _updating = true;
        Machine.SelectedItem = format.Family;
        RefreshFormats(format.Id);
        RefreshProtections(detectedProtectionId);
        _updating = false;
    }

    private void RefreshFormats(string? selectedId = null)
    {
        var formats = _catalog.FormatsFor(SelectedMachine);
        Format.ItemsSource = formats;
        Format.SelectedItem = formats.FirstOrDefault(item => item.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase))
            ?? formats.FirstOrDefault();
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
        _updating = true; RefreshFormats(); RefreshProtections(); _updating = false;
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
