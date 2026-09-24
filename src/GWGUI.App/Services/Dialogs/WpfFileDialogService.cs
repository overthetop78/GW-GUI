using GWGUI.App.Contracts.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Dialogs;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace GWGUI.App.Services.Dialogs;

public sealed class WpfFileDialogService(Window owner) : IFileDialogService
{
    public string? OpenFile(OpenFileRequest request)
    {
        var dialog = new OpenFileDialog { Filter = request.Filter };
        var initialDirectory = GetDialogDirectory(request.InitialDirectory);
        if (initialDirectory is not null) dialog.InitialDirectory = initialDirectory;
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
        var initialDirectory = GetDialogDirectory(request.InitialDirectory);
        if (initialDirectory is not null) dialog.InitialDirectory = initialDirectory;
        return dialog.ShowDialog(owner) == true ? dialog.FolderName : null;
    }

    internal static string? GetDialogDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return null;
        return NormalizeDialogPath(path);
    }

    internal static string NormalizeDialogPath(string path)
    {
        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase)) return @"\\" + path[8..];
        return path.StartsWith(@"\\?\", StringComparison.Ordinal) ? path[4..] : path;
    }
}
