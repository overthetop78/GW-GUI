using GWGUI.App.Constants.Files;
using GWGUI.App.Enums.Explorer;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.App.Views.Controls.Common;

public partial class FileEntryIcon : UserControl
{
    public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
        nameof(Category), typeof(ExplorerIconCategory), typeof(FileEntryIcon),
        new PropertyMetadata(ExplorerIconCategory.File, (owner, _) => ((FileEntryIcon)owner).Refresh()));

    public FileEntryIcon()
    {
        InitializeComponent();
        Refresh();
    }

    public ExplorerIconCategory Category
    {
        get => (ExplorerIconCategory)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    private void Refresh()
    {
        if (!IsInitialized) return;
        FolderIcon.Visibility = Category == ExplorerIconCategory.Folder ? Visibility.Visible : Visibility.Collapsed;
        LinkMark.Visibility = Category == ExplorerIconCategory.Link ? Visibility.Visible : Visibility.Collapsed;
        FileIcon.Visibility = Category is not ExplorerIconCategory.Folder and not ExplorerIconCategory.Link ? Visibility.Visible : Visibility.Collapsed;
        TypeMark.Data = Geometry.Parse(Category switch
        {
            ExplorerIconCategory.Text => FileEntryIconGeometryConstants.Text,
            ExplorerIconCategory.Image => FileEntryIconGeometryConstants.Image,
            ExplorerIconCategory.Audio => FileEntryIconGeometryConstants.Audio,
            ExplorerIconCategory.Archive => FileEntryIconGeometryConstants.Archive,
            ExplorerIconCategory.Program => FileEntryIconGeometryConstants.Program,
            ExplorerIconCategory.DiskImage => FileEntryIconGeometryConstants.DiskImage,
            _ => FileEntryIconGeometryConstants.File
        });
    }
}
