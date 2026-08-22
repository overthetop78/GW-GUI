using GWGUI.App.Contracts.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Dialogs;
using System.Windows;
using Microsoft.Win32;

namespace GWGUI.App.Services.Dialogs;

public sealed class WpfFileDialogService(Window owner) : IFileDialogService
{
    public string? OpenFile(OpenFileRequest request)
    {
        var dialog = new OpenFileDialog { Filter = request.Filter };
        if (!string.IsNullOrWhiteSpace(request.InitialDirectory)) dialog.InitialDirectory = request.InitialDirectory;
        if (!string.IsNullOrWhiteSpace(request.FileName)) dialog.FileName = request.FileName;
        return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
    }

    public string? SaveFile(SaveFileRequest request)
    {
        var dialog = new SaveFileDialog { Filter = request.Filter, FileName = request.FileName };
        if (!string.IsNullOrWhiteSpace(request.DefaultExtension)) dialog.DefaultExt = request.DefaultExtension;
        return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
    }

    public string? SelectFolder(SelectFolderRequest request)
    {
        var dialog = new OpenFolderDialog { Title = request.Title };
        if (!string.IsNullOrWhiteSpace(request.InitialDirectory)) dialog.InitialDirectory = request.InitialDirectory;
        return dialog.ShowDialog(owner) == true ? dialog.FolderName : null;
    }
}
