using System.IO;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images;

namespace GWGUI.App.Controls;

public static class ExplorerFileIconClassifier
{
    public static ExplorerFileSystemFamily FamilyFor(ExploredDiskImage document) =>
        ExplorerFileSystemFamilyResolver.Resolve(document);

    public static ExplorerIconKind IconFor(FileSystemEntry entry, ExplorerFileSystemFamily family = ExplorerFileSystemFamily.Unknown)
    {
        if (entry.Kind == FileSystemEntryKind.Directory) return ExplorerIconKind.Folder;
        if (entry.Kind == FileSystemEntryKind.Link) return ExplorerIconKind.Link;
        var extension = Path.GetExtension(entry.Name);
        var profile = ExplorerFileTypeProfileCatalog.For(family);

        var contentIcon = ExplorerFileContentClassifier.KnownIcon(entry, family);
        if (contentIcon is not null) return contentIcon.Value;
        if (profile.Programs.Contains(extension)) return ExplorerIconKind.Program;
        if (profile.Images.Contains(extension)) return ExplorerIconKind.Image;
        if (profile.Audio.Contains(extension)) return ExplorerIconKind.Audio;
        if (profile.Archives.Contains(extension)) return ExplorerIconKind.Archive;
        if (profile.DiskImages.Contains(extension)) return ExplorerIconKind.DiskImage;
        if (profile.Text.Contains(extension) || ExplorerFileContentClassifier.LooksLikeText(entry.Content)) return ExplorerIconKind.Text;
        return ExplorerIconKind.File;
    }

    public static string TypeResourceKeyFor(ExplorerIconKind kind) => kind switch
    {
        ExplorerIconKind.Folder => "Explorer.Directory",
        ExplorerIconKind.Text => "Explorer.Type.Text",
        ExplorerIconKind.Image => "Explorer.Type.Image",
        ExplorerIconKind.Audio => "Explorer.Type.Audio",
        ExplorerIconKind.Archive => "Explorer.Type.Archive",
        ExplorerIconKind.Program => "Explorer.Type.Program",
        ExplorerIconKind.DiskImage => "Explorer.Type.DiskImage",
        ExplorerIconKind.Link => "Explorer.Link",
        _ => "Explorer.File"
    };

}
