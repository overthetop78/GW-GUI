using System.Windows;
using System.Windows.Controls;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;

namespace GWGUI.App;

public partial class ConversionFormatControl : UserControl
{
    public DiskFormat Format { get; }
    public event EventHandler? ValueChanged;
    public bool IsSelected => FormatCheck.IsChecked == true;
    public IReadOnlySet<string> ExplicitExtensions => ExtensionsPanel.Children.OfType<CheckBox>().Where(x => x.IsChecked == true).Select(x => (string)x.Tag).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public ConversionFormatControl(DiskFormat format)
    {
        InitializeComponent(); Format = format; FormatCheck.Content = format.DisplayName; FormatCheck.ToolTip = $"Aucune extension cochée : {format.Extensions.First(x => x.IsDefault).Extension.ToUpperInvariant()} par défaut.";
        foreach (var extension in format.Extensions)
        {
            var check = new CheckBox { Content = extension.Extension.TrimStart('.').ToUpperInvariant(), Tag = extension.Extension, Margin = new Thickness(12, 0, 0, 0), ToolTip = extension.DisplayName };
            check.Checked += SelectionChanged; check.Unchecked += SelectionChanged; ExtensionsPanel.Children.Add(check);
        }
    }

    public ConversionSelection ToSelection() => new(Format.Id, ExplicitExtensions);
    public void SetState(bool selected, IEnumerable<string>? explicitExtensions)
    {
        FormatCheck.IsChecked = selected;
        var wanted = explicitExtensions?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        foreach (var check in ExtensionsPanel.Children.OfType<CheckBox>()) check.IsChecked = wanted.Contains((string)check.Tag);
    }
    private void SelectionChanged(object sender, RoutedEventArgs e) => ValueChanged?.Invoke(this, EventArgs.Empty);
}
