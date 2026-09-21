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
    }

    private static void ExpectAudio(
        GWGUI.MediaAnalysis.Contracts.MediaContentTypeDefinition? definition,
        string message)
    {
        if (definition?.Category != MediaContentCategory.Audio
            || definition.ContentFormat != MediaContentFormat.Audio)
            throw new InvalidOperationException(message);
    }
}
