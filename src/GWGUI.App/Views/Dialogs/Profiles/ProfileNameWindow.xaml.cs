using GWGUI.App.Localization.Extensions;
using System.Windows;

namespace GWGUI.App.Views.Dialogs.Profiles;

public partial class ProfileNameWindow : Window
{
    private readonly Action _accept;
    private readonly Action _nameRequired;
    public string ProfileName => NameText.Text.Trim();
    public ProfileNameWindow(string? initialName = null) : this(initialName, null, null) { }
    internal ProfileNameWindow(string? initialName, Action? accept, Action? nameRequired)
    {
        InitializeComponent(); NameText.Text = initialName ?? "";
        _accept = accept ?? (() => DialogResult = true);
        _nameRequired = nameRequired ?? (() => MessageBox.Show(this, LocExtension.Get("Profile.NameRequired"), LocExtension.Get("Profile.Title"), MessageBoxButton.OK, MessageBoxImage.Information));
        Loaded += (_, _) => { NameText.SelectAll(); NameText.Focus(); };
    }
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProfileName)) { _nameRequired(); return; }
        _accept();
    }
}
