using GWGUI.App.Localization.Extensions;
using System.Windows;

namespace GWGUI.App.Views.Dialogs.Profiles;

public partial class ProfileNameWindow : Window
{
    public string ProfileName => NameText.Text.Trim();
    public ProfileNameWindow(string? initialName = null) { InitializeComponent(); NameText.Text = initialName ?? ""; NameText.SelectAll(); NameText.Focus(); }
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProfileName)) { MessageBox.Show(this, LocExtension.Get("Profile.NameRequired"), LocExtension.Get("Profile.Title"), MessageBoxButton.OK, MessageBoxImage.Information); return; }
        DialogResult = true;
    }
}
