using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Shell;

public partial class TerminalSection : UserControl
{
    public TerminalSection() => InitializeComponent();
    public TextBox CommandTextBox => Command;
    public TextBox OutputTextBox => Output;
    public Button CopyButton => Copy;
}
