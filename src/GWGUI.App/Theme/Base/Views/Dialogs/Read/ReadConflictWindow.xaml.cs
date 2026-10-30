using GWGUI.App.Enums.Services.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Dialogs.Read;

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
