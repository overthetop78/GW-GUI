using FileSystemEntryKind = GWGUI.MediaEngine.Enums.FileSystemEntryKind;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.MediaEngine.Contracts.Explorer;


namespace GWGUI.App.ViewModels.Explorer;

public sealed class ExplorerContentItem
{
    public ExplorerContentItem(FileSystemEntry entry)
    {
        Entry = entry;
        Definition = ExplorerFilePresentation.DefinitionFor(entry);
        IconCategory = Definition.IconCategory;
        TypeText = ExplorerFilePresentation.TypeTextFor(Definition);
        Tone = Definition.ExecutionKind != ExplorerExecutionKind.None
            ? ExplorerEntryTone.Executable
            : Definition.Category == ExplorerFileCategory.System || IsMarked(entry, "Hidden") || IsMarked(entry, "System")
                ? ExplorerEntryTone.Muted
                : ExplorerEntryTone.Default;
    }

    public FileSystemEntry Entry { get; }
    public GWGUI.App.Contracts.Explorer.ExplorerFileTypeDefinition Definition { get; }
    public string Name => Entry.Name;
    public ExplorerIconCategory IconCategory { get; }
    public string TypeText { get; }
    public ExplorerEntryTone Tone { get; }
    public string SizeText => Entry.Kind == FileSystemEntryKind.Directory ? string.Empty : StorageSizeFormatter.FormatBytes(Entry.Size);
    public string ModifiedText => Entry.Modified?.LocalDateTime.ToString("g") ?? "\u2014";

    private static bool IsMarked(FileSystemEntry entry, string value) =>
        entry.Attributes.Any(attribute => attribute.Equals(value, StringComparison.OrdinalIgnoreCase));
}
