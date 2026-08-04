using System.Windows;

namespace GWGUI.App;

public partial class ProfileNameWindow : Window
{
    public string ProfileName => NameText.Text.Trim();
    public ProfileNameWindow(string? initialName = null) { InitializeComponent(); NameText.Text = initialName ?? ""; NameText.SelectAll(); NameText.Focus(); }
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProfileName)) { MessageBox.Show(this, "Indiquez un nom.", "Profil", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        DialogResult = true;
    }
}
