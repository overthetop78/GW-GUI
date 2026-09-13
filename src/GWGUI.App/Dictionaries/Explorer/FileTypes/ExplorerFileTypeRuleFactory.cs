using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

internal static class ExplorerFileTypeRuleFactory
{
    public static ExplorerFileTypeDefinition Rule(
        ExplorerFileSystemFamily? family,
        string extension,
        ExplorerFileCategory category,
        ExplorerTextEncoding encoding = ExplorerTextEncoding.NotApplicable,
        ExplorerExecutionKind execution = ExplorerExecutionKind.None)
    {
        var normalized = Normalize(extension);
        var (resourceKey, icon, content, preview) = Presentation(category);
        if (encoding == ExplorerTextEncoding.NotApplicable && content is ExplorerContentFormat.PlainText or ExplorerContentFormat.Configuration)
            encoding = ExplorerTextEncoding.Unknown;
        return new(family, normalized, category, resourceKey, execution, icon, content, encoding, preview);
    }

    public static string Normalize(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return string.Empty;
        var trimmed = extension.Trim();
        return (trimmed[0] == '.' ? trimmed : $".{trimmed}").ToLowerInvariant();
    }

    private static (string ResourceKey, ExplorerIconCategory Icon, ExplorerContentFormat Content, ExplorerPreviewKind Preview)
        Presentation(ExplorerFileCategory category) => category switch
        {
            ExplorerFileCategory.Text => ("Explorer.Type.Text", ExplorerIconCategory.Text, ExplorerContentFormat.PlainText, ExplorerPreviewKind.Text),
            ExplorerFileCategory.Document => ("Explorer.Type.Document", ExplorerIconCategory.Document, ExplorerContentFormat.PlainText, ExplorerPreviewKind.Text),
            ExplorerFileCategory.SourceCode => ("Explorer.Type.SourceCode", ExplorerIconCategory.SourceCode, ExplorerContentFormat.PlainText, ExplorerPreviewKind.Text),
            ExplorerFileCategory.BasicProgram => ("Explorer.Type.BasicProgram", ExplorerIconCategory.BasicProgram, ExplorerContentFormat.BasicProgram, ExplorerPreviewKind.BasicListing),
            ExplorerFileCategory.BootProgram => ("Explorer.Type.BootProgram", ExplorerIconCategory.System, ExplorerContentFormat.SystemBinary, ExplorerPreviewKind.Hexadecimal),
            ExplorerFileCategory.Program => ("Explorer.Type.Program", ExplorerIconCategory.Program, ExplorerContentFormat.Unknown, ExplorerPreviewKind.None),
            ExplorerFileCategory.Executable => ("Explorer.Type.Executable", ExplorerIconCategory.Executable, ExplorerContentFormat.NativeExecutable, ExplorerPreviewKind.Hexadecimal),
            ExplorerFileCategory.Command => ("Explorer.Type.Command", ExplorerIconCategory.Command, ExplorerContentFormat.CommandScript, ExplorerPreviewKind.Text),
            ExplorerFileCategory.System => ("Explorer.Type.SystemFile", ExplorerIconCategory.System, ExplorerContentFormat.SystemBinary, ExplorerPreviewKind.Hexadecimal),
            ExplorerFileCategory.ObjectCode => ("Explorer.Type.ObjectCode", ExplorerIconCategory.ObjectCode, ExplorerContentFormat.ObjectCode, ExplorerPreviewKind.Hexadecimal),
            ExplorerFileCategory.Data => ("Explorer.Type.Data", ExplorerIconCategory.Data, ExplorerContentFormat.StructuredData, ExplorerPreviewKind.Hexadecimal),
            ExplorerFileCategory.Configuration => ("Explorer.Type.Configuration", ExplorerIconCategory.Configuration, ExplorerContentFormat.Configuration, ExplorerPreviewKind.Text),
            ExplorerFileCategory.Library => ("Explorer.Type.Library", ExplorerIconCategory.Library, ExplorerContentFormat.Library, ExplorerPreviewKind.Hexadecimal),
            ExplorerFileCategory.Image => ("Explorer.Type.Image", ExplorerIconCategory.Image, ExplorerContentFormat.Image, ExplorerPreviewKind.Image),
            ExplorerFileCategory.Audio => ("Explorer.Type.Audio", ExplorerIconCategory.Audio, ExplorerContentFormat.Audio, ExplorerPreviewKind.Audio),
            ExplorerFileCategory.Media => ("Explorer.Type.Media", ExplorerIconCategory.Media, ExplorerContentFormat.Unknown, ExplorerPreviewKind.ExternalApplication),
            ExplorerFileCategory.Archive => ("Explorer.Type.Archive", ExplorerIconCategory.Archive, ExplorerContentFormat.Archive, ExplorerPreviewKind.None),
            ExplorerFileCategory.DiskImage => ("Explorer.Type.DiskImage", ExplorerIconCategory.DiskImage, ExplorerContentFormat.DiskImage, ExplorerPreviewKind.None),
            ExplorerFileCategory.Link => ("Explorer.Link", ExplorerIconCategory.Link, ExplorerContentFormat.Unknown, ExplorerPreviewKind.None),
            ExplorerFileCategory.Font => ("Explorer.Type.Font", ExplorerIconCategory.Font, ExplorerContentFormat.Font, ExplorerPreviewKind.ExternalApplication),
            _ => ("Explorer.File", ExplorerIconCategory.File, ExplorerContentFormat.Unknown, ExplorerPreviewKind.None)
        };
}
