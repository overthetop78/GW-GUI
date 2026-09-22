using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Interfaces;

namespace GWGUI.MediaAnalysis.Functions;

/// <summary>Identifies an already extracted file from its own metadata and bytes.</summary>
public sealed class MediaContentClassifier : IMediaContentClassifier
{
    public MediaContentTypeDefinition? Classify(
        string extension,
        MediaEntryKind kind,
        string? nativeTypeId,
        string comment,
        bool? dataValid,
        IReadOnlyList<byte>? content,
        IReadOnlyDictionary<string, string> metadata,
        MediaFileSystemFamily family)
    {
        ArgumentNullException.ThrowIfNull(extension);
        ArgumentNullException.ThrowIfNull(comment);
        ArgumentNullException.ThrowIfNull(metadata);
        if (kind == MediaEntryKind.Directory)
            return Fallback(MediaContentCategory.File, "Explorer.Directory", MediaContentIconIds.Folder);
        if (kind == MediaEntryKind.Link)
            return Fallback(MediaContentCategory.Link, "Explorer.Link", MediaContentIconIds.Link);

        if (content is { Count: 0 })
            return Fallback(MediaContentCategory.File,
                extension.Length == 0 ? "Explorer.File" : "Explorer.FileWithExtension",
                MediaContentIconIds.File, extension, MediaContentFormat.Empty);

        var knownRule = MediaContentRecognitionCatalog.Find(family, extension, content);
        var known = knownRule is null
            ? null
            : MediaContentRecognitionFunctions.ToDefinition(knownRule, extension);
        var recognized = KnownCategory(comment, dataValid, family);
        if (recognized is not null && (known is null || known.Category is MediaContentCategory.File or MediaContentCategory.Data))
        {
            var encoding = recognized == MediaContentCategory.Text && family == MediaFileSystemFamily.Atari8Bit
                ? MediaTextEncoding.Atascii
                : MediaTextEncoding.NotApplicable;
            return MediaContentRecognitionFunctions.ToDefinition(
                new(family, MediaContentRecognitionFunctions.NormalizeExtension(extension), [],
                    recognized.Value, encoding,
                    recognized == MediaContentCategory.Executable
                        ? MediaExecutionKind.NativeExecutable
                        : MediaExecutionKind.None,
                    PreviewFor(recognized.Value)));
        }
        if (known is not null) return known;
        if (LooksLikeText(content, family))
            return MediaContentRecognitionFunctions.ToDefinition(
                new(family, MediaContentRecognitionFunctions.NormalizeExtension(extension), [],
                    MediaContentCategory.Text,
                    family == MediaFileSystemFamily.Atari8Bit
                        ? MediaTextEncoding.Atascii
                        : MediaTextEncoding.Unknown,
                    MediaExecutionKind.None, MediaPreviewKind.Text));
        return Fallback(MediaContentCategory.File,
            extension.Length == 0 ? "Explorer.File" : "Explorer.FileWithExtension",
            MediaContentIconIds.File, extension);
    }

    private static MediaContentTypeDefinition Fallback(
        MediaContentCategory category,
        string resourceKey,
        string iconId,
        string extension = "",
        MediaContentFormat contentFormat = MediaContentFormat.Unknown) =>
        new(null, MediaContentRecognitionFunctions.NormalizeExtension(extension), category, resourceKey,
            MediaExecutionKind.None, iconId, contentFormat,
            MediaTextEncoding.NotApplicable, MediaPreviewKind.None);

    private static MediaContentCategory? KnownCategory(
        string comment,
        bool? dataValid,
        MediaFileSystemFamily family)
    {
        if (dataValid == false) return MediaContentCategory.Data;
        var type = comment.Trim();
        if (family == MediaFileSystemFamily.Commodore && type.StartsWith("PRG", StringComparison.OrdinalIgnoreCase)) return MediaContentCategory.Program;
        if (family == MediaFileSystemFamily.AppleDos && type is "Text") return MediaContentCategory.Text;
        if (family == MediaFileSystemFamily.AppleDos && type is "Integer BASIC" or "Applesoft BASIC") return MediaContentCategory.BasicProgram;
        if (family == MediaFileSystemFamily.ProDos && type is "Text") return MediaContentCategory.Text;
        if (family == MediaFileSystemFamily.ProDos && type is "BASIC") return MediaContentCategory.BasicProgram;
        if (family == MediaFileSystemFamily.ProDos && type is "System") return MediaContentCategory.System;
        if (family == MediaFileSystemFamily.Macintosh)
        {
            if (type.Equals("APPL", StringComparison.OrdinalIgnoreCase)) return MediaContentCategory.Executable;
            if (type.Equals("TEXT", StringComparison.OrdinalIgnoreCase)) return MediaContentCategory.Text;
            if (type.Equals("PICT", StringComparison.OrdinalIgnoreCase)) return MediaContentCategory.Image;
            if (type.Equals("snd", StringComparison.OrdinalIgnoreCase) || type.Equals("AIFF", StringComparison.OrdinalIgnoreCase)) return MediaContentCategory.Audio;
        }
        if (family == MediaFileSystemFamily.Ucsd)
        {
            if (type.Equals("UCSD code file", StringComparison.OrdinalIgnoreCase)) return MediaContentCategory.Executable;
            if (type.Equals("UCSD text file", StringComparison.OrdinalIgnoreCase)) return MediaContentCategory.Text;
            if (type is "UCSD graphics file" or "UCSD photo file") return MediaContentCategory.Image;
        }
        return null;
    }

    private static bool LooksLikeText(IReadOnlyList<byte>? data, MediaFileSystemFamily family)
    {
        if (data is not { Count: > 0 }) return false;
        var sample = data.Take(Math.Min(data.Count, 512)).ToArray();
        var printable = sample.Count(value =>
        {
            if (value is 9 or 10 or 13) return true;
            if (family != MediaFileSystemFamily.Atari8Bit) return value is >= 32 and < 127;
            if (value == 0x9B) return true;
            var atascii = value & 0x7F;
            return atascii is >= 32 and < 127;
        });
        return printable >= sample.Length * 0.9;
    }

    private static MediaPreviewKind PreviewFor(MediaContentCategory category) => category switch
    {
        MediaContentCategory.Text => MediaPreviewKind.Text,
        MediaContentCategory.BasicProgram => MediaPreviewKind.BasicListing,
        MediaContentCategory.Executable => MediaPreviewKind.Hexadecimal,
        MediaContentCategory.System => MediaPreviewKind.Hexadecimal,
        MediaContentCategory.Image => MediaPreviewKind.Image,
        MediaContentCategory.Audio => MediaPreviewKind.Audio,
        _ => MediaPreviewKind.None
    };

}
