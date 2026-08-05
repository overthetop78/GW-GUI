using System.Windows;
using Microsoft.Win32;

namespace GWGUI.App.Services;

public sealed record OpenFileRequest(string Filter, string? InitialDirectory = null, string? FileName = null);
public sealed record SaveFileRequest(string Filter, string FileName, string? DefaultExtension = null);
public sealed record SelectFolderRequest(string Title, string? InitialDirectory = null);

public interface IFileDialogService
{
    string? OpenFile(OpenFileRequest request);
    string? SaveFile(SaveFileRequest request);
    string? SelectFolder(SelectFolderRequest request);
}

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
