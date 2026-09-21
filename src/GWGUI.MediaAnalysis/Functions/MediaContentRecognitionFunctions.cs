using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Functions;

internal static class MediaContentRecognitionFunctions
{
    public static string NormalizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return string.Empty;
        var trimmed = extension.Trim();
        return (trimmed[0] == '.' ? trimmed : $".{trimmed}").ToLowerInvariant();
    }

    public static MediaContentTypeDefinition ToDefinition(
        MediaContentRecognitionRule rule,
        string? actualExtension = null)
    {
        var (resourceKey, icon, contentFormat) = Presentation(rule.Category);
        var extension = rule.Extension.Length > 0
            ? rule.Extension
            : NormalizeExtension(actualExtension);
        return new(rule.Family, extension, rule.Category, resourceKey,
            rule.ExecutionKind, icon, contentFormat,
            rule.TextEncoding, rule.PreviewKind);
    }

    private static (string ResourceKey, string Icon, MediaContentFormat ContentFormat)
        Presentation(MediaContentCategory category) => category switch
        {
            MediaContentCategory.Text => ("Explorer.Type.Text", MediaContentIconIds.Text, MediaContentFormat.PlainText),
            MediaContentCategory.Document => ("Explorer.Type.Document", MediaContentIconIds.Document, MediaContentFormat.PlainText),
            MediaContentCategory.SourceCode => ("Explorer.Type.SourceCode", MediaContentIconIds.SourceCode, MediaContentFormat.PlainText),
            MediaContentCategory.BasicProgram => ("Explorer.Type.BasicProgram", MediaContentIconIds.BasicProgram, MediaContentFormat.BasicProgram),
            MediaContentCategory.BootProgram => ("Explorer.Type.BootProgram", MediaContentIconIds.System, MediaContentFormat.SystemBinary),
            MediaContentCategory.Program => ("Explorer.Type.Program", MediaContentIconIds.Program, MediaContentFormat.Unknown),
            MediaContentCategory.Executable => ("Explorer.Type.Executable", MediaContentIconIds.Executable, MediaContentFormat.NativeExecutable),
            MediaContentCategory.Command => ("Explorer.Type.Command", MediaContentIconIds.Command, MediaContentFormat.CommandScript),
            MediaContentCategory.System => ("Explorer.Type.SystemFile", MediaContentIconIds.System, MediaContentFormat.SystemBinary),
            MediaContentCategory.ObjectCode => ("Explorer.Type.ObjectCode", MediaContentIconIds.ObjectCode, MediaContentFormat.ObjectCode),
            MediaContentCategory.Data => ("Explorer.Type.Data", MediaContentIconIds.Data, MediaContentFormat.StructuredData),
            MediaContentCategory.Configuration => ("Explorer.Type.Configuration", MediaContentIconIds.Configuration, MediaContentFormat.Configuration),
            MediaContentCategory.Library => ("Explorer.Type.Library", MediaContentIconIds.Library, MediaContentFormat.Library),
            MediaContentCategory.Image => ("Explorer.Type.Image", MediaContentIconIds.Image, MediaContentFormat.Image),
            MediaContentCategory.Audio => ("Explorer.Type.Audio", MediaContentIconIds.Audio, MediaContentFormat.Audio),
            MediaContentCategory.Media => ("Explorer.Type.Media", MediaContentIconIds.Media, MediaContentFormat.Media),
            MediaContentCategory.Archive => ("Explorer.Type.Archive", MediaContentIconIds.Archive, MediaContentFormat.Archive),
            MediaContentCategory.DiskImage => ("Explorer.Type.DiskImage", MediaContentIconIds.DiskImage, MediaContentFormat.DiskImage),
            MediaContentCategory.Link => ("Explorer.Link", MediaContentIconIds.Link, MediaContentFormat.Unknown),
            MediaContentCategory.Font => ("Explorer.Type.Font", MediaContentIconIds.Font, MediaContentFormat.Font),
            _ => ("Explorer.File", MediaContentIconIds.File, MediaContentFormat.Unknown)
        };
}
