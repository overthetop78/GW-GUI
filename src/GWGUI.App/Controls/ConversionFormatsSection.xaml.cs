using System.Windows.Controls;
using System.Windows.Input;
using GWGUI.App.Localization;
using GWGUI.App.ViewModels;
using GWGUI.Domain.Conversion;

namespace GWGUI.App.Controls;

public partial class ConversionFormatsSection : UserControl
{
    private IReadOnlyList<ConversionFormatPresentation> _items = [];
    private string _sourceExtension = "";

    public ConversionFormatsSection() => InitializeComponent();
    public event EventHandler? ValueChanged;
    public IReadOnlyList<string> SelectedOutputLines => SelectedOutputs.Items.Cast<string>().ToArray();
    public IReadOnlyList<string> MachineChoices => Machines.Items.Cast<string>().ToArray();
    public IReadOnlyList<ConversionFormatPresentation> VisibleFormats => Formats.Items.Cast<ConversionFormatPresentation>().ToArray();

    public void SetItems(IReadOnlyList<ConversionFormatPresentation> items, string? sourceExtension = null)
    {
        var selectedMachine = Machines.SelectedItem as string;
        _sourceExtension = sourceExtension ?? "";
        _items = items;
        SelectedOutputs.ItemsSource = items.Where(item => item.IsSelected)
            .SelectMany(item => SelectedLines(item)).ToArray();
        var machines = items.Select(item => item.Format.Family).Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.CurrentCultureIgnoreCase).ToArray();
        Machines.ItemsSource = machines;
        Machines.SelectedItem = machines.FirstOrDefault(machine => machine.Equals(selectedMachine, StringComparison.OrdinalIgnoreCase))
            ?? machines.FirstOrDefault();
        RefreshFormats();
    }

    private IEnumerable<string> SelectedLines(ConversionFormatPresentation item)
    {
        var extensions = item.ExplicitExtensions.Count == 0
            ? item.Format.Extensions.Where(extension => extension.IsDefault).Take(1).Select(extension => extension.Extension)
            : item.ExplicitExtensions.Order(StringComparer.OrdinalIgnoreCase);
        foreach (var extension in extensions)
        {
            var fidelity = ConversionFidelity.ForConversion(_sourceExtension, extension);
            yield return $"{item.Format.DisplayName} · {extension.TrimStart('.').ToUpperInvariant()} · {LocExtension.Get(FidelityKey(fidelity))}";
        }
    }

    private static string FidelityKey(ConversionFidelityLevel fidelity) => fidelity switch
    {
        ConversionFidelityLevel.ReconstructedTracks => "Conversion.Fidelity.ReconstructedTracks",
        ConversionFidelityLevel.PreservedFlux => "Conversion.Fidelity.PreservedFlux",
        _ => "Conversion.Fidelity.SectorData"
    };

    private void RefreshFormats()
    {
        if (Machines.SelectedItem is not string machine) { Formats.ItemsSource = null; return; }
        Formats.ItemsSource = _items.Where(item => item.Format.Family.Equals(machine, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.IsSelected).ThenByDescending(item => item.Format.IsCommon)
            .ThenBy(item => item.Format.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    private void Machines_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshFormats();
    private void Format_ValueChanged(object? sender, EventArgs e) => ValueChanged?.Invoke(sender, e);
    private void List_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer viewer || viewer.ScrollableHeight <= 0) return;
        viewer.ScrollToVerticalOffset(Math.Clamp(viewer.VerticalOffset - e.Delta / 3d, 0, viewer.ScrollableHeight));
        e.Handled = true;
    }
}
