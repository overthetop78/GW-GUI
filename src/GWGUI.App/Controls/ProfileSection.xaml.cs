using System.Windows.Controls;

namespace GWGUI.App.Controls;

public partial class ProfileSection : UserControl
{
    public ProfileSection() => InitializeComponent();
    public ComboBox ProfileCombo => Profiles;
    public Button SaveButton => Save;
    public Button ResetButton => Reset;
}
