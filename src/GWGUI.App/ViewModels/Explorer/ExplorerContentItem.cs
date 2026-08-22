using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.MediaEngine.FileSystems;


namespace GWGUI.App.ViewModels.Explorer;

public sealed class ExplorerContentItem
{
    public ExplorerContentItem(FileSystemEntry entry, ExplorerFileSystemFamily family = ExplorerFileSystemFamily.Unknown)
    {
        Entry = entry;
        IconCategory = ExplorerFileIconClassifier.IconFor(entry, family);
        TypeText = LocExtension.Get(ExplorerFileIconClassifier.TypeResourceKeyFor(IconCategory));
    }

    public FileSystemEntry Entry { get; }
    public string Name => Entry.Name;
    public ExplorerIconCategory IconCategory { get; }
    public string TypeText { get; }
    public string SizeText => Entry.Kind == FileSystemEntryKind.Directory ? string.Empty : StorageSizeFormatter.FormatBytes(Entry.Size);
    public string ModifiedText => Entry.Modified?.LocalDateTime.ToString("g") ?? "\u2014";
}
