using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Controls;

public partial class OptionsProfilesSection : UserControl
{
    public OptionsProfilesSection() => InitializeComponent();
    public ListBox ReadProfiles => ReadProfilesList;
    public ListBox WriteProfiles => WriteProfilesList;
    public ListBox ConvertProfiles => ConvertProfilesList;
    public event RoutedEventHandler? RenameRequested;
    public event RoutedEventHandler? DeleteRequested;
    public event KeyEventHandler? ProfileKeyDown;
    public event MouseButtonEventHandler? ProfileLeftButtonDown;
    public event MouseButtonEventHandler? ProfileRightButtonDown;
    private void RenameProfile_Click(object sender, RoutedEventArgs e) => RenameRequested?.Invoke(sender, e);
    private void DeleteProfile_Click(object sender, RoutedEventArgs e) => DeleteRequested?.Invoke(sender, e);
    private void ProfileList_KeyDown(object sender, KeyEventArgs e) => ProfileKeyDown?.Invoke(sender, e);
    private void ProfileList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => ProfileLeftButtonDown?.Invoke(sender, e);
    private void ProfileList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) => ProfileRightButtonDown?.Invoke(sender, e);
}
