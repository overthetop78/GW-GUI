using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GWGUI.App.Controls;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Options;

internal sealed class ProfileOptionsController
{
    private readonly Window _owner;
    private readonly OptionsProfilesSection _section;
    private readonly ProfileOptionsState _state;
    private readonly Func<Task> _persistAsync;
    private readonly Func<string, object[], string> _localize;
    private ProfileOptionRow? _lastClick;
    private DateTime _lastClickAt;

    public ProfileOptionsController(
        Window owner,
        OptionsProfilesSection section,
        ProfileOptionsState state,
        Func<Task> persistAsync,
        Func<string, object[], string> localize)
    {
        _owner = owner;
        _section = section;
        _state = state;
        _persistAsync = persistAsync;
        _localize = localize;

        _section.ReadProfiles.ItemsSource = _state.Read;
        _section.WriteProfiles.ItemsSource = _state.Write;
        _section.ConvertProfiles.ItemsSource = _state.Convert;
        _section.RenameRequested += Rename;
        _section.DeleteRequested += Delete;
        _section.ProfileKeyDown += KeyDown;
        _section.ProfileLeftButtonDown += LeftButtonDown;
        _section.ProfileRightButtonDown += RightButtonDown;
    }

    public ObservableCollection<ProfileOptionRow> Read => _state.Read;
    public ObservableCollection<ProfileOptionRow> Write => _state.Write;
    public ObservableCollection<ProfileOptionRow> Convert => _state.Convert;

    public void ApplyTo(AppSettings settings) => _state.ApplyTo(settings);

    private async void Rename(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile(sender) is not ProfileOptionRow row) return;
        var dialog = new ProfileNameWindow(row.Name) { Owner = _owner };
        if (dialog.ShowDialog() != true) return;
        if (_state.ContainsName(row, dialog.ProfileName))
        {
            MessageBox.Show(_owner, Localize("Profile.DuplicateName"), Localize("Profile.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _state.Rename(row, dialog.ProfileName);
        await _persistAsync();
    }

    private async void Delete(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile(sender) is not ProfileOptionRow row) return;
        if (MessageBox.Show(_owner, Localize("Profile.DeleteConfirm", row.Name), Localize("Profile.Title"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        _state.For(row.Operation).Remove(row);
        await _persistAsync();
    }

    private void KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2)
        {
            Rename(sender, new RoutedEventArgs());
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            Delete(sender, new RoutedEventArgs());
            e.Handled = true;
        }
    }

    private void LeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox list ||
            ItemsControl.ContainerFromElement(list, e.OriginalSource as DependencyObject) is not ListBoxItem item ||
            item.DataContext is not ProfileOptionRow row) return;
        var now = DateTime.UtcNow;
        var delay = now - _lastClickAt;
        if (Equals(list.SelectedItem, row) && _lastClick == row &&
            delay >= TimeSpan.FromMilliseconds(450) && delay <= TimeSpan.FromSeconds(1.5))
        {
            Rename(list, new RoutedEventArgs());
            _lastClick = null;
            e.Handled = true;
            return;
        }

        _lastClick = row;
        _lastClickAt = now;
    }

    private static void RightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox list &&
            ItemsControl.ContainerFromElement(list, e.OriginalSource as DependencyObject) is ListBoxItem item)
            item.IsSelected = true;
    }

    private ProfileOptionRow? SelectedProfile(object sender)
    {
        if (sender is ListBox list) return list.SelectedItem as ProfileOptionRow;
        if (sender is MenuItem { Parent: ContextMenu context } && context.PlacementTarget is ListBox contextList)
            return contextList.SelectedItem as ProfileOptionRow;
        return _section.ReadProfiles.SelectedItem as ProfileOptionRow ??
               _section.WriteProfiles.SelectedItem as ProfileOptionRow ??
               _section.ConvertProfiles.SelectedItem as ProfileOptionRow;
    }

    private string Localize(string key, params object[] arguments) => _localize(key, arguments);
}
