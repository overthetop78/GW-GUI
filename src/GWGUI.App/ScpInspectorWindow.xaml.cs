namespace GWGUI.App;

public partial class ScpInspectorWindow : System.Windows.Window
{
    public ScpInspectorWindow()
    {
        InitializeComponent(); Panel.IsDetached = true;
        Panel.CloseRequested += (_, _) => Close();
        Panel.AttachRequested += (_, _) => { AttachRequested?.Invoke(this, EventArgs.Empty); Close(); };
    }
    public event EventHandler? AttachRequested;
}
