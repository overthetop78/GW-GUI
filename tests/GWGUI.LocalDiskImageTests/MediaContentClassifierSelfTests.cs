using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;

namespace GWGUI.MediaAudit;

internal static class MediaContentClassifierSelfTests
{
    internal static void Run()
    {
        var classifier = new MediaContentClassifier();
        var metadata = new Dictionary<string, string>();
        byte[] anticMusic = [(byte)'A', (byte)'M', (byte)'1', 0x19, 0x02, 0x4d];

        var extensionless = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            anticMusic, metadata, MediaFileSystemFamily.Atari8Bit);
        var misleadingExtension = classifier.Classify(
            ".in", MediaEntryKind.File, null, string.Empty, true,
            anticMusic, metadata, MediaFileSystemFamily.Atari8Bit);
        var otherFamily = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            anticMusic, metadata, MediaFileSystemFamily.IbmPc);

        ExpectAudio(extensionless, "An extensionless AM1 file on Atari 8-bit must be recognized as audio.");
        ExpectAudio(misleadingExtension, "An AM1 file with an unrelated extension on Atari 8-bit must be recognized as audio.");
        if (otherFamily?.Category == MediaContentCategory.Audio)
            throw new InvalidOperationException("The AM1 signature must not classify files outside the Atari 8-bit family.");

        var extensionRule = GWGUI.MediaAnalysis.Dictionaries.ContentRecognition.MediaContentRecognitionCatalog.Find(
            MediaFileSystemFamily.Atari8Bit, ".tur");
        if (extensionRule?.Category != MediaContentCategory.BasicProgram)
            throw new InvalidOperationException("Extension recognition must remain part of the common catalog.");

        var fixedEndRule = new MediaContentRecognitionRule(
            null, string.Empty, [new(2, MediaContentSearchDirection.End, "END"u8.ToArray())],
            MediaContentCategory.Archive, MediaTextEncoding.NotApplicable,
            MediaExecutionKind.None, MediaPreviewKind.None);
        if (!MediaContentSignatureMatcher.Matches(fixedEndRule, "prefix-END"u8.ToArray()))
            throw new InvalidOperationException("A fixed end position must locate the first byte of the signature from the end of the file.");

        var forwardSearchRule = new MediaContentRecognitionRule(
            null, string.Empty, [new(null, MediaContentSearchDirection.Start, "CODE"u8.ToArray())],
            MediaContentCategory.Data, MediaTextEncoding.NotApplicable,
            MediaExecutionKind.None, MediaPreviewKind.Hexadecimal);
        if (!MediaContentSignatureMatcher.Matches(forwardSearchRule, "before-CODE-after"u8.ToArray()))
            throw new InvalidOperationException("A free start search must find a signature anywhere in the file.");

        var backwardSearchRule = new MediaContentRecognitionRule(
            null, string.Empty, [new(null, MediaContentSearchDirection.End, "CODE"u8.ToArray())],
            MediaContentCategory.Data, MediaTextEncoding.NotApplicable,
            MediaExecutionKind.None, MediaPreviewKind.Hexadecimal);
        if (!MediaContentSignatureMatcher.Matches(backwardSearchRule, "before-CODE-after"u8.ToArray()))
            throw new InvalidOperationException("A free end search must find a signature anywhere in the file.");

        var multiHeaderRule = new MediaContentRecognitionRule(
            null, string.Empty,
            [
                new(0, MediaContentSearchDirection.Start, "FORM"u8.ToArray()),
                new(8, MediaContentSearchDirection.Start, "ILBM"u8.ToArray())
            ], MediaContentCategory.Image, MediaTextEncoding.NotApplicable,
            MediaExecutionKind.None, MediaPreviewKind.Image);
        if (!MediaContentSignatureMatcher.Matches(multiHeaderRule,
                [(byte)'F', (byte)'O', (byte)'R', (byte)'M', 0, 0, 0, 4, (byte)'I', (byte)'L', (byte)'B', (byte)'M']))
            throw new InvalidOperationException("Header patterns must support named byte sequences at exact offsets.");

        var abcRuntime = new byte[512];
        ApplySignatures(abcRuntime,
            MediaContentSignatures.AtariBinaryLoadMarker,
            MediaContentSignatures.AtariAbcRuntimeCode,
            MediaContentSignatures.AtariAbcRuntimeRunVector);
        var runtimeDefinition = classifier.Classify(
            ".x1f", MediaEntryKind.File, null, string.Empty, true,
            abcRuntime, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(runtimeDefinition, MediaContentCategory.Library,
            "An ABC runtime interpreter must be recognized as a library independently of its extension.");

        var abcRelocationUtility = new byte[256];
        ApplySignatures(abcRelocationUtility,
            MediaContentSignatures.AtariAbcRelocationLoader,
            MediaContentSignatures.AtariAbcRelocationInitVector);
        var relocationDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            abcRelocationUtility, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(relocationDefinition, MediaContentCategory.Executable,
            "The ABC MKRELO relocation utility must be recognized as an executable without relying on its name.");

        var tokenizedBasic = new byte[]
        {
            0x00, 0x00, 0x00, 0x01, 0x3C, 0x01, 0x3D, 0x01,
            0xF5, 0x01, 0x27, 0x11, 0x35, 0x11, 0x4D, 0x4F
        };
        var basicDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            tokenizedBasic, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(basicDefinition, MediaContentCategory.BasicProgram,
            "A saved tokenized Atari BASIC program must be recognized without relying on its filename.");

        var atasciiUser = new byte[] { 0x56, 0x49, 0x45, 0x57, 0x45, 0x44, 0x9B };
        var atasciiUserDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            atasciiUser, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectAtasciiText(atasciiUserDefinition,
            "An extensionless ATASCII text ending in 0x9B must be recognized as text.");

        var atasciiReport = new byte[]
        {
            0x31, 0x9B, 0x32, 0x38, 0x9B, 0x38, 0x30, 0x9B,
            0x2A, 0x2A, 0x2A, 0x20, 0x52, 0x45, 0x50, 0x4F, 0x52, 0x54, 0x20, 0x2A, 0x2A, 0x2A, 0x9B
        };
        var atasciiReportDefinition = classifier.Classify(
            ".f01", MediaEntryKind.File, null, string.Empty, true,
            atasciiReport, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectAtasciiText(atasciiReportDefinition,
            "ATASCII report data with an unknown extension must be recognized as text.");
    }

    private static void ExpectAudio(
        GWGUI.MediaAnalysis.Contracts.MediaContentTypeDefinition? definition,
        string message)
    {
        if (definition?.Category != MediaContentCategory.Audio
            || definition.ContentFormat != MediaContentFormat.Audio)
            throw new InvalidOperationException(message);
    }

    private static void ExpectCategory(
        MediaContentTypeDefinition? definition,
        MediaContentCategory expected,
        string message)
    {
        if (definition?.Category != expected)
            throw new InvalidOperationException(message);
    }

    private static void ExpectAtasciiText(MediaContentTypeDefinition? definition, string message)
    {
        if (definition?.Category != MediaContentCategory.Text
            || definition.TextEncoding != MediaTextEncoding.Atascii)
            throw new InvalidOperationException(message);
    }

    private static void ApplySignatures(byte[] content, params MediaContentSignature[] signatures)
    {
        foreach (var signature in signatures)
        {
            if (signature.Position is not int position)
                throw new InvalidOperationException("This self-test requires fixed signature positions.");
            var start = signature.Direction == MediaContentSearchDirection.Start
                ? position
                : content.Length - 1 - position;
            for (var index = 0; index < signature.Bytes.Count; index++)
                content[start + index] = signature.Bytes[index];
        }
    }
}
