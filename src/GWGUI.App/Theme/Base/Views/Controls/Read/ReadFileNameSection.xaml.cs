using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Read;

public partial class ReadFileNameSection : UserControl
{
    public ReadFileNameSection() => InitializeComponent();
    public TextBox FileNameTextBox => FileNameInput;
    public TextBox ExtensionTextBox => ExtensionText;
}
