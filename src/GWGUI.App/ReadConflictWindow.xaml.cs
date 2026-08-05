using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Services;

namespace GWGUI.App;

public partial class ReadConflictWindow : Window
{
    public ReadConflictChoice Choice { get; private set; } = ReadConflictChoice.EditName;

    public ReadConflictWindow(string outputPath)
    {
        InitializeComponent();
        OutputPathText.Text = outputPath;
    }

    private void Choice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string value } && Enum.TryParse(value, out ReadConflictChoice choice))
            Choice = choice;
        DialogResult = true;
    }
}
