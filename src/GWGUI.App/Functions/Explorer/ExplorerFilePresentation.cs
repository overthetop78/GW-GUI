using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Localization.Extensions;
using GWGUI.MediaEngine.Contracts.Explorer;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.App.Functions.Explorer;

/// <summary>Traduit les identifiants fournis par MediaEngine en valeurs d'affichage.</summary>
public static class ExplorerFilePresentation
{
    public static ExplorerFileTypeDefinition DefinitionFor(FileSystemEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.Analysis is not { } analysis) return Generic(entry.Kind);
        return new ExplorerFileTypeDefinition(
            analysis.Extension,
            Parse<ExplorerFileCategory>(analysis.CategoryId, ExplorerFileCategory.File),
            analysis.TypeResourceKey,
            Parse<ExplorerExecutionKind>(analysis.ExecutionKindId, ExplorerExecutionKind.None),
            IconFor(analysis.IconId),
            Parse<ExplorerContentFormat>(analysis.ContentFormatId, ExplorerContentFormat.Unknown),
            Parse<ExplorerTextEncoding>(analysis.TextEncodingId, ExplorerTextEncoding.NotApplicable),
            Parse<ExplorerPreviewKind>(analysis.PreviewKindId, ExplorerPreviewKind.None));
    }

    public static string TypeTextFor(ExplorerFileTypeDefinition definition) =>
        definition.TypeResourceKey == "Explorer.FileWithExtension"
            ? LocExtension.Get(definition.TypeResourceKey, definition.Extension.TrimStart('.').ToUpperInvariant())
            : LocExtension.Get(definition.TypeResourceKey);

    private static ExplorerFileTypeDefinition Generic(FileSystemEntryKind kind)
    {
        var (category, key, icon) = kind switch
        {
            FileSystemEntryKind.Directory => (ExplorerFileCategory.File, "Explorer.Directory", ExplorerIconCategory.Folder),
            FileSystemEntryKind.Link => (ExplorerFileCategory.Link, "Explorer.Link", ExplorerIconCategory.Link),
            _ => (ExplorerFileCategory.File, "Explorer.File", ExplorerIconCategory.File)
        };
        return new ExplorerFileTypeDefinition(string.Empty, category, key,
            ExplorerExecutionKind.None, icon, ExplorerContentFormat.Unknown,
            ExplorerTextEncoding.NotApplicable, ExplorerPreviewKind.None);
    }

    private static T Parse<T>(string id, T fallback) where T : struct, Enum =>
        Enum.TryParse<T>(id, false, out var value) && Enum.IsDefined(value) ? value : fallback;

    private static ExplorerIconCategory IconFor(string id) => id switch
    {
        "folder" => ExplorerIconCategory.Folder,
        "text" => ExplorerIconCategory.Text,
        "document" => ExplorerIconCategory.Document,
        "source-code" => ExplorerIconCategory.SourceCode,
        "basic-program" => ExplorerIconCategory.BasicProgram,
        "executable" => ExplorerIconCategory.Executable,
        "command" => ExplorerIconCategory.Command,
        "system" => ExplorerIconCategory.System,
        "object-code" => ExplorerIconCategory.ObjectCode,
        "data" => ExplorerIconCategory.Data,
        "configuration" => ExplorerIconCategory.Configuration,
        "library" => ExplorerIconCategory.Library,
        "image" => ExplorerIconCategory.Image,
        "audio" => ExplorerIconCategory.Audio,
        "media" => ExplorerIconCategory.Media,
        "archive" => ExplorerIconCategory.Archive,
        "disk-image" => ExplorerIconCategory.DiskImage,
        "link" => ExplorerIconCategory.Link,
        "font" => ExplorerIconCategory.Font,
        "program" => ExplorerIconCategory.Program,
        _ => ExplorerIconCategory.File
    };
}
