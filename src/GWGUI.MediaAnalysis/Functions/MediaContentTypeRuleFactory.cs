using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Functions;

internal static class MediaContentTypeRuleFactory
{
    public static MediaContentTypeDefinition Rule(
        MediaFileSystemFamily? family,
        string extension,
        MediaContentCategory category,
        MediaTextEncoding encoding = MediaTextEncoding.NotApplicable,
        MediaExecutionKind execution = MediaExecutionKind.None)
    {
        var normalized = Normalize(extension);
        var (resourceKey, icon, content, preview) = Presentation(category);
        if (encoding == MediaTextEncoding.NotApplicable && content is MediaContentFormat.PlainText or MediaContentFormat.Configuration)
            encoding = MediaTextEncoding.Unknown;
        return new(family, normalized, category, resourceKey, execution, icon, content, encoding, preview);
    }

    public static string Normalize(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return string.Empty;
        var trimmed = extension.Trim();
        return (trimmed[0] == '.' ? trimmed : $".{trimmed}").ToLowerInvariant();
    }

    private static (string ResourceKey, string Icon, MediaContentFormat Content, MediaPreviewKind Preview)
        Presentation(MediaContentCategory category) => category switch
        {
            MediaContentCategory.Text => ("Explorer.Type.Text", MediaContentIconIds.Text, MediaContentFormat.PlainText, MediaPreviewKind.Text),
            MediaContentCategory.Document => ("Explorer.Type.Document", MediaContentIconIds.Document, MediaContentFormat.PlainText, MediaPreviewKind.Text),
            MediaContentCategory.SourceCode => ("Explorer.Type.SourceCode", MediaContentIconIds.SourceCode, MediaContentFormat.PlainText, MediaPreviewKind.Text),
            MediaContentCategory.BasicProgram => ("Explorer.Type.BasicProgram", MediaContentIconIds.BasicProgram, MediaContentFormat.BasicProgram, MediaPreviewKind.BasicListing),
            MediaContentCategory.BootProgram => ("Explorer.Type.BootProgram", MediaContentIconIds.System, MediaContentFormat.SystemBinary, MediaPreviewKind.Hexadecimal),
            MediaContentCategory.Program => ("Explorer.Type.Program", MediaContentIconIds.Program, MediaContentFormat.Unknown, MediaPreviewKind.None),
            MediaContentCategory.Executable => ("Explorer.Type.Executable", MediaContentIconIds.Executable, MediaContentFormat.NativeExecutable, MediaPreviewKind.Hexadecimal),
            MediaContentCategory.Command => ("Explorer.Type.Command", MediaContentIconIds.Command, MediaContentFormat.CommandScript, MediaPreviewKind.Text),
            MediaContentCategory.System => ("Explorer.Type.SystemFile", MediaContentIconIds.System, MediaContentFormat.SystemBinary, MediaPreviewKind.Hexadecimal),
            MediaContentCategory.ObjectCode => ("Explorer.Type.ObjectCode", MediaContentIconIds.ObjectCode, MediaContentFormat.ObjectCode, MediaPreviewKind.Hexadecimal),
            MediaContentCategory.Data => ("Explorer.Type.Data", MediaContentIconIds.Data, MediaContentFormat.StructuredData, MediaPreviewKind.Hexadecimal),
            MediaContentCategory.Configuration => ("Explorer.Type.Configuration", MediaContentIconIds.Configuration, MediaContentFormat.Configuration, MediaPreviewKind.Text),
            MediaContentCategory.Library => ("Explorer.Type.Library", MediaContentIconIds.Library, MediaContentFormat.Library, MediaPreviewKind.Hexadecimal),
            MediaContentCategory.Image => ("Explorer.Type.Image", MediaContentIconIds.Image, MediaContentFormat.Image, MediaPreviewKind.Image),
            MediaContentCategory.Audio => ("Explorer.Type.Audio", MediaContentIconIds.Audio, MediaContentFormat.Audio, MediaPreviewKind.Audio),
            MediaContentCategory.Media => ("Explorer.Type.Media", MediaContentIconIds.Media, MediaContentFormat.Media, MediaPreviewKind.ExternalApplication),
            MediaContentCategory.Archive => ("Explorer.Type.Archive", MediaContentIconIds.Archive, MediaContentFormat.Archive, MediaPreviewKind.None),
            MediaContentCategory.DiskImage => ("Explorer.Type.DiskImage", MediaContentIconIds.DiskImage, MediaContentFormat.DiskImage, MediaPreviewKind.None),
            MediaContentCategory.Link => ("Explorer.Link", MediaContentIconIds.Link, MediaContentFormat.Unknown, MediaPreviewKind.None),
            MediaContentCategory.Font => ("Explorer.Type.Font", MediaContentIconIds.Font, MediaContentFormat.Font, MediaPreviewKind.ExternalApplication),
            _ => ("Explorer.File", MediaContentIconIds.File, MediaContentFormat.Unknown, MediaPreviewKind.None)
        };
}
