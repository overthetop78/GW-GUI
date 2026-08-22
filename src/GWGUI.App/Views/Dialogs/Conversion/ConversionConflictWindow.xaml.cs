using GWGUI.Domain.Conversion;
using GWGUI.App.Localization.Extensions;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace GWGUI.App.Views.Dialogs.Conversion;

public enum ConversionConflictChoice { Overwrite, Skip, Number }
public sealed record ConversionConflictOption(ConversionConflictChoice Value, string Label);
public sealed class ConversionConflictRow
{
    public ConversionOutput Output { get; }
    public string FileName => Path.GetFileName(Output.OutputPath);
    public IReadOnlyList<ConversionConflictOption> Choices { get; } =
    [new(ConversionConflictChoice.Overwrite, LocExtension.Get("Conflict.Overwrite")), new(ConversionConflictChoice.Skip, LocExtension.Get("Conflict.Skip")), new(ConversionConflictChoice.Number, LocExtension.Get("Conflict.Number"))];
    public ConversionConflictOption SelectedChoice { get; set; }
    public ConversionConflictChoice Choice => SelectedChoice.Value;
    public ConversionConflictRow(ConversionOutput output) { Output = output; SelectedChoice = Choices[2]; }
}

public partial class ConversionConflictWindow : Window
{
    public ObservableCollection<ConversionConflictRow> Rows { get; }
    public ConversionConflictWindow(IEnumerable<ConversionOutput> outputs) { InitializeComponent(); Rows = new(outputs.Select(x => new ConversionConflictRow(x))); DataContext = this; }
    private void ApplyAll_Click(object sender, RoutedEventArgs e) { if (sender is Button { Tag: string value } && Enum.TryParse<ConversionConflictChoice>(value, out var choice)) foreach (var row in Rows) row.SelectedChoice = row.Choices.Single(x => x.Value == choice); DataContext = null; DataContext = this; }
    private void Continue_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
