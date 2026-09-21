using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Dictionaries.FileTypes;
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

        var known = MediaContentTypeCatalog.Find(family, extension);
        var recognized = KnownCategory(comment, dataValid, content, family);
        if (recognized is not null && (known is null || known.Category is MediaContentCategory.File or MediaContentCategory.Data))
        {
            var encoding = recognized == MediaContentCategory.Text && family == MediaFileSystemFamily.Atari8Bit
                ? MediaTextEncoding.Atascii
                : MediaTextEncoding.NotApplicable;
            return MediaContentTypeRuleFactory.Rule(family, extension, recognized.Value, encoding,
                recognized == MediaContentCategory.Executable ? MediaExecutionKind.NativeExecutable : MediaExecutionKind.None);
        }
        if (known is not null) return known;
        if (LooksLikeText(content))
            return MediaContentTypeRuleFactory.Rule(family, extension, MediaContentCategory.Text, MediaTextEncoding.Unknown);
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
        new(null, MediaContentTypeRuleFactory.Normalize(extension), category, resourceKey,
            MediaExecutionKind.None, iconId, contentFormat,
            MediaTextEncoding.NotApplicable, MediaPreviewKind.None);

    private static MediaContentCategory? KnownCategory(
        string comment,
        bool? dataValid,
        IReadOnlyList<byte>? content,
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
        if (IsAmigaExecutable(content) && family == MediaFileSystemFamily.Amiga) return MediaContentCategory.Executable;
        if (IsDosExecutable(content) && family == MediaFileSystemFamily.IbmPc) return MediaContentCategory.Executable;
        if (IsAtariExecutable(content) && family == MediaFileSystemFamily.AtariTos) return MediaContentCategory.Executable;
        if (HasFormType(content, "ILBM")) return MediaContentCategory.Image;
        if (HasFormType(content, "8SVX")) return MediaContentCategory.Audio;
        return null;
    }

    private static bool LooksLikeText(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: > 0 }) return false;
        var sample = data.Take(Math.Min(data.Count, 512)).ToArray();
        var printable = sample.Count(value => value is 9 or 10 or 13 || value >= 32 && value < 127);
        return printable >= sample.Length * 0.9;
    }

    private static bool IsAmigaExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 4 } && data[0] == 0 && data[1] == 0 && data[2] == 3 && data[3] == 0xF3;

    private static bool IsDosExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 2 } && data[0] == (byte)'M' && data[1] == (byte)'Z';

    private static bool IsAtariExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 2 } && data[0] == 0x60 && data[1] == 0x1A;

    private static bool HasFormType(IReadOnlyList<byte>? data, string type) =>
        data is { Count: >= 12 } &&
        data[0] == (byte)'F' && data[1] == (byte)'O' && data[2] == (byte)'R' && data[3] == (byte)'M' &&
        data.Skip(8).Take(4).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(type));
}
