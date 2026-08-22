using GWGUI.App.Contracts.Services.Dialogs;
namespace GWGUI.App.Interfaces.Services.Dialogs;

public interface IFileDialogService
{
    string? OpenFile(OpenFileRequest request);
    string? SaveFile(SaveFileRequest request);
    string? SelectFolder(SelectFolderRequest request);
}
