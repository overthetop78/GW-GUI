using System.Windows.Controls;
using System.Windows.Documents;

namespace GWGUI.App.Views.Controls.Shell;

public partial class TerminalSection : UserControl
{
    public TerminalSection() => InitializeComponent();
    public TextBox CommandTextBox => Command;
    public RichTextBox OutputTextBox => Output;
    public string OutputText => new TextRange(Output.Document.ContentStart, Output.Document.ContentEnd).Text.TrimEnd('\r', '\n');
    public Button CopyButton => Copy;
}
