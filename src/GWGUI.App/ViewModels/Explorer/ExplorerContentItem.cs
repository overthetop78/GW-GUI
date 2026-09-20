using FileSystemEntryKind = GWGUI.MediaEngine.Enums.FileSystemEntryKind;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.MediaEngine.Contracts.Explorer;


namespace GWGUI.App.ViewModels.Explorer;

public sealed class ExplorerContentItem
{
    public ExplorerContentItem(FileSystemEntry entry, ExplorerFileSystemFamily family = ExplorerFileSystemFamily.Unknown)
    {
        Entry = entry;
        Definition = ExplorerFileIconClassifier.DefinitionFor(entry, family);
        IconCategory = Definition.IconCategory;
        TypeText = ExplorerFileIconClassifier.TypeTextFor(Definition);
        Tone = Definition.ExecutionKind != ExplorerExecutionKind.None
            ? ExplorerEntryTone.Executable
            : Definition.Category == ExplorerFileCategory.System || IsMarked(entry, "Hidden") || IsMarked(entry, "System")
                ? ExplorerEntryTone.Muted
                : ExplorerEntryTone.Default;
    }

    public FileSystemEntry Entry { get; }
    public GWGUI.App.Contracts.Explorer.ExplorerFileTypeDefinition Definition { get; }
    public string Name => DisplayName();
    public ExplorerIconCategory IconCategory { get; }
    public string TypeText { get; }
    public ExplorerEntryTone Tone { get; }
    public string SizeText => Entry.Kind == FileSystemEntryKind.Directory ? string.Empty : StorageSizeFormatter.FormatBytes(Entry.Size);
    public string ModifiedText => Entry.Modified?.LocalDateTime.ToString("g") ?? "\u2014";

    private string DisplayName()
    {
        if (!Entry.SyntheticName
            || !Entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
            || !sourceKind.Equals("atari-cas-records", StringComparison.Ordinal))
            return Entry.Name;
        var ordinal = Entry.Metadata.TryGetValue("logicalFileOrdinal", out var value)
            && int.TryParse(value, out var parsedOrdinal)
                ? Math.Max(1, parsedOrdinal)
                : 1;
        var suffix = ordinal == 1 ? string.Empty : $"-{ordinal:D4}";
        return Definition.Category switch
        {
            ExplorerFileCategory.BasicProgram => $"RUN{suffix}.BAS",
            ExplorerFileCategory.Executable => $"RUN{suffix}.XEX",
            ExplorerFileCategory.BootProgram => $"BOOT{suffix}.BIN",
            _ => $"DATA-{ordinal:D4}.BIN"
        };
    }

    private static bool IsMarked(FileSystemEntry entry, string value) =>
        entry.Attributes.Any(attribute => attribute.Equals(value, StringComparison.OrdinalIgnoreCase));
}
