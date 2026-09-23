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
            null, string.Empty, [new(2, MediaContentSearchDirection.End, ["END"u8.ToArray()])],
            MediaContentCategory.Archive, MediaTextEncoding.NotApplicable,
            MediaExecutionKind.None, MediaPreviewKind.None);
        if (!MediaContentSignatureMatcher.Matches(fixedEndRule, "prefix-END"u8.ToArray()))
            throw new InvalidOperationException("A fixed end position must locate the first byte of the signature from the end of the file.");

        var forwardSearchRule = new MediaContentRecognitionRule(
            null, string.Empty, [new(null, MediaContentSearchDirection.SearchStart, ["HEAD"u8.ToArray(), "CODE"u8.ToArray(), "END"u8.ToArray()])],
            MediaContentCategory.Data, MediaTextEncoding.NotApplicable,
            MediaExecutionKind.None, MediaPreviewKind.Hexadecimal);
        if (!MediaContentSignatureMatcher.Matches(forwardSearchRule, "before-HEAD-gap-CODE-gap-END-after"u8.ToArray()))
            throw new InvalidOperationException("A forward search must find every byte group successively from the start.");
        if (MediaContentSignatureMatcher.Matches(forwardSearchRule, "before-HEAD-gap-END-gap-CODE-after"u8.ToArray()))
            throw new InvalidOperationException("A forward search must reject byte groups found out of order.");
        if (MediaContentSignatureMatcher.Matches(forwardSearchRule, "before-HEAD-gap-CODE-after"u8.ToArray()))
            throw new InvalidOperationException("A forward search must stop when the current byte group cannot be found.");

        var backwardSearchRule = new MediaContentRecognitionRule(
            null, string.Empty, [new(null, MediaContentSearchDirection.SearchEnd, ["END"u8.ToArray(), "CODE"u8.ToArray(), "HEAD"u8.ToArray()])],
            MediaContentCategory.Data, MediaTextEncoding.NotApplicable,
            MediaExecutionKind.None, MediaPreviewKind.Hexadecimal);
        if (!MediaContentSignatureMatcher.Matches(backwardSearchRule, "before-HEAD-gap-CODE-gap-END-after"u8.ToArray()))
            throw new InvalidOperationException("A backward search must find every byte group successively from the end.");
        if (MediaContentSignatureMatcher.Matches(backwardSearchRule, "before-END-gap-CODE-gap-HEAD-after"u8.ToArray()))
            throw new InvalidOperationException("A backward search must reject byte groups found out of order.");
        if (MediaContentSignatureMatcher.Matches(backwardSearchRule, "before-CODE-gap-END-after"u8.ToArray()))
            throw new InvalidOperationException("A backward search must stop when the current byte group cannot be found.");

        var multiHeaderRule = new MediaContentRecognitionRule(
            null, string.Empty,
            [
                new(0, MediaContentSearchDirection.Start, ["FORM"u8.ToArray()]),
                new(8, MediaContentSearchDirection.Start, ["ILBM"u8.ToArray()])
            ], MediaContentCategory.Image, MediaTextEncoding.NotApplicable,
            MediaExecutionKind.None, MediaPreviewKind.Image);
        if (!MediaContentSignatureMatcher.Matches(multiHeaderRule,
                [(byte)'F', (byte)'O', (byte)'R', (byte)'M', 0, 0, 0, 4, (byte)'I', (byte)'L', (byte)'B', (byte)'M']))
            throw new InvalidOperationException("Header patterns must support named byte sequences at exact offsets.");

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

        byte[] artistUnleashedGraphic =
        [
            0x20, 0x20, 0x34, 0x31, 0x32, 0x39, 0x36, 0x9B,
            0x00, 0x00, 0x32, 0x0A, 0x01, 0x0E,
            0xFF, 0xFF, 0x18, 0x44, 0xE0, 0x02, 0x00, 0x9B
        ];
        var artistDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            artistUnleashedGraphic, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(artistDefinition, MediaContentCategory.Image,
            "An extensionless Artist Unleashed graphic must be recognized as an image even when its pixels contain binary-load markers.");

        byte[] atariBinaryLoad = [0xFF, 0xFF, 0x00, 0x20, 0x02, 0x20, 0xAA, 0xBB, 0xCC];
        var binaryLoadDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            atariBinaryLoad, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(binaryLoadDefinition, MediaContentCategory.Executable,
            "An Atari Binary Load file beginning with its marker must be recognized as executable.");

        byte[] floatingBinaryMarkers = [0x20, 0x10, 0xFF, 0xFF, 0x18, 0x44, 0xE0, 0x02, 0x00];
        var floatingMarkersDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            floatingBinaryMarkers, metadata, MediaFileSystemFamily.Atari8Bit);
        if (floatingMarkersDefinition?.Category == MediaContentCategory.Executable)
            throw new InvalidOperationException("Binary-load markers found inside unrelated content must not classify it as executable.");

        byte[] microsoftBasicCioUser =
        [
            0xA0, 0x01, 0xB1, 0xDD, 0x29, 0x07, 0x0A, 0x0A, 0x0A, 0x0A, 0xAA, 0xA0, 0x03, 0xB1, 0xDD, 0x9D,
            0x18, 0x44, 0x00, 0x00, 0x62, 0x10, 0xA5, 0xDD, 0x91, 0x02, 0xC8, 0xD0, 0xF8, 0xA0, 0x00, 0x60,
            0xDD, 0xA0, 0x0B, 0xBD, 0x4A, 0x03, 0x91, 0xDD, 0xE8, 0xC8, 0xC8, 0xC0, 0x16, 0x90, 0xF4, 0x60
        ];
        var cioUserDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            microsoftBasicCioUser, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(cioUserDefinition, MediaContentCategory.Library,
            "The reusable Atari Microsoft BASIC CIO USR routines must be recognized as a library.");

        byte[] microIllustratorGraphic =
        [
            0xFF, 0x80, 0xC9, 0xC7, 0x1A, 0x00, 0x01, 0x02,
            0x0E, 0x00, 0x28, 0x00, 0xC0, 0x26, 0xCA, 0x92
        ];
        var microIllustratorDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            microIllustratorGraphic, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(microIllustratorDefinition, MediaContentCategory.Image,
            "An extensionless Micro Illustrator picture must be recognized from its fixed format fields.");

        byte[] tokenizedBasicUtility =
        [
            0x00, 0x00, 0x20, 0x01, 0x63, 0x01, 0x64, 0x01,
            0x14, 0x02, 0x1E, 0x18, 0x34, 0x18, 0xCE, 0xD9
        ];
        var tokenizedBasicUtilityDefinition = classifier.Classify(
            ".utl", MediaEntryKind.File, null, string.Empty, true,
            tokenizedBasicUtility, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(tokenizedBasicUtilityDefinition, MediaContentCategory.BasicProgram,
            "A saved Atari BASIC utility must be recognized from its extension and saved-program prefix.");

        var binaryUtilityDefinition = classifier.Classify(
            ".utl", MediaEntryKind.File, null, string.Empty, true,
            atariBinaryLoad, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(binaryUtilityDefinition, MediaContentCategory.Executable,
            "A binary-load Atari utility must remain executable instead of being classified only from its UTL extension.");

        byte[] atariWriterInternalData =
        [
            0x17, 0x00, 0x0C, 0x00, 0x02, 0x00, 0x00, 0x01, 0x00, 0x05, 0x00, 0x00, 0x0A, 0x00, 0x46, 0x00,
            0x00, 0x01, 0x46, 0x02, 0x0C, 0x00, 0x00, 0x00,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
        ];
        var atariWriterDataDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            atariWriterInternalData, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(atariWriterDataDefinition, MediaContentCategory.Data,
            "AtariWriter Plus internal structured content must be recognized as data.");

        var zeroFilledDataDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            new byte[27], metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(zeroFilledDataDefinition, MediaContentCategory.Data,
            "The AtariWriter Plus zero-filled state block must be recognized as data.");

        byte[] tokenizedAssemblerSource =
        [
            0x53, 0x43, 0xB5, 0x9A, 0x1F, 0x9C, 0x0E, 0x0A,
            0x00, 0x81, 0x2E, 0x4F, 0x52, 0x81, 0x24, 0x32,
            0x30, 0x30, 0x30, 0x00
        ];
        var assemblerSourceDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            tokenizedAssemblerSource, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(assemblerSourceDefinition, MediaContentCategory.SourceCode,
            "Tokenized Atari assembler source must be recognized as source code.");

        byte[] awardWareCharacters =
        [
            0x40, 0x01, 0xC0, 0x00, 0x01, 0x01, 0x50, 0xFF,
            0xF0, 0x01, 0x26, 0x00, 0x0F, 0xF0, 0x01, 0x26,
            0x00, 0xCF, 0xF3, 0x01
        ];
        var awardWareCharactersDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            awardWareCharacters, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(awardWareCharactersDefinition, MediaContentCategory.Font,
            "An AwardWare character table must be recognized as a font without relying on its filename.");

        byte[] awardWareShortTemplate =
        [
            0xC1, 0x88, 0x54, 0x00, 0x40, 0x02, 0xE0, 0x02,
            0x42, 0xFF, 0x01, 0x00, 0x40, 0x02, 0xE0, 0x02
        ];
        var awardWareShortTemplateDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            awardWareShortTemplate, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(awardWareShortTemplateDefinition, MediaContentCategory.Data,
            "An AwardWare short template must be recognized as structured data without relying on its filename.");

        byte[] awardWareLayout =
        [
            0x07, 0x00, 0x10, 0x00, 0x23, 0x01,
            0x45, 0x40, 0x02, 0xE0, 0x02, 0x00,
            0x41, 0x6D, 0x6F, 0x75, 0x6E, 0x74,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        ];
        var awardWareLayoutDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            awardWareLayout, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(awardWareLayoutDefinition, MediaContentCategory.Data,
            "An AwardWare page layout must be recognized as structured data without relying on its filename.");

        byte[] awardWareWidths =
        [
            0x10, 0x0B, 0x12, 0x00, 0x15, 0x00, 0x1A, 0x09,
            0x0F, 0x0F, 0x11, 0x1A, 0x0B, 0x1A, 0x0B, 0x16,
            0x15, 0x15, 0x15, 0x15
        ];
        var awardWareWidthsDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            awardWareWidths, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(awardWareWidthsDefinition, MediaContentCategory.Data,
            "An AwardWare character-width table must be recognized as structured data without relying on its filename.");

        byte[][] bGraphMachineRoutines =
        [
            [0x68, 0x68, 0x68, 0x85, 0xE5, 0x29, 0x7F, 0x85, 0xE4, 0x68, 0x68, 0xC9, 0x01, 0xF0, 0x08, 0xA9, 0x00, 0x60, 0x9B],
            [0xD8, 0x68, 0x68, 0x85, 0xCC, 0x68, 0x85, 0xCB, 0x68, 0x85, 0xCE, 0x68, 0x85, 0xCD, 0x68, 0x68, 0x60, 0x9B],
            [0xA4, 0x57, 0xA9, 0x28, 0xC0, 0x07, 0xB0, 0x06, 0x4A, 0xC0, 0x05, 0xB0, 0x01, 0x4A, 0x85, 0xE5, 0x60, 0x9B]
        ];
        foreach (var bGraphMachineRoutine in bGraphMachineRoutines)
        {
            var bGraphMachineRoutineDefinition = classifier.Classify(
                string.Empty, MediaEntryKind.File, null, string.Empty, true,
                bGraphMachineRoutine, metadata, MediaFileSystemFamily.Atari8Bit);
            ExpectCategory(bGraphMachineRoutineDefinition, MediaContentCategory.Library,
                "A B-Graph machine-language support routine must be recognized as a library without relying on its filename.");
        }

        byte[] bGraphImage =
        [
            0x30, 0x9B, 0x30, 0x9B, 0x31, 0x34, 0x9B,
            0x70, 0x70, 0x70, 0x4F, 0x50, 0x81, 0x0F, 0x0F,
            0x30, 0x9B, 0x30, 0x9B, 0x31, 0x34, 0x9B
        ];
        var bGraphImageDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            bGraphImage, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(bGraphImageDefinition, MediaContentCategory.Image,
            "A B-Graph binary picture containing successive image blocks must be recognized as an image without relying on its filename.");

        var basicXeExtensionDefinition = classifier.Classify(
            ".oss", MediaEntryKind.File, null, string.Empty, true,
            [0xFF, 0xDD, 0x07, 0x3F, 0x13, 0x6B], metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(basicXeExtensionDefinition, MediaContentCategory.Library,
            "An Atari 8-bit OSS language-extension file must be recognized as a library from its family-specific extension.");

        var blackMagicMusicDefinition = classifier.Classify(
            FileTypeExtensions.Msc, MediaEntryKind.File, null, string.Empty, true,
            [0x01, 0x02, 0x03], metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectAudio(blackMagicMusicDefinition,
            "An Atari 8-bit Black Magic Composer music file must be recognized as audio from its family-specific extension.");

        var blackMagicDrumPatternDefinition = classifier.Classify(
            FileTypeExtensions.Drp, MediaEntryKind.File, null, string.Empty, true,
            [0x04, 0x05, 0x06], metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectAudio(blackMagicDrumPatternDefinition,
            "An Atari 8-bit Black Magic Composer drum-pattern file must be recognized as audio from its family-specific extension.");

        var messageIndexDefinition = classifier.Classify(
            FileTypeExtensions.Ism, MediaEntryKind.File, null, string.Empty, true,
            [0x01, 0x00, 0x04, 0x0D], metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(messageIndexDefinition, MediaContentCategory.Data,
            "An Atari 8-bit BBCS message-index file must be recognized as data from its family-specific extension.");

        var surveyDefinition = classifier.Classify(
            FileTypeExtensions.Srv, MediaEntryKind.File, null, string.Empty, true,
            [0xFE, 0xFE, 0x01, 0x04], metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(surveyDefinition, MediaContentCategory.Data,
            "An Atari 8-bit BBCS survey file must be recognized as data from its family-specific extension.");

        byte[] bearEssentialsPicture =
        [
            0x48, 0xAD, 0x6E, 0x06, 0x8D, 0x0A, 0xD4, 0x8D,
            0x18, 0xD0, 0xEE, 0x6E, 0x06, 0x68, 0x40, 0xAD,
            0x6F, 0x06, 0x8D, 0x6E, 0x06, 0xCE, 0x6F, 0x06,
            0x8E, 0x8E, 0x8E, 0x8E
        ];
        var bearEssentialsPictureDefinition = classifier.Classify(
            string.Empty, MediaEntryKind.File, null, string.Empty, true,
            bearEssentialsPicture, metadata, MediaFileSystemFamily.Atari8Bit);
        ExpectCategory(bearEssentialsPictureDefinition, MediaContentCategory.Image,
            "The Bear Essentials binary picture must be recognized as an image without relying on its filename.");
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

}
