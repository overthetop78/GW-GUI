using System.Windows;

namespace GWGUI.App.Services;

public enum UserDialogButtons { Ok, OkCancel, YesNo, YesNoCancel }
public enum UserDialogIcon { None, Information, Warning, Error, Question }
public enum UserDialogResult { None, Ok, Cancel, Yes, No }

public interface IMessageDialogService
{
    UserDialogResult Show(string message, string title, UserDialogButtons buttons = UserDialogButtons.Ok, UserDialogIcon icon = UserDialogIcon.None);
}

public sealed class WpfMessageDialogService(Window owner) : IMessageDialogService
{
    public UserDialogResult Show(string message, string title, UserDialogButtons buttons = UserDialogButtons.Ok, UserDialogIcon icon = UserDialogIcon.None)
    {
        var result = MessageBox.Show(owner, message, title, buttons switch
        {
            UserDialogButtons.OkCancel => MessageBoxButton.OKCancel,
            UserDialogButtons.YesNo => MessageBoxButton.YesNo,
            UserDialogButtons.YesNoCancel => MessageBoxButton.YesNoCancel,
            _ => MessageBoxButton.OK
        }, icon switch
        {
            UserDialogIcon.Information => MessageBoxImage.Information,
            UserDialogIcon.Warning => MessageBoxImage.Warning,
            UserDialogIcon.Error => MessageBoxImage.Error,
            UserDialogIcon.Question => MessageBoxImage.Question,
            _ => MessageBoxImage.None
        });
        return result switch
        {
            MessageBoxResult.OK => UserDialogResult.Ok,
            MessageBoxResult.Cancel => UserDialogResult.Cancel,
            MessageBoxResult.Yes => UserDialogResult.Yes,
            MessageBoxResult.No => UserDialogResult.No,
            _ => UserDialogResult.None
        };
    }
}
