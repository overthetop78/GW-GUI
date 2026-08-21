using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.App.Enums;

namespace GWGUI.App.Controls;

public static class ExplorerFileIconClassifier
{
    public static ExplorerFileSystemFamily FamilyFor(ExploredDiskImage document) =>
        ExplorerFileSystemFamilyResolver.Resolve(document);

    public static ExplorerIconCategory IconFor(FileSystemEntry entry, ExplorerFileSystemFamily family = ExplorerFileSystemFamily.Unknown)
    {
        if (entry.Kind == FileSystemEntryKind.Directory) return ExplorerIconCategory.Folder;
        if (entry.Kind == FileSystemEntryKind.Link) return ExplorerIconCategory.Link;
        var extension = Path.GetExtension(entry.Name);
        var profile = ExplorerFileTypeProfileCatalog.For(family);

        var contentIcon = ExplorerFileContentClassifier.KnownIcon(entry, family);
        if (contentIcon is not null) return contentIcon.Value;
        if (profile.Programs.Contains(extension)) return ExplorerIconCategory.Program;
        if (profile.Images.Contains(extension)) return ExplorerIconCategory.Image;
        if (profile.Audio.Contains(extension)) return ExplorerIconCategory.Audio;
        if (profile.Archives.Contains(extension)) return ExplorerIconCategory.Archive;
        if (profile.DiskImages.Contains(extension)) return ExplorerIconCategory.DiskImage;
        if (profile.Text.Contains(extension) || ExplorerFileContentClassifier.LooksLikeText(entry.Content)) return ExplorerIconCategory.Text;
        return ExplorerIconCategory.File;
    }

    public static string TypeResourceKeyFor(ExplorerIconCategory category) => category switch
    {
        ExplorerIconCategory.Folder => "Explorer.Directory",
        ExplorerIconCategory.Text => "Explorer.Type.Text",
        ExplorerIconCategory.Image => "Explorer.Type.Image",
        ExplorerIconCategory.Audio => "Explorer.Type.Audio",
        ExplorerIconCategory.Archive => "Explorer.Type.Archive",
        ExplorerIconCategory.Program => "Explorer.Type.Program",
        ExplorerIconCategory.DiskImage => "Explorer.Type.DiskImage",
        ExplorerIconCategory.Link => "Explorer.Link",
        _ => "Explorer.File"
    };

}
