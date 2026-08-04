using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using GWGUI.Domain.Conversion;

namespace GWGUI.App;

public enum ConversionConflictChoice { Overwrite, Skip, Number }
public sealed class ConversionConflictRow
{
    public ConversionOutput Output { get; }
    public string FileName => Path.GetFileName(Output.OutputPath);
    public IReadOnlyList<ConversionConflictChoice> Choices { get; } = Enum.GetValues<ConversionConflictChoice>();
    public ConversionConflictChoice Choice { get; set; } = ConversionConflictChoice.Number;
    public ConversionConflictRow(ConversionOutput output) => Output = output;
}

public partial class ConversionConflictWindow : Window
{
    public ObservableCollection<ConversionConflictRow> Rows { get; }
    public ConversionConflictWindow(IEnumerable<ConversionOutput> outputs) { InitializeComponent(); Rows = new(outputs.Select(x => new ConversionConflictRow(x))); DataContext = this; }
    private void ApplyAll_Click(object sender, RoutedEventArgs e) { if (sender is Button { Tag: string value } && Enum.TryParse<ConversionConflictChoice>(value, out var choice)) foreach (var row in Rows) row.Choice = choice; DataContext = null; DataContext = this; }
    private void Continue_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
