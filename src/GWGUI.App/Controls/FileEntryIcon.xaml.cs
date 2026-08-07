using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Controls;

public enum ExplorerIconKind { Folder, Text, Image, Audio, Archive, Program, DiskImage, Link, File }

public partial class FileEntryIcon : UserControl
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind), typeof(ExplorerIconKind), typeof(FileEntryIcon),
        new PropertyMetadata(ExplorerIconKind.File, (owner, _) => ((FileEntryIcon)owner).Refresh()));

    public FileEntryIcon()
    {
        InitializeComponent();
        Refresh();
    }

    public ExplorerIconKind Kind
    {
        get => (ExplorerIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    private void Refresh()
    {
        if (!IsInitialized) return;
        FolderIcon.Visibility = Kind == ExplorerIconKind.Folder ? Visibility.Visible : Visibility.Collapsed;
        LinkMark.Visibility = Kind == ExplorerIconKind.Link ? Visibility.Visible : Visibility.Collapsed;
        FileIcon.Visibility = Kind is not ExplorerIconKind.Folder and not ExplorerIconKind.Link ? Visibility.Visible : Visibility.Collapsed;
        TypeMark.Data = Geometry.Parse(Kind switch
        {
            ExplorerIconKind.Text => "M1,1 L9,1 M1,4 L9,4 M1,7 L7,7",
            ExplorerIconKind.Image => "M1,7 L4,4 L6,6 L8,3 L10,7 Z M2,2 A1,1 0 1 0 2.1,2",
            ExplorerIconKind.Audio => "M4,1 L9,0 L9,6 C7,5 6,6 6,7 C6,9 10,9 10,6 L10,0 L4,1 L4,6 C2,5 1,6 1,7 C1,9 5,9 5,6 Z",
            ExplorerIconKind.Archive => "M1,1 L9,1 L9,8 L1,8 Z M4,1 L6,1 L6,3 L4,3 Z M4,4 L6,4 L6,6 L4,6 Z",
            ExplorerIconKind.Program => "M2,2 L8,2 L8,7 L2,7 Z M3,4 L4,5 L3,6 M5,6 L7,6",
            ExplorerIconKind.DiskImage => "M1,1 L9,1 L9,9 L1,9 Z M3,1 L7,1 L7,4 L3,4 Z M3,6 L7,6 L7,8 L3,8 Z",
            _ => "M2,2 L8,2 M2,5 L8,5 M2,8 L6,8"
        });
    }
}
