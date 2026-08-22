using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Dialogs;
using System.Windows;

namespace GWGUI.App.Services.Dialogs;

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
