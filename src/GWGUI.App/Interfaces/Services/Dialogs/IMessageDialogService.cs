using GWGUI.App.Enums.Services.Dialogs;
namespace GWGUI.App.Interfaces.Services.Dialogs;

public interface IMessageDialogService
{
    UserDialogResult Show(string message, string title, UserDialogButtons buttons = UserDialogButtons.Ok, UserDialogIcon icon = UserDialogIcon.None);
}
