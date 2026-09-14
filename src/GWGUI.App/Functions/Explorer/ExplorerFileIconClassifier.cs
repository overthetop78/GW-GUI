using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Dictionaries.Explorer.FileTypes;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Localization.Extensions;
using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;


namespace GWGUI.App.Functions.Explorer;

public static class ExplorerFileIconClassifier
{
    public static ExplorerFileSystemFamily FamilyFor(ExploredDiskImage document) =>
        ExplorerFileSystemFamilyResolver.Resolve(document);

    public static ExplorerFileSystemFamily FamilyFor(string formatId, string? fileSystemId) =>
        ExplorerFileSystemFamilyResolver.Resolve(formatId, fileSystemId);

    public static ExplorerFileTypeDefinition DefinitionFor(FileSystemEntry entry, ExplorerFileSystemFamily family = ExplorerFileSystemFamily.Unknown)
    {
        if (entry.Kind == FileSystemEntryKind.Directory) return Synthetic(ExplorerFileCategory.File, "Explorer.Directory", ExplorerIconCategory.Folder);
        if (entry.Kind == FileSystemEntryKind.Link) return Synthetic(ExplorerFileCategory.Link, "Explorer.Link", ExplorerIconCategory.Link);
        var extension = Path.GetExtension(entry.Name);
        if (entry.Content is { Count: 0 })
            return Synthetic(ExplorerFileCategory.File,
                extension.Length == 0 ? "Explorer.File" : "Explorer.FileWithExtension",
                ExplorerIconCategory.File, extension, ExplorerContentFormat.Empty);
        var known = ExplorerFileTypeCatalog.Find(family, extension);
        var contentCategory = ExplorerFileContentClassifier.KnownCategory(entry, family);
        if (contentCategory is not null && (known is null || known.Category is ExplorerFileCategory.File or ExplorerFileCategory.Data))
        {
            var encoding = contentCategory == ExplorerFileCategory.Text && family == ExplorerFileSystemFamily.Atari8Bit
                ? ExplorerTextEncoding.Atascii
                : ExplorerTextEncoding.NotApplicable;
            return ExplorerFileTypeRuleFactory.Rule(family, extension, contentCategory.Value,
                encoding,
                execution: contentCategory == ExplorerFileCategory.Executable ? ExplorerExecutionKind.NativeExecutable : ExplorerExecutionKind.None);
        }
        if (known is not null) return known;
        if (ExplorerFileContentClassifier.LooksLikeText(entry.Content))
            return ExplorerFileTypeRuleFactory.Rule(family, extension, ExplorerFileCategory.Text, ExplorerTextEncoding.Unknown);
        return Synthetic(ExplorerFileCategory.File, extension.Length == 0 ? "Explorer.File" : "Explorer.FileWithExtension", ExplorerIconCategory.File, extension);
    }

    public static ExplorerIconCategory IconFor(FileSystemEntry entry, ExplorerFileSystemFamily family = ExplorerFileSystemFamily.Unknown) =>
        DefinitionFor(entry, family).IconCategory;

    public static string TypeTextFor(ExplorerFileTypeDefinition definition) =>
        definition.TypeResourceKey == "Explorer.FileWithExtension"
            ? LocExtension.Get(definition.TypeResourceKey, definition.Extension.TrimStart('.').ToUpperInvariant())
            : LocExtension.Get(definition.TypeResourceKey);

    public static string TypeResourceKeyFor(ExplorerIconCategory category) => category switch
    {
        ExplorerIconCategory.Folder => "Explorer.Directory",
        ExplorerIconCategory.Text => "Explorer.Type.Text",
        ExplorerIconCategory.Document => "Explorer.Type.Document",
        ExplorerIconCategory.SourceCode => "Explorer.Type.SourceCode",
        ExplorerIconCategory.BasicProgram => "Explorer.Type.BasicProgram",
        ExplorerIconCategory.Executable => "Explorer.Type.Executable",
        ExplorerIconCategory.Command => "Explorer.Type.Command",
        ExplorerIconCategory.System => "Explorer.Type.SystemFile",
        ExplorerIconCategory.ObjectCode => "Explorer.Type.ObjectCode",
        ExplorerIconCategory.Data => "Explorer.Type.Data",
        ExplorerIconCategory.Configuration => "Explorer.Type.Configuration",
        ExplorerIconCategory.Library => "Explorer.Type.Library",
        ExplorerIconCategory.Image => "Explorer.Type.Image",
        ExplorerIconCategory.Audio => "Explorer.Type.Audio",
        ExplorerIconCategory.Media => "Explorer.Type.Media",
        ExplorerIconCategory.Archive => "Explorer.Type.Archive",
        ExplorerIconCategory.Program => "Explorer.Type.Program",
        ExplorerIconCategory.DiskImage => "Explorer.Type.DiskImage",
        ExplorerIconCategory.Link => "Explorer.Link",
        ExplorerIconCategory.Font => "Explorer.Type.Font",
        _ => "Explorer.File"
    };

    private static ExplorerFileTypeDefinition Synthetic(
        ExplorerFileCategory category,
        string resourceKey,
        ExplorerIconCategory icon,
        string extension = "",
        ExplorerContentFormat contentFormat = ExplorerContentFormat.Unknown) =>
        new(null, ExplorerFileTypeRuleFactory.Normalize(extension), category, resourceKey,
            ExplorerExecutionKind.None, icon, contentFormat,
            ExplorerTextEncoding.NotApplicable, ExplorerPreviewKind.None);

}
