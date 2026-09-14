using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static class ExplorerFileContentClassifier
{
    public static ExplorerFileCategory? KnownCategory(FileSystemEntry entry, ExplorerFileSystemFamily family)
    {
        var metadata = MetadataCategory(entry, family);
        if (metadata is not null) return metadata;
        if (IsAmigaExecutable(entry.Content) && family == ExplorerFileSystemFamily.Amiga) return ExplorerFileCategory.Executable;
        if (IsDosExecutable(entry.Content) && family == ExplorerFileSystemFamily.IbmPc) return ExplorerFileCategory.Executable;
        if (IsAtariExecutable(entry.Content) && family == ExplorerFileSystemFamily.AtariSt) return ExplorerFileCategory.Executable;
        if (family == ExplorerFileSystemFamily.Atari8Bit)
        {
            if (string.Equals(entry.NativeTypeId, "atari-print-shop-icon", StringComparison.Ordinal)) return ExplorerFileCategory.Image;
            if (string.Equals(entry.NativeTypeId, "word-magic-main-dictionary", StringComparison.Ordinal)) return ExplorerFileCategory.Data;
            if (IsSuper3DPlotterPoints(entry) || IsSuper3DPlotterLines(entry)) return ExplorerFileCategory.Image;
            if (IsSuper3DPlotterPrinterProfile(entry.Content)) return ExplorerFileCategory.Configuration;
            if (IsSuperMailerPrinterType(entry)) return ExplorerFileCategory.Configuration;
            if (IsTechnicolorDreamImageComponent(entry)) return ExplorerFileCategory.Image;
            if (IsTipImage(entry)) return ExplorerFileCategory.Image;
            if (IsTrzmielCompressedImage(entry)) return ExplorerFileCategory.Image;
            if (IsTrickMachineRoutine(entry)) return ExplorerFileCategory.Library;
            if (IsTrickMusicData(entry)) return ExplorerFileCategory.Audio;
            if (IsTrickDataTable(entry)) return ExplorerFileCategory.Data;
            if (IsTrickFont(entry)) return ExplorerFileCategory.Font;
            if (IsTrickBinaryContainer(entry)) return ExplorerFileCategory.Executable;
            if (IsUueDecoderBinaryCommand(entry)) return ExplorerFileCategory.Executable;
            if (IsVideo130XeHelpDocument(entry)) return ExplorerFileCategory.Document;
            if (IsAaEditorDocument(entry.Content)) return ExplorerFileCategory.Document;
            if (IsVideoScannerImage(entry)) return ExplorerFileCategory.Image;
            if (IsVidigPaintImage(entry)) return ExplorerFileCategory.Image;
            if (IsXlPaintRipImage(entry)) return ExplorerFileCategory.Image;
            if (IsXlPaintImage(entry)) return ExplorerFileCategory.Image;
            if (IsVirtuosoComposition(entry)) return ExplorerFileCategory.Audio;
            if (IsVoicemasterSpeechSample(entry)) return ExplorerFileCategory.Audio;
            if (IsAngRawDigitizedSample(entry)) return ExplorerFileCategory.Audio;
            if (IsVisiCalcCatalogModule(entry)) return ExplorerFileCategory.Library;
            if (IsVisualiserHelpDocument(entry)) return ExplorerFileCategory.Document;
            if (IsVisualiserGraphicResource(entry.Content)) return ExplorerFileCategory.Image;
            if (IsWritersToolPrinterProfile(entry)) return ExplorerFileCategory.Configuration;
            if (IsWritersToolExtensionModule(entry)) return ExplorerFileCategory.Library;
            if (entry.Content is { Count: 1 }) return ExplorerFileCategory.Data;
            if (IsFilledWithZero(entry.Content)) return ExplorerFileCategory.Data;
            if (entry.Content is { Count: >= 4 } && entry.Content[0] == 0xff && entry.Content[1] == 0x80 && entry.Content[2] == 0xc9 && entry.Content[3] == 0xc7)
                return ExplorerFileCategory.Image;
            if (IsAtari8BitAdvancedMusicSystem(entry.Content)) return ExplorerFileCategory.Audio;
            if (IsAtari8BitDrumPattern(entry.Content)) return ExplorerFileCategory.Audio;
            if (IsAtari8BitDrumSampleBank(entry.Content)) return ExplorerFileCategory.Audio;
            if (IsMidiPatternEditorComposition(entry)) return ExplorerFileCategory.Audio;
            if (IsAtariMidiSequencerFile(entry.Content)) return ExplorerFileCategory.Audio;
            if (IsMusicConstructionSetFile(entry.Content)) return ExplorerFileCategory.Audio;
            if (IsMusicStudioFile(entry.Content)) return ExplorerFileCategory.Audio;
            if (IsAtari8BitRaytracerBackground(entry.Content)) return ExplorerFileCategory.Image;
            if (IsAwardWareCatalog(entry.Content)) return ExplorerFileCategory.Data;
            if (IsAwardWarePrinterTemplate(entry.Content)) return ExplorerFileCategory.Configuration;
            if (IsAwardWareDocumentTemplate(entry.Content)) return ExplorerFileCategory.Document;
            if (IsAwardWareFontWidths(entry.Content)) return ExplorerFileCategory.Font;
            if (IsAwardWareGraphic(entry.Content)) return ExplorerFileCategory.Image;
            if (IsAwardWarePrinterProfile(entry.Content)) return ExplorerFileCategory.Configuration;
            if (IsAwardWarePrinterSelection(entry.Content)) return ExplorerFileCategory.Configuration;
            if (IsBGraphMachineRoutine(entry.Content)) return ExplorerFileCategory.Library;
            if (IsBGraphData(entry.Content)) return ExplorerFileCategory.Data;
            if (IsBashADrumPattern(entry.Content) || IsBashADrumSong(entry.Content)) return ExplorerFileCategory.Audio;
            if (IsBearEssentialsPicture(entry.Content)) return ExplorerFileCategory.Image;
            if (IsAtariRawFontBank(entry.Content)) return ExplorerFileCategory.Font;
            if (IsAtariRawCharacterSet(entry.Content)) return ExplorerFileCategory.Font;
            if (IsEpsonFxDownloadableFont(entry.Content)) return ExplorerFileCategory.Font;
            if (IsAtariRawScreenImage(entry.Content)) return ExplorerFileCategory.Image;
            if (IsAtariRawScreenWithPalette(entry.Content)) return ExplorerFileCategory.Image;
            if (IsPrintShopConverterScreen(entry.Content)) return ExplorerFileCategory.Image;
            if (IsPlayerMissileGraphicsTabletImage(entry.Content)) return ExplorerFileCategory.Image;
            if (IsTypesetterIcon(entry)) return ExplorerFileCategory.Image;
            if (IsAtariTwoColorPackedBitmap(entry.Content)) return ExplorerFileCategory.Image;
            if (IsPicilityImage(entry.Content)) return ExplorerFileCategory.Image;
            if (IsPrintPowerCatalog(entry)) return ExplorerFileCategory.Data;
            if (IsPrintPowerPrinterSelection(entry.Content)) return ExplorerFileCategory.Configuration;
            if (IsPrintPowerGraphicLibrary(entry)) return ExplorerFileCategory.Image;
            if (IsRambrandtUserPattern(entry)) return ExplorerFileCategory.Image;
            if (IsRubberStampPad(entry)) return ExplorerFileCategory.Image;
            if (IsSchemaDesignDocument(entry)) return ExplorerFileCategory.Document;
            if (IsSchematicDesignerDocument(entry)) return ExplorerFileCategory.Document;
            if (IsScreenAidedManagementCatalog(entry)) return ExplorerFileCategory.Data;
            if (IsScreenAidedManagementPrinterProfile(entry.Content)) return ExplorerFileCategory.Configuration;
            if (IsScreenDumpPrinterProfile(entry)) return ExplorerFileCategory.Configuration;
            if (IsDosAsciiDocument(entry)) return ExplorerFileCategory.Text;
            if (IsSoftsynthFont(entry)) return ExplorerFileCategory.Font;
            if (IsSoftsynthComposition(entry.Content)) return ExplorerFileCategory.Audio;
            if (IsSongwriterLesson(entry)) return ExplorerFileCategory.Data;
            if (IsSoundTrackerMusic(entry)) return ExplorerFileCategory.Audio;
            if (IsSoundTrackerInstrumentData(entry)) return ExplorerFileCategory.Data;
            if (IsAtariMadDesignerImage(entry)) return ExplorerFileCategory.Image;
            if (IsAtariMarcoPixelEditorImage(entry)) return ExplorerFileCategory.Image;
            if (IsAtariMegacolorEditorImage(entry)) return ExplorerFileCategory.Image;
            if (IsMultiGraphViewImage(entry)) return ExplorerFileCategory.Image;
            if (IsNewsroomPhoto(entry.Content)) return ExplorerFileCategory.Image;
            if (HasAsciiPrefix(entry.Content, "CIN 1.2")) return ExplorerFileCategory.Image;
            if (IsCentroDeCostosControl(entry.Content)) return ExplorerFileCategory.Configuration;
            if (IsMantisEditorConfiguration(entry)) return ExplorerFileCategory.Configuration;
            if (IsPantherConversionTable(entry)) return ExplorerFileCategory.Configuration;
            if (IsPaperclipPrinterConfiguration(entry)) return ExplorerFileCategory.Configuration;
            if (IsDigiVoiceSample(entry.Content)) return ExplorerFileCategory.Audio;
            if (IsEasyScanConfiguration(entry.Content)) return ExplorerFileCategory.Configuration;
            if (IsAtariPrinterControlProfile(entry.Content)) return ExplorerFileCategory.Configuration;
            if (IsAtariBasicBlockLoadedLogo(entry)) return ExplorerFileCategory.Image;
            if (IsAtariGraphicsDefinition(entry) || IsAtariGraphicsMemoryPlane(entry)) return ExplorerFileCategory.Image;
            if (IsMicroProseAtariTileMap(entry)) return ExplorerFileCategory.Image;
            if (IsMicroProseAtariEffectSequence(entry)) return ExplorerFileCategory.Audio;
            if (IsAtariCadVectorDrawing(entry)) return ExplorerFileCategory.Image;
            if (IsAtariSparseTileMap(entry.Content) || IsSupertronsCharacterSet(entry)) return ExplorerFileCategory.Image;
            if (IsAtariPrexorPackedExecutable(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsAtariA000SelfExtractingExecutable(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsAtariBurgersPackedExecutable(entry.Content) || IsAtariMiner2049PackedExecutable(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsAtariPrexorRoutine(entry.Content)) return ExplorerFileCategory.Library;
            if (IsAtariUnicumLevel(entry)) return ExplorerFileCategory.Data;
            if (IsAtariCassetteRecordProgram(entry)) return ExplorerFileCategory.BootProgram;
            if (IsAtariCharacterGeneratorCassetteProgram(entry)) return ExplorerFileCategory.BootProgram;
            if (IsAtariAstroChaseCassetteProgram(entry)) return ExplorerFileCategory.BootProgram;
            if (IsAtariCassetteGraphicRecordStream(entry)) return ExplorerFileCategory.Image;
            if (IsAtariCassetteDataRecordStream(entry)) return ExplorerFileCategory.Data;
            if (IsAtariCassettePartialDataRecordStream(entry)) return ExplorerFileCategory.Data;
            if (IsAtariCassettePartialProgramStream(entry)) return ExplorerFileCategory.BootProgram;
            if (IsAtariCassetteAdventureDatabase(entry)) return ExplorerFileCategory.Data;
            if (IsAtariCassetteCompilationDataStream(entry)) return ExplorerFileCategory.Data;
            if (IsAtariCassetteAssemblerSourceStream(entry)) return ExplorerFileCategory.SourceCode;
            if (IsAtariWhoDaresWinsGameData(entry)) return ExplorerFileCategory.Data;
            if (IsAtariWhoDaresWinsGraphics(entry)) return ExplorerFileCategory.Image;
            if (IsAtariPhantomGameData(entry)) return ExplorerFileCategory.Data;
            if (IsAtariPhantomGraphics(entry)) return ExplorerFileCategory.Image;
            if (IsAnime4EverImage(entry)) return ExplorerFileCategory.Image;
            if (IsAtariLcRasterImage(entry)) return ExplorerFileCategory.Image;
            if (IsAtariDgtRawSample(entry)) return ExplorerFileCategory.Audio;
            if (IsAtariGtfImage(entry)) return ExplorerFileCategory.Image;
            if (IsAtariTrackerAudio(entry)) return ExplorerFileCategory.Audio;
            if (IsAtariPlayerMissileBlankingRoutine(entry)) return ExplorerFileCategory.Library;
            if (IsK3WaveTableUtility(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsKyanPascalEditor(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsFontMakerEditor(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsLasertellerCore(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsNewsroomRawModule(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsPaperclipHiddenModule(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsDiskWizardSpeechBasic(entry.Content)) return ExplorerFileCategory.BasicProgram;
            if (IsDiskWizardSpeechLibrary(entry.Content)) return ExplorerFileCategory.Library;
            if (HasAsciiPrefix(entry.Content, "DC-MOD13\0") || HasAsciiPrefix(entry.Content, "DC-MOD20\0"))
                return ExplorerFileCategory.Library;
            if (HasAsciiPrefix(entry.Content, "DAISY-DOT NLQ FONT")) return ExplorerFileCategory.Font;
            if (IsAtari8BitArtistUnleashedImage(entry.Content)) return ExplorerFileCategory.Image;
            if (HasAsciiPrefix(entry.Content, "CREATION") && entry.Content is { Count: > 8 } && entry.Content[8] == 0x9b)
                return ExplorerFileCategory.Data;
            if (HasAsciiPrefix(entry.Content, ";------Operating System Equates")) return ExplorerFileCategory.SourceCode;
            if (IsAtariMac65Source(entry.Content)) return ExplorerFileCategory.SourceCode;
            if (IsAtariWriterDocument(entry.Content)) return ExplorerFileCategory.Document;
            if (IsMiniOfficeDocument(entry.Content)) return ExplorerFileCategory.Document;
            if (IsAtasciiTerminalScreen(entry)) return ExplorerFileCategory.Text;
            if (IsAtariAssemblerSource(entry.Content)) return ExplorerFileCategory.SourceCode;
            if (IsAtari8BitAtasciiBasicListing(entry.Content)) return ExplorerFileCategory.BasicProgram;
            if (LooksLikeAtasciiText(entry.Content)) return ExplorerFileCategory.Text;
            if (IsAtari8BitBasicProgram(entry.Content) || IsAtari8BitPackedCassetteBasicProgram(entry))
                return ExplorerFileCategory.BasicProgram;
            if (IsAtariPascalExecutable(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsAtari8BitXex(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsAtari8BitExecutableWithEmbeddedPayload(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsAtari8BitAbcRuntimeInterpreter(entry.Content)) return ExplorerFileCategory.Library;
            if (IsAtari8BitAbcCompiledProgram(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsAtariLogoInterpreterMemoryImage(entry)) return ExplorerFileCategory.BootProgram;
            if (IsAtari8BitDosBootImageFile(entry.Content)) return ExplorerFileCategory.BootProgram;
            if (IsAtari8BitDiskBootProgram(entry.Content)) return ExplorerFileCategory.BootProgram;
            if (IsAtari8BitCassetteBootProgram(entry.Content)) return ExplorerFileCategory.BootProgram;
            if (HasAsciiPrefix(entry.Content, "AM1")) return ExplorerFileCategory.Audio;
            if (entry.Content is { Count: 125 }) return ExplorerFileCategory.Data;
        }
        if (HasFormType(entry.Content, "ILBM")) return ExplorerFileCategory.Image;
        if (HasFormType(entry.Content, "8SVX")) return ExplorerFileCategory.Audio;
        return null;
    }

    public static bool LooksLikeText(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: > 0 }) return false;
        var sample = data.Take(Math.Min(data.Count, 512)).ToArray();
        var printable = sample.Count(value => value is 9 or 10 or 13 || value >= 32 && value < 127);
        return printable >= sample.Length * 0.9;
    }

    private static bool LooksLikeAtasciiText(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: > 0 }) return false;
        var sampleLength = Math.Min(data.Count, 512);
        var printable = 0;
        var hasAtasciiEndOfLine = false;
        for (var index = 0; index < sampleLength; index++)
        {
            var value = data[index];
            if (value == 0x9b)
            {
                printable++;
                hasAtasciiEndOfLine = true;
            }
            else if ((value & 0x7f) is >= 32 and < 127 || value is 9 or 10 or 13)
            {
                printable++;
            }
        }
        return hasAtasciiEndOfLine && printable >= sampleLength * 0.9;
    }

    private static bool IsAaEditorDocument(IReadOnlyList<byte>? data) =>
        data is { Count: 1012 }
        && data.Take(11).SequenceEqual(new byte[]
        {
            0xa0, 0xc1, 0xc1, 0xc5, 0xe4, 0xe9, 0xf4, 0xef, 0xf2, 0xa0, 0x9b
        });

    private static bool IsAtari8BitAdvancedMusicSystem(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 64 }) return false;
        var values = new int[10];
        var offset = 0;
        for (var field = 0; field < values.Length; field++)
        {
            var digits = 0;
            var value = 0;
            while (offset < data.Count && data[offset] is >= (byte)'0' and <= (byte)'9' && digits < 6)
            {
                value = checked(value * 10 + data[offset] - (byte)'0');
                offset++;
                digits++;
            }
            if (digits == 0 || offset >= data.Count || data[offset++] != 0x9b) return false;
            values[field] = value;
        }
        return values[0] > 0
            && values[0] == values[2] && values[2] == values[4] && values[4] == values[6]
            && values[1] < values[3] && values[3] < values[5] && values[5] < values[7]
            && values[7] < data.Count
            && values[8] == values[9] && values[8] is > 0 and <= 255
            && offset < data.Count;
    }

    private static bool IsFilledWithZero(IReadOnlyList<byte>? data) =>
        data is { Count: > 0 } && data.All(value => value == 0);

    private static bool IsAtariWriterDocument(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 64 }
            || ReadUInt16(data, 0) != 0x0017
            || data[25] != 0x84)
            return false;
        var body = data.Skip(32).ToArray();
        return body.Contains((byte)0x9b)
            && body.Count(value => value == 0x9b || (value & 0x7f) is >= 32 and < 127) >= body.Length * 0.9;
    }

    private static bool IsMiniOfficeDocument(IReadOnlyList<byte>? data) =>
        data is { Count: >= 64 }
        && data[0] == 0xcd && data[1] == 0xc4 && data[2] == 0x01 && data[3] == 0x0d
        && data.Contains((byte)0x9b);

    private static bool IsAtasciiTerminalScreen(FileSystemEntry entry) =>
        entry.Content is { Count: > 0 } data
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".ata", StringComparison.OrdinalIgnoreCase)
        && data.Contains((byte)0x9b);

    private static bool IsAtariAssemblerSource(IReadOnlyList<byte>? data) =>
        data is { Count: >= 128 }
        && ContainsAscii(data, "LDA", 2048)
        && ContainsAscii(data, "STA", 2048)
        && ContainsAscii(data, "JSR", 2048)
        && ContainsAscii(data, "RTS", 2048);

    private static bool IsAtari8BitArtistUnleashedImage(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 32 }) return false;
        var offset = 0;
        while (offset < 16 && data[offset] == (byte)' ') offset++;
        var address = System.Text.Encoding.ASCII.GetBytes("41296");
        if (offset == 0 || offset + address.Length + 3 >= data.Count) return false;
        for (var index = 0; index < address.Length; index++)
            if (data[offset + index] != address[index]) return false;
        offset += address.Length;
        return data[offset] == 0x9b && data[offset + 1] == 0 && data[offset + 2] == 0;
    }

    private static bool IsAtari8BitDrumPattern(IReadOnlyList<byte>? data) =>
        data is { Count: 282 }
        && data[0] == 0x42 && data[1] == 0x01
        && data[2] == 0x23 && data[3] == 0x45;

    private static bool IsAtari8BitDrumSampleBank(IReadOnlyList<byte>? data) =>
        data is { Count: > 1024 }
        && ContainsAscii(data, "BASS1", 256)
        && ContainsAscii(data, "SNARE1", 256)
        && ContainsAscii(data, "HANDCLAP", 256)
        && ContainsAscii(data, "COWBELL", 256);

    private static bool IsMidiPatternEditorComposition(FileSystemEntry entry) =>
        entry.Content is { Count: >= 1024 } data
        && data.Count % 256 == 0
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".mpe", StringComparison.OrdinalIgnoreCase)
        && data.Take(256).Any(value => value is 0xfe or 0xff);

    private static bool IsAtariMidiSequencerFile(IReadOnlyList<byte>? data) =>
        data is { Count: >= 4 }
        && data[0] == 0xb3 && data[1] == 0xa5 && data[2] == 0xb1;

    private static bool IsMusicConstructionSetFile(IReadOnlyList<byte>? data) =>
        data is { Count: >= 17 }
        && (data[0] == 0x1f && data[1] == 0x0a && data[2] == 0x10 && data[3] == 0x00
            || data.Skip(4).Take(13).SequenceEqual(new byte[]
            {
                0x01, 0x09, 0x01, 0x0d, 0x0d, 0x00, 0x00, 0xf1, 0xea, 0xdc, 0xff, 0xf1, 0xea
            }));

    private static bool IsMusicStudioFile(IReadOnlyList<byte>? data) =>
        data is { Count: >= 256 }
        && ContainsAscii(data, "larinet", 256)
        && ContainsAscii(data, "ass", 256);

    private static bool IsNewsroomRawModule(IReadOnlyList<byte>? data) =>
        data is { Count: >= 5 }
        && data[0] == 0x20 && data[1] == 0xe0 && data[2] == 0x07
        && data[3] == 0xd8 && data[4] == 0x4c;

    private static bool IsPaperclipHiddenModule(IReadOnlyList<byte>? data) =>
        data is { Count: 241 }
        && data[0] == 0x29 && data[1] == 0x13 && data[2] == 0x9d
        && data[3] == 0x31 && data[4] == 0x13 && data[5] == 0xf0;

    private static bool IsAtari8BitRaytracerBackground(IReadOnlyList<byte>? data) =>
        data is { Count: 965 }
        && data[0] == 0x0a && data[1] == 0x06
        && data[2] == 0x0f && data[3] == 0x46;

    private static bool IsAwardWareCatalog(IReadOnlyList<byte>? data) =>
        data is { Count: 1605 }
        && data[0] == 0x40 && data[1] == 0x01
        && data[2] == 0xc0 && data[3] == 0x00;

    private static bool IsAwardWarePrinterTemplate(IReadOnlyList<byte>? data) =>
        data is { Count: 323 }
        && data[0] == 0x02 && data[1] == 0x89
        && data[2] == 0x54 && data[3] == 0x00;

    private static bool IsAwardWareDocumentTemplate(IReadOnlyList<byte>? data)
    {
        if (data is null) return false;
        if (data.Count == 103)
            return data[0] == 0xc1 && data[1] == 0x88
                && data[2] == 0x54 && data[3] == 0x00
                && ContainsAscii(data, "License", data.Count);
        return data.Count is 2432 or 2944 or 3200 or 4736 or 7168
            && ReadUInt16(data, 0) > 0
            && ReadUInt16(data, 0) < data.Count;
    }

    private static bool IsAwardWareFontWidths(IReadOnlyList<byte>? data) =>
        data is { Count: 455 }
        && data.Take(96).All(value => value <= 0x20);

    private static bool IsAwardWareGraphic(IReadOnlyList<byte>? data) =>
        data is { Count: > 900 }
        && ((data[0] == 0x90 && data[1] == 0x00 && data[2] == 0x70 && data[3] == 0x00)
            || (data.Count == 7722 && data[0] == 0x14 && data[1] == 0x00
                && data[4] == 0x80 && data[5] == 0x01));

    private static bool IsAwardWarePrinterProfile(IReadOnlyList<byte>? data) =>
        data is { Count: 291 or 292 }
        && data.Take(32).Contains((byte)0x0d)
        && data.Take(32).Contains((byte)0x0a);

    private static bool IsAwardWarePrinterSelection(IReadOnlyList<byte>? data) =>
        data is { Count: 16 }
        && data[0] is >= (byte)'0' and <= (byte)'9'
        && data[1] == (byte)':'
        && data.Contains((byte)0x9b);

    private static bool IsBGraphMachineRoutine(IReadOnlyList<byte>? data) =>
        data is { Count: >= 100 }
        && ((data[0] == 0x68 && data[1] == 0x68 && data[2] == 0x68 && data[3] == 0x85 && data[4] == 0xe5)
            || (data[0] == 0xd8 && data[1] == 0x68 && data[2] == 0x68 && data[3] == 0x85 && data[4] == 0xcc)
            || (data[0] == 0xa4 && data[1] == 0x57 && data[2] == 0xa9 && data[3] == 0x28 && data[4] == 0xc0));

    private static bool IsBGraphData(IReadOnlyList<byte>? data) =>
        data is { Count: > 1024 }
        && data[0] == (byte)'0' && data[1] == 0x9b
        && data[2] == (byte)'0' && data[3] == 0x9b
        && data[4] == (byte)'1' && data[5] == (byte)'4' && data[6] == 0x9b;

    private static bool IsBashADrumPattern(IReadOnlyList<byte>? data) =>
        data is { Count: 2710 }
        && data[0] == 1 && data[1] == 0 && data[2] == 0
        && data[3] == 1 && data[4] == 1 && data[5] == 0;

    private static bool IsBashADrumSong(IReadOnlyList<byte>? data) =>
        data is { Count: 40 }
        && data.Take(10).SequenceEqual(Enumerable.Range(1, 10).Select(value => (byte)value));

    private static bool IsBearEssentialsPicture(IReadOnlyList<byte>? data) =>
        data is { Count: 8320 }
        && data[0] == 0x48 && data[1] == 0xad && data[2] == 0x6e && data[3] == 0x06
        && data[4] == 0x8d && data[5] == 0x0a && data[6] == 0xd4;

    private static bool IsAtariRawFontBank(IReadOnlyList<byte>? data) =>
        data is { Count: 3072 }
        && data.Take(8).All(value => value == 0)
        && data.Skip(8).Take(5).All(value => value == 0x38);

    private static bool IsAtariRawCharacterSet(IReadOnlyList<byte>? data) =>
        data is { Count: 1024 }
        && data.Take(8).All(value => value == 0)
        && data.Skip(8).Any(value => value != 0);

    private static bool IsEpsonFxDownloadableFont(IReadOnlyList<byte>? data) =>
        data is { Count: >= 8 }
        && data[0] == 0x1b && data[1] == (byte)':'
        && Enumerable.Range(2, Math.Min(32, data.Count - 3))
            .Any(index => data[index] == 0x1b && data[index + 1] == (byte)'&');

    private static bool IsAtariRawScreenImage(IReadOnlyList<byte>? data) =>
        data is { Count: 7625 or 7680 or 7900 or 8000 }
        || data is { Count: 10003 } && data[0] == 0x00 && data[1] == 0x40
        || data is { Count: 10018 } && data[0] == 0x00 && data[1] == 0x20;

    private static bool IsAtariRawScreenWithPalette(IReadOnlyList<byte>? data) =>
        data is { Count: 244 or 7684 or 7685 };

    private static bool IsPrintShopConverterScreen(IReadOnlyList<byte>? data) =>
        data is { Count: 7857 }
        && data.Take(7680).Any(value => value != 0)
        && data.Skip(7680).Take(176).All(value => value == 0)
        && data[7856] == 0x11;

    private static bool IsPlayerMissileGraphicsTabletImage(IReadOnlyList<byte>? data) =>
        data is { Count: 4206 }
        && data[0] == 0x17
        && data.Skip(1).Take(5).All(value => (value & 1) == 0);

    private static bool IsTypesetterIcon(FileSystemEntry entry) =>
        entry.Content is { Count: 1791 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".ts", StringComparison.OrdinalIgnoreCase);

    private static bool IsAtariTwoColorPackedBitmap(IReadOnlyList<byte>? data) =>
        data is { Count: 3072 }
        && data.All(value => ((value ^ (value >> 1)) & 0x55) == 0);

    private static bool IsPicilityImage(IReadOnlyList<byte>? data)
    {
        if (data is { Count: 15872 })
            return data.Take(1024).All(value => value == 0xff)
                && data.TakeLast(1024).All(value => value == 0x00)
                && data.Skip(1024).Take(data.Count - 2048).Any(value => value is not 0x00 and not 0xff);

        return data is { Count: 767 }
            && data[0] == 0x04 && data[1] == 0x1e
            && data.Count(value => value == 0x55) >= data.Count / 3;
    }

    private static bool IsPrintPowerCatalog(FileSystemEntry entry) =>
        entry.Content is { Count: 593 or 995 } data
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".d8a", StringComparison.OrdinalIgnoreCase)
        && data.Count(value => value == 0x9b) >= 4;

    private static bool IsPrintPowerPrinterSelection(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 7 and <= 32 }
            || data[0] != (byte)'D' || data[1] != (byte)'1' || data[2] != (byte)':')
            return false;
        var terminator = -1;
        for (var index = 3; index < data.Count; index++)
        {
            if (data[index] != 0x9b) continue;
            terminator = index;
            break;
        }
        return terminator is >= 4 and <= 15
            && data.Skip(3).Take(terminator - 3).All(value => value is >= (byte)'0' and <= (byte)'9'
                or >= (byte)'A' and <= (byte)'Z');
    }

    private static bool IsPrintPowerGraphicLibrary(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        if (!(string.Equals(extension, ".001", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".004", StringComparison.OrdinalIgnoreCase))
            || entry.Content is not { Count: >= 256 } data
            || data[1] != 0 || data[2] != 0 || data[3] != 0)
            return false;

        var imageCount = data[0];
        if (imageCount is < 1 or > 64) return false;
        var headerLength = 4 + ((imageCount - 1) * 2);
        if (headerLength >= data.Count) return false;

        var previousEnd = headerLength;
        for (var index = 0; index < imageCount - 1; index++)
        {
            var imageEnd = ReadUInt16(data, 4 + (index * 2));
            if (imageEnd <= previousEnd || imageEnd >= data.Count) return false;
            previousEnd = imageEnd;
        }

        return true;
    }

    private static bool IsRambrandtUserPattern(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".usr", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: 424 } data
            || data[0] != 0x0b || data[1] != 0 || data[2] != 0 || data[3] != 0x03)
            return false;

        byte[] signature = [0x9e, 0x7a, 0x62, 0xc4, 0x59, 0x52, 0x4e, 0xc7, 0xf6, 0xb1, 0x07];
        return new[] { 32, 116, 200, 284, 368 }
            .All(offset => data.Skip(offset).Take(signature.Length).SequenceEqual(signature));
    }

    private static bool IsRubberStampPad(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".pad", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 1791 } data
        && data.Take(256).All(value => value == 0)
        && data.Skip(256).Take(data.Count - 512).Any(value => value != 0)
        && data.Skip(data.Count - 256).All(value => value == 0);

    private static bool IsSchemaDesignDocument(FileSystemEntry entry)
    {
        if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(entry.Name))
            || entry.Content is not { } data)
            return false;

        if (data.Count == 6200)
            return data.Take(40).All(value => value == 0)
                && data[40] == 0x7f
                && data.Skip(41).Take(38).All(value => value == 0xff)
                && data[79] == 0xfe
                && data.Skip(6120).All(value => value == 0);

        return data.Count == 4750
            && data.Take(64).All(value => value == 0)
            && data[87] == 0xc0 && data[88] == 0 && data[89] == 0x60
            && data.Skip(4089).All(value => value == 0);
    }

    private static bool IsSchematicDesignerDocument(FileSystemEntry entry)
    {
        if (entry.Content is not { } data) return false;
        var extension = System.IO.Path.GetExtension(entry.Name);
        if (string.Equals(extension, ".sch", StringComparison.OrdinalIgnoreCase))
            return data.Count == 7424
                && data.Take(40).All(value => value == 0)
                && data.Skip(data.Count - 32).All(value => value == 0)
                && data.Skip(40).Take(data.Count - 72).Any(value => value != 0);

        return string.Equals(extension, ".zom", StringComparison.OrdinalIgnoreCase)
            && data.Count == 1760
            && data.Take(20).All(value => value == 0)
            && data.Skip(data.Count - 32).All(value => value == 0xff)
            && data.Skip(20).Take(data.Count - 52).Any(value => value != 0);
    }

    private static bool IsScreenAidedManagementCatalog(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".mem", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 3072 } data
        && data.Take(13).Any(value => value != 0)
        && data.Skip(13).All(value => value == 0);

    private static bool IsScreenAidedManagementPrinterProfile(IReadOnlyList<byte>? data) =>
        data is { Count: 18 }
        && data[0] == (byte)'S'
        && data.Skip(6).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'A' })
        && data.Skip(10).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'L' })
        && data.Skip(15).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'U' });

    private static bool IsScreenDumpPrinterProfile(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".par", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 64 } data
        && data.Skip(2).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'@' })
        && data.Skip(11).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'A' })
        && data.Skip(20).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'K' })
        && data.Skip(29).Take(2).SequenceEqual(new byte[] { 0x1b, (byte)'L' })
        && data.Skip(47).Take(9).All(value => value is >= 32 and < 127);

    private static bool IsDosAsciiDocument(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".asc", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: > 0 } data)
            return false;

        var hasCrLf = false;
        for (var index = 0; index < data.Count - 1; index++)
        {
            if (data[index] != 0x0d || data[index + 1] != 0x0a) continue;
            hasCrLf = true;
            break;
        }
        var textBytes = data.Count(value => value is 0x0a or 0x0d or 0x10 or 0x18 || value >= 0x20);
        return hasCrLf && textBytes >= data.Count * 0.98;
    }

    private static bool IsSoftsynthFont(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".syn", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 1568 } data
        && data.Take(10).SequenceEqual(new byte[] { 0x70, 0x70, 0x42, 0x40, 0x9c, 0x02, 0x02, 0x4f, 0x00, 0x70 })
        && data.Skip(10).Take(38).All(value => value == 0x0f)
        && data.Skip(64).Any(value => value != 0);

    private static bool IsSoftsynthComposition(IReadOnlyList<byte>? data) =>
        data is { Count: >= 128 }
        && data[0] == (byte)'S' && data[1] == (byte)'Y' && data[2] == (byte)'N' && data[3] == 0x9b
        && data.Skip(data.Count - 5).SequenceEqual(new byte[] { 0x00, 0x80, 0x20, 0x20, 0x20 });

    private static bool IsSongwriterLesson(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".ida", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 1498 } data
        && data[0] == 0x80 && data[1] == 0x8c && data[2] == 0x00
        && data.Skip(29).Take(20).SequenceEqual(new byte[]
        {
            0xae, 0x2e, 0xae, 0x2e, 0xae, 0x2e, 0xae, 0x2e, 0xae, 0x2e,
            0x8c, 0x8c, 0x8d, 0x8d, 0x8e, 0x8e, 0x8f, 0x8f, 0x90, 0x90
        })
        && data.Skip(data.Count - 25).SequenceEqual(new byte[]
        {
            0x34, 0x4f, 0x6a, 0x85, 0xa0, 0xbb, 0xd6, 0xf1, 0x0c, 0x27,
            0x91, 0x91, 0x91, 0x91, 0x91, 0x91, 0x91, 0x91,
            0x92, 0x92, 0xea, 0xea, 0xea, 0xea, 0xea
        });

    private static bool IsSoundTrackerMusic(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".muz", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 128 } data
        && data.Take(6).SequenceEqual(new byte[] { (byte)'M', (byte)'u', (byte)'s', (byte)'i', (byte)'c', (byte)' ' });

    private static bool IsSoundTrackerInstrumentData(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".dta", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 4646 } data
        && data.Take(6).SequenceEqual(new byte[] { (byte)'R', (byte)'A', (byte)'W', (byte)'D', (byte)'T', (byte)'A' });

    private static bool IsSuper3DPlotterPoints(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".pts", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 10 } data
        && data[0] > 0
        && data.Count == 4 + data[0] * 6;

    private static bool IsSuper3DPlotterLines(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".lin", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { } data)
            return false;
        if (data.Count == 0) return true;
        return data.Count >= 6
            && data.Count % 3 == 0
            && data[0] is >= 1 and <= 7
            && data.Skip(data.Count - 3).All(value => value == 0);
    }

    private static bool IsSuper3DPlotterPrinterProfile(IReadOnlyList<byte>? data) =>
        data is { Count: 18 }
        && data.SequenceEqual(new byte[]
        {
            0x1b, (byte)'@', 0x9b, 0x1b, (byte)'l', 0x00,
            0x1b, (byte)'A', 0x08, 0x9b, 0x1b, (byte)'K', 0xc0, 0x00,
            0x9b, 0x00, 0x00, 0x9b
        });

    private static bool IsSuperMailerPrinterType(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".typ", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 4 } data
        && data.SequenceEqual(new byte[] { 0x01, 0x0f, 0x12, 0x0e });

    private static bool IsTechnicolorDreamImageComponent(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        return (string.Equals(extension, ".col", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".lum", StringComparison.OrdinalIgnoreCase))
            && entry.Content is { Count: >= 128 } data
            && data[2] == 0x16 && data[3] == 0x05 && data[4] == 0x35 && data[5] == 0x00;
    }

    private static bool IsTipImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".tip", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 9 } data
        && data[0] == (byte)'T' && data[1] == (byte)'I' && data[2] == (byte)'P'
        && data[3] == 0x01 && data[4] == 0x00 && data[5] == 0xa0;

    private static bool IsTrzmielCompressedImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".cpr", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 128 } data
        && data[0] == 0x02
        && data.Take(Math.Min(128, data.Count)).Any(value => value is >= 0x82 and <= 0x8f);

    private static bool IsTrickMachineRoutine(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".dan", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { } data)
            return false;
        return data.Count == 20
                && data.Take(3).SequenceEqual(new byte[] { 0x48, 0x8a, 0x48 })
                && data.Skip(16).SequenceEqual(new byte[] { 0x68, 0xaa, 0x68, 0x40 })
            || data.Count == 88
                && data.Take(5).SequenceEqual(new byte[] { 0x68, 0xa0, 0x0b, 0xa2, 0x86 })
                && data.Skip(85).SequenceEqual(new byte[] { 0x4c, 0x62, 0xe4 });
    }

    private static bool IsTrickMusicData(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".dan", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 1056 } data
        && data.Take(6).SequenceEqual(new byte[] { 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x00 })
        && data.Skip(data.Count - 16).All(value => value == 0xf2);

    private static bool IsTrickDataTable(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".dan", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 336 } data
        && data.Take(12).SequenceEqual(new byte[] { 0x00, 0x00, 0x67, 0x00, 0x01, 0x61, 0x00, 0x02, 0x61, 0x00, 0x03, 0x69 });

    private static bool IsTrickFont(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".pfd", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 965 } data
        && data.Take(6).SequenceEqual(new byte[] { 0xb3, 0xa6, 0xaa, 0x34, 0x00, 0x00 });

    private static bool IsTrickBinaryContainer(FileSystemEntry entry) =>
        string.IsNullOrEmpty(System.IO.Path.GetExtension(entry.Name))
        && entry.Content is { Count: >= 3000 } data
        && data[0] == 0xfb && data[1] == 0xc2
        && data[data.Count - 4] == 0xff && data[data.Count - 3] == 0xff;

    private static bool IsUueDecoderBinaryCommand(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".bat", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 250 } data
        && data.Take(4).SequenceEqual(new byte[] { 0x93, 0xc1, 0x15, 0xd1 })
        && data.Skip(data.Count - 5).SequenceEqual(new byte[] { 0x20, 0xd9, 0x30, 0xa0, 0x02 });

    private static bool IsVideo130XeHelpDocument(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        if (extension.Length != 4
            || !extension.Skip(1).All(char.IsDigit)
            || entry.Content is not { Count: >= 1024 } data
            || ReadUInt16(data, 0) != data.Count - 3
            || data.Skip(3).Take(4).Any(value => value != 0))
            return false;
        var body = data.Skip(7).ToArray();
        return body.Count(value => (value & 0x7f) <= 0x3f) >= body.Length * 0.75;
    }

    private static bool IsVideoScannerImage(FileSystemEntry entry) =>
        string.IsNullOrEmpty(System.IO.Path.GetExtension(entry.Name))
        && entry.Content is { Count: 4375 } data
        && data.Take(97).All(value => value == 0)
        && data[97] == 0xc0
        && data[4347] == 0xc0
        && data.Skip(4348).All(value => value == 0)
        && data.Skip(98).Take(4249).Any(value => value != 0);

    private static bool IsVidigPaintImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".rap", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 7681 } data
        && data.Take(7680).Distinct().Skip(1).Any();

    private static bool IsXlPaintRipImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".rip", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: > 64 } data
        && data.Take(6).SequenceEqual(new byte[] { (byte)'R', (byte)'I', (byte)'P', (byte)'1', (byte)'.', (byte)'6' })
        && data.Skip(6).Take(18).SequenceEqual(new byte[]
        {
            0x20, 0x10, 0x00, 0x01, 0x00, 0x21, 0x00, 0x50, 0x00,
            0xc8, 0x00, 0x00, 0x54, 0x3a, 0x09, 0x43, 0x4d, 0x3a
        });

    private static bool IsXlPaintImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".xlp", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: > 256 and <= 32768 } data
        && (data[0] & 0x0f) == 0x04
        && (data[1] & 0x0f) == 0x08
        && (data[2] & 0x0f) == 0x0c
        && data[3] == 0x00
        && data.Skip(4).Distinct().Take(32).Count() == 32;

    private static bool IsVirtuosoComposition(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        return (string.Equals(extension, ".jl", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".jm", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".dm", StringComparison.OrdinalIgnoreCase))
            && entry.Content is { Count: 5875 or 6500 } data
            && data.Take(128).Count(value => value == 0) >= 24
            && data.Distinct().Take(128).Count() == 128;
    }

    private static bool IsVoicemasterSpeechSample(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".spe", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: >= 260 } data
            || data[0] != 0x00 || data[1] != 0x56)
            return false;

        var declaredPayloadLength = data[2] | data[3] << 8;
        return declaredPayloadLength == data.Count - 4
            && data.Skip(4).Distinct().Take(32).Count() == 32;
    }

    private static bool IsVisiCalcCatalogModule(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".cat", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 375 } data
        && data.Take(8).SequenceEqual(new byte[] { 0xaa, 0x08, 0x14, 0x0b, 0xbe, 0x0a, 0xcb, 0x09 })
        && data.Skip(data.Count - 6).SequenceEqual(new byte[] { 0xa9, 0x20, 0x99, 0x03, 0x14, 0xc8 });

    private static bool IsVisualiserHelpDocument(FileSystemEntry entry)
    {
        var extension = System.IO.Path.GetExtension(entry.Name);
        if (!entry.Name.StartsWith("HELP.", StringComparison.OrdinalIgnoreCase)
            || extension.Length != 4
            || char.ToUpperInvariant(extension[1]) is not ('C' or 'S')
            || !char.IsDigit(extension[2]) || !char.IsDigit(extension[3])
            || entry.Content is not { Count: 961 } data)
            return false;

        var hasMixedPrompt = Enumerable.Range(0, data.Count - 12)
            .Any(offset => data.Skip(offset).Take(13).SequenceEqual(new byte[]
            {
                0x30, (byte)'r', (byte)'e', (byte)'s', (byte)'s', 0x00,
                (byte)'a', (byte)'n', (byte)'y', 0x00, (byte)'k', (byte)'e', (byte)'y'
            }));
        var hasMenu = ContainsAscii(data, "exit", data.Count)
            && ContainsAscii(data, "view", data.Count)
            && ContainsAscii(data, "next", data.Count)
            && ContainsAscii(data, "page", data.Count);
        var invertedPrompt = new byte[]
        {
            0xb0, 0xf2, 0xe5, 0xf3, 0xf3, 0x80,
            0xe1, 0xee, 0xf9, 0x80, 0xeb, 0xe5, 0xf9
        };
        var hasInvertedPrompt = Enumerable.Range(0, data.Count - invertedPrompt.Length + 1)
            .Any(offset => data.Skip(offset).Take(invertedPrompt.Length).SequenceEqual(invertedPrompt));
        return hasMixedPrompt || hasMenu || hasInvertedPrompt;
    }

    private static bool IsVisualiserGraphicResource(IReadOnlyList<byte>? data) =>
        data is { Count: 3165 }
            && data.Take(6).SequenceEqual(new byte[] { 0x2b, 0x28, 0x01, 0x00, 0xa4, 0x00 })
        || data is { Count: 3328 }
            && data.Take(8).SequenceEqual(new byte[] { 0x66, 0x66, 0x66, 0x36, 0x00, 0x00, 0x00, 0xf0 });

    private static bool IsWritersToolPrinterProfile(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".ppp", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 680 } data
        && data.Take(6).SequenceEqual(new byte[] { 0xff, 0xff, 0x00, 0x6f, 0xb2, 0x71 })
        && ContainsAscii(data, "AT825", 32)
        && data.Count(value => value == 0x1b) >= 12;

    private static bool IsWritersToolExtensionModule(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".ext", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 256 } data
        && data[0] == 0x4c
        && (ContainsAscii(data, "The Writer's Tool", data.Count)
            || ContainsAscii(data, "THE WRITER'S TOOL", data.Count));

    private static bool IsAtariMadDesignerImage(FileSystemEntry entry) =>
        entry.Content is { Count: 16384 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".mbg", StringComparison.OrdinalIgnoreCase);

    private static bool IsAtariMarcoPixelEditorImage(FileSystemEntry entry) =>
        entry.Content is { Count: > 0 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".cpi", StringComparison.OrdinalIgnoreCase);

    private static bool IsAtariMegacolorEditorImage(FileSystemEntry entry) =>
        entry.Content is { Count: 7856 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".mga", StringComparison.OrdinalIgnoreCase);

    private static bool IsMultiGraphViewImage(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: > 0 } data) return false;
        var extension = System.IO.Path.GetExtension(entry.Name);
        if ((string.Equals(extension, ".256", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".sfd", StringComparison.OrdinalIgnoreCase))
            && HasAsciiPrefix(data, "S101"))
            return true;
        return string.Equals(extension, ".apa", StringComparison.OrdinalIgnoreCase) && data.Count == 7720
            || string.Equals(extension, ".inp", StringComparison.OrdinalIgnoreCase) && data.Count == 16004;
    }

    private static bool IsMantisEditorConfiguration(FileSystemEntry entry) =>
        entry.Content is { Count: 42 }
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".ecf", StringComparison.OrdinalIgnoreCase);

    private static bool IsPantherConversionTable(FileSystemEntry entry) =>
        entry.Content is { Count: > 0 } data
        && string.Equals(System.IO.Path.GetExtension(entry.Name), ".cvn", StringComparison.OrdinalIgnoreCase)
        && data[0] == 0x9b
        && ContainsAscii(data, "#13=", data.Count)
        && ContainsAscii(data, "#10=", data.Count);

    private static bool IsPaperclipPrinterConfiguration(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: > 0 } data) return false;
        var extension = System.IO.Path.GetExtension(entry.Name);
        return string.Equals(extension, ".cnf", StringComparison.OrdinalIgnoreCase) && data.Count == 190
            || string.Equals(extension, ".cng", StringComparison.OrdinalIgnoreCase)
                && data.Count <= 64 && data.Contains((byte)0x9b) && data.Contains((byte)0xff);
    }

    private static bool IsNewsroomPhoto(IReadOnlyList<byte>? data) =>
        data is { Count: >= 64 }
        && data[0] == 0xff && data[1] == 0xff && data[2] == 0x00 && data[3] == 0xa0
        && ContainsAscii(data, "NEWSROOM", 128);

    private static bool IsEasyScanConfiguration(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: 120 } || data.Take(28).Any(value => value != 0) || data[28] != 2 || data[29] != 1)
            return false;
        for (var index = 0; index < 16; index++)
        {
            if (data[53 + index] != (byte)(((index + 1) & 15) << 4)) return false;
        }
        return true;
    }

    private static bool IsAtariPrinterControlProfile(IReadOnlyList<byte>? data) =>
        data is { Count: 9 }
        && data[0] == 0x1b && data[2] == 0x9b
        && data[3] == 0x1b && data[5] == 0x9b
        && data[6] == 0x1b && data[8] == 0x9b;

    private static bool IsK3WaveTableUtility(IReadOnlyList<byte>? data) =>
        data is { Count: >= 5 }
        && (data[0] == 0x20 && data[1] == 0xc3 && data[2] == 0x55 && data[3] == 0x4c && data[4] == 0xbb
            || data[0] == 0x20 && data[1] == 0x03 && data[2] == 0x50 && data[3] == 0xa2 && data[4] == 0x70
            || data[0] == 0x4c && data[1] == 0x42 && data[2] == 0x50 && data[3] == 0xa2 && data[4] == 0x70);

    private static bool IsKyanPascalEditor(IReadOnlyList<byte>? data) =>
        data is { Count: >= 512 }
        && ReadUInt16(data, 0) == 0xffff
        && ReadUInt16(data, 2) == 0x2005
        && ReadUInt16(data, 4) == 0x342a
        && ContainsAscii(data, "Not a Load File", data.Count)
        && ContainsAscii(data, "Invailid Device", data.Count);

    private static bool IsFontMakerEditor(IReadOnlyList<byte>? data) =>
        data is { Count: 2668 }
        && ContainsAscii(data, "FontMaker by Charles Brannon", 512)
        && ContainsAscii(data, "Pick a character...", data.Count);

    private static bool IsLasertellerCore(IReadOnlyList<byte>? data) =>
        data is { Count: >= 512 }
        && data[0] == 0x4c && data[1] == 0xd4 && data[2] == 0x46 && data[3] == 0x48
        && ContainsAscii(data, "D:LASXML.DAT", 256)
        && ContainsAscii(data, "D:COREML.DAT", 256);

    private static bool IsCentroDeCostosControl(IReadOnlyList<byte>? data) =>
        data is { Count: 16 }
        && data.Take(7).All(value => value == 2)
        && data.Skip(7).Take(7).All(value => value == 1)
        && data[14] == 0x1d && data[15] == 0x9b;

    private static bool IsDigiVoiceSample(IReadOnlyList<byte>? data) =>
        data is { Count: 32518 }
        && ReadUInt16(data, 0) == 0xffff
        && ReadUInt16(data, 2) == 0x4000
        && ReadUInt16(data, 4) == 0xbf00;

    private static bool IsAngRawDigitizedSample(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: >= 512 and <= 4096 } data
            || !entry.Name.StartsWith("DOCUMENT.", StringComparison.OrdinalIgnoreCase))
            return false;

        var extension = System.IO.Path.GetExtension(entry.Name);
        if (extension.Length <= 1 || !int.TryParse(extension.AsSpan(1), out _)) return false;

        return data.Distinct().Count() >= 48
            && data.Count(value => value == 0) * 4 < data.Count
            && data.Count(value => (value & 0x80) != 0) * 5 >= data.Count * 2;
    }

    private static bool IsDiskWizardSpeechBasic(IReadOnlyList<byte>? data) =>
        data is { Count: >= 100 }
        && (ContainsAscii(data, "RETURN TO MAIN MENU", data.Count)
            || ContainsAscii(data, "USERS CLUB", data.Count));

    private static bool IsDiskWizardSpeechLibrary(IReadOnlyList<byte>? data) =>
        data is { Count: 145 }
        && data[0] == 0xf4 && data[1] == 0x14
        && data[2] == 0xa6 && data[3] == 0xd2;

    private static ExplorerFileCategory? MetadataCategory(FileSystemEntry entry, ExplorerFileSystemFamily family)
    {
        var type = entry.Comment.Trim();
        if (family == ExplorerFileSystemFamily.Commodore && type.StartsWith("PRG", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Program;
        if (family == ExplorerFileSystemFamily.AppleDos && type is "Text") return ExplorerFileCategory.Text;
        if (family == ExplorerFileSystemFamily.AppleDos && type is "Integer BASIC" or "Applesoft BASIC") return ExplorerFileCategory.BasicProgram;
        if (family == ExplorerFileSystemFamily.ProDos && type is "Text") return ExplorerFileCategory.Text;
        if (family == ExplorerFileSystemFamily.ProDos && type is "BASIC") return ExplorerFileCategory.BasicProgram;
        if (family == ExplorerFileSystemFamily.ProDos && type is "System") return ExplorerFileCategory.System;
        if (family == ExplorerFileSystemFamily.Macintosh)
        {
            if (type.Equals("APPL", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Executable;
            if (type.Equals("TEXT", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Text;
            if (type.Equals("PICT", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Image;
            if (type.Equals("snd", StringComparison.OrdinalIgnoreCase) || type.Equals("AIFF", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Audio;
        }
        if (family == ExplorerFileSystemFamily.Ucsd)
        {
            if (type.Equals("UCSD code file", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Executable;
            if (type.Equals("UCSD text file", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Text;
            if (type is "UCSD graphics file" or "UCSD photo file") return ExplorerFileCategory.Image;
        }
        return null;
    }

    private static bool IsAmigaExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 4 } && data[0] == 0 && data[1] == 0 && data[2] == 3 && data[3] == 0xF3;

    private static bool IsDosExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 2 } && data[0] == (byte)'M' && data[1] == (byte)'Z';

    private static bool IsAtariExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 2 } && data[0] == 0x60 && data[1] == 0x1A;

    private static bool IsAtari8BitBasicProgram(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 14 } || ReadUInt16(data, 0) != 0) return false;
        var vntp = ReadUInt16(data, 2);
        var vntd = ReadUInt16(data, 4);
        var vvtp = ReadUInt16(data, 6);
        var stmtab = ReadUInt16(data, 8);
        var stmcur = ReadUInt16(data, 10);
        var starp = ReadUInt16(data, 12);
        var origin = vntp - 14;
        var normalizedVntp = vntp - origin;
        var normalizedVntd = vntd - origin;
        var normalizedVvtp = vvtp - origin;
        var normalizedStmtab = stmtab - origin;
        var normalizedStmcur = stmcur - origin;
        var normalizedStarp = starp - origin;
        if (vntp < 0x0100
            || normalizedVntp != 14
            || normalizedVntp > normalizedVntd
            || normalizedVntd > normalizedVvtp
            || normalizedVvtp > normalizedStmtab
            || normalizedStmtab >= data.Count) return false;
        if (normalizedStmtab < normalizedStmcur
            && normalizedStmcur <= normalizedStarp
            && normalizedStarp <= data.Count) return true;
        return HasValidAtari8BitBasicLineTable(data, normalizedStmtab);
    }

    private static bool IsAtari8BitPackedCassetteBasicProgram(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: >= 1024 } data
            || !entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
            || !sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
            || !entry.Metadata.TryGetValue("checksumPresent", out var checksumPresent)
            || !bool.TryParse(checksumPresent, out var hasChecksums) || !hasChecksums
            || !entry.Metadata.TryGetValue("endRecordPresent", out var endRecordPresent)
            || !bool.TryParse(endRecordPresent, out var hasEndRecord) || !hasEndRecord
            || ReadUInt16(data, 0) != 0)
            return false;

        var vntp = ReadUInt16(data, 2);
        var vntd = ReadUInt16(data, 4);
        var vvtp = ReadUInt16(data, 6);
        var stmtab = ReadUInt16(data, 8);
        var stmcur = ReadUInt16(data, 10);
        var starp = ReadUInt16(data, 12);
        var expandedLength = starp - vntp + 14;
        return vntp == 0x0200
            && vntd > vntp && vntd - vntp <= 0x0100
            && vvtp >= vntd && vvtp - vntd <= 16
            && stmtab > vvtp
            && stmcur > stmtab
            && starp >= stmcur
            && expandedLength * 100 >= data.Count * 190
            && expandedLength * 100 <= data.Count * 205;
    }

    private static bool IsAtari8BitAtasciiBasicListing(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 32 } || data[^1] != 0x9b) return false;
        var offset = 0;
        var lines = 0;
        var previousLine = -1;
        while (offset < data.Count)
        {
            var line = 0;
            var digitCount = 0;
            while (offset < data.Count && data[offset] is >= (byte)'0' and <= (byte)'9' && digitCount < 5)
            {
                line = line * 10 + data[offset] - (byte)'0';
                offset++;
                digitCount++;
            }
            if (digitCount == 0 || offset >= data.Count || data[offset] != (byte)' ' || line < previousLine)
                return false;
            previousLine = line;
            var lineEnd = offset + 1;
            while (lineEnd < data.Count && data[lineEnd] != 0x9b) lineEnd++;
            if (lineEnd >= data.Count) return false;
            lines++;
            offset = lineEnd + 1;
        }
        return lines >= 3
            && (ContainsAscii(data, " POKE ", data.Count)
                || ContainsAscii(data, "GRAPHICS ", data.Count))
            && ContainsAscii(data, "USR(", data.Count);
    }

    private static bool IsAtariMac65Source(IReadOnlyList<byte>? data) =>
        data is { Count: >= 32 }
        && data[0] == 0xfe && data[1] == 0xfe
        && ContainsAscii(data, ";", 512);

    private static bool HasValidAtari8BitBasicLineTable(IReadOnlyList<byte> data, int offset)
    {
        var lines = 0;
        var previousLine = -1;
        while (offset < data.Count)
        {
            if (offset + 3 > data.Count) return lines >= 3;
            var line = ReadUInt16(data, offset);
            var length = data[offset + 2];
            if (line < previousLine || length < 4 || offset + length > data.Count) return lines >= 3;
            previousLine = line;
            offset += length;
            lines++;
        }
        return lines > 0;
    }

    private static bool IsAtari8BitXex(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 7 } || ReadUInt16(data, 0) != 0xffff) return false;
        var offset = 2;
        var segments = 0;
        while (offset < data.Count)
        {
            while (offset + 1 < data.Count && ReadUInt16(data, offset) == 0xffff) offset += 2;
            if (segments > 0 && data.Skip(offset).All(value => value is 0 or 0x1a or 0x9b)) return true;
            if (offset + 4 > data.Count) return false;
            var start = ReadUInt16(data, offset);
            var end = ReadUInt16(data, offset + 2);
            if (end < start) return false;
            offset += 4;
            var length = end - start + 1;
            if (offset + length > data.Count) return false;
            offset += length;
            segments++;
        }
        return segments > 0;
    }

    private static bool IsAtariPascalExecutable(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 10 }
            || ReadUInt16(data, 0) != 0xffff
            || ReadUInt16(data, 2) != 0x2000)
            return false;
        var endAddress = ReadUInt16(data, 4);
        if (endAddress < 0x2000) return false;
        var entryOffset = 6 + endAddress - 0x2000 + 1;
        return entryOffset + 4 == data.Count
            && ReadUInt16(data, entryOffset) == 0x2000
            && ReadUInt16(data, entryOffset + 2) == 0x2000;
    }

    private static bool IsAtari8BitExecutableWithEmbeddedPayload(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 12 } || ReadUInt16(data, 0) != 0xffff) return false;
        var offset = 2;
        var loadedRanges = new List<(int Start, int End)>();
        var validEntryVector = false;
        while (offset < data.Count)
        {
            while (offset + 1 < data.Count && ReadUInt16(data, offset) == 0xffff) offset += 2;
            if (offset + 4 > data.Count) return validEntryVector || HasEmbeddedAtariEntryVector(data, offset, loadedRanges);
            var start = ReadUInt16(data, offset);
            var end = ReadUInt16(data, offset + 2);
            if (end < start) return validEntryVector || HasEmbeddedAtariEntryVector(data, offset, loadedRanges);
            offset += 4;
            var length = end - start + 1;
            if (offset + length > data.Count)
            {
                var declaredRanges = loadedRanges.Append((start, end)).ToArray();
                return validEntryVector || HasEmbeddedAtariEntryVector(data, offset - 4, declaredRanges);
            }
            if (length == 2 && (start == 0x02e0 || start == 0x02e2))
            {
                var target = ReadUInt16(data, offset);
                validEntryVector = loadedRanges.Any(range => target >= range.Start && target <= range.End);
            }
            loadedRanges.Add((start, end));
            offset += length;
        }
        return false;
    }

    private static bool HasEmbeddedAtariEntryVector(
        IReadOnlyList<byte> data,
        int searchOffset,
        IReadOnlyList<(int Start, int End)> loadedRanges)
    {
        for (var offset = Math.Max(0, searchOffset); offset + 5 < data.Count; offset++)
        {
            var start = ReadUInt16(data, offset);
            if (start is not (0x02e0 or 0x02e2) || ReadUInt16(data, offset + 2) != start + 1) continue;
            var target = ReadUInt16(data, offset + 4);
            if (loadedRanges.Any(range => target >= range.Start && target <= range.End)) return true;
        }
        return false;
    }

    private static bool IsAtari8BitCassetteBootProgram(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 128 } || data[1] == 0) return false;
        var recordCount = data[1];
        var loadAddress = ReadUInt16(data, 2);
        var initAddress = ReadUInt16(data, 4);
        var loadedEnd = loadAddress + recordCount * 128;
        return loadAddress > 0
            && loadedEnd <= 0x10000
            && (initAddress == 0 || initAddress >= loadAddress && initAddress < loadedEnd)
            && data.Count > (recordCount - 1) * 128;
    }

    private static bool IsAtari8BitDiskBootProgram(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 128 }
            || data[1] == 0
            || data.Count != data[1] * 128)
            return false;

        var loadAddress = ReadUInt16(data, 2);
        var continuationAddress = ReadUInt16(data, 4);
        return loadAddress > 0
            && loadAddress + data.Count <= 0x10000
            && continuationAddress > 0;
    }

    private static bool IsAtari8BitDosBootImageFile(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 125 }
            || data[0] != 0
            || data[1] == 0
            || data.Count <= (data[1] - 1) * 125
            || data.Count > data[1] * 125)
            return false;

        var loadAddress = ReadUInt16(data, 2);
        var continuationAddress = ReadUInt16(data, 4);
        return loadAddress >= 0x0400
            && loadAddress + data[1] * 128 <= 0x10000
            && continuationAddress >= loadAddress
            && continuationAddress < loadAddress + data[1] * 128;
    }

    private static bool IsAtariBasicBlockLoadedLogo(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".fun", StringComparison.OrdinalIgnoreCase)
            || !System.IO.Path.GetFileNameWithoutExtension(entry.Name).Contains("LOGO", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: 160 or 4096 } data)
            return false;

        return data.Count(value => value == 0) >= data.Count / 8
            && data.Distinct().Take(16).Count() == 16;
    }

    private static bool IsAtariPlayerMissileBlankingRoutine(FileSystemEntry entry) =>
        string.Equals(entry.Name, "PMBLANK.FIL", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 256 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0xa2, 0x03, 0xbd, 0xf4, 0x06, 0xf0, 0x59, 0x38,
            0xdd, 0xf0, 0x06, 0xf0, 0x53, 0x8d, 0xfe, 0x06
        })
        && data.Skip(155).Take(16).SequenceEqual(new byte[]
        {
            0x4c, 0x62, 0xe4, 0x00, 0x00, 0x68, 0xa9, 0x07,
            0xa2, 0x06, 0xa0, 0x00, 0x20, 0x5c, 0xe4, 0x60
        })
        && data.Skip(171).All(value => value == 0);

    private static bool IsAtariGraphicsDefinition(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".gdf", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 16 and <= 2048 } data
        && data.Count % 8 == 0
        && data.Distinct().Take(8).Count() == 8;

    private static bool IsAtariGraphicsMemoryPlane(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".mem", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 2560 and <= 7680 } data
        && data.Count % 40 == 0
        && data.Count(value => value == 0) >= data.Count / 16
        && data.Distinct().Take(16).Count() == 16;

    private static bool IsMicroProseAtariTileMap(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".map", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: >= 66 and <= 8192 } data)
            return false;

        var width = data[0];
        var height = data[1];
        if (width < 8 || height < 8) return false;
        var headerLength = data.Count - width * height;
        return headerLength is 2 or 42
            && data.Skip(headerLength).Distinct().Take(16).Count() == 16;
    }

    private static bool IsMicroProseAtariEffectSequence(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".eff", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 480 } data
        && data.Take(12).SequenceEqual(new byte[]
        {
            0x81, 0x02, 0x00, 0x05, 0x0a, 0x06, 0x01, 0xff,
            0x01, 0x01, 0x01, 0x01
        })
        && data.Count(value => value == 0x80) >= 200
        && data.Count(value => value == 0x81) >= 8
        && data.Count(value => value == 0x01) >= 80;

    private static bool IsAtariCadVectorDrawing(FileSystemEntry entry)
    {
        if (!string.Equals(System.IO.Path.GetExtension(entry.Name), ".cad", StringComparison.OrdinalIgnoreCase)
            || entry.Content is not { Count: >= 16 } data
            || data[0] != (byte)'C' || data[1] != (byte)'A' || data[2] != (byte)'D'
            || data[3] is 0 or > 64 || data[4] != 0 || data[5] != 0)
            return false;

        var coordinateCount = data[3] + 1;
        var tableEnd = 6 + coordinateCount * 3;
        if (tableEnd >= data.Count) return false;
        for (var index = 0; index < coordinateCount; index++)
        {
            if (data[6 + index * 3] != 0x2c) return false;
        }
        return data.Skip(tableEnd).Any(value => value != 0);
    }

    private static bool IsAtariSparseTileMap(IReadOnlyList<byte>? data) =>
        data is { Count: 1000 }
        && data.Count(value => value == 0) >= 400
        && data.Distinct().Take(33).Count() <= 32
        && data.Any(value => value != 0);

    private static bool IsSupertronsCharacterSet(FileSystemEntry entry) =>
        string.Equals(entry.Name, "ZSP", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 1024 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00,
            0x18, 0x18, 0x18, 0x18, 0x18, 0x00, 0x18, 0x00
        })
        && data.Count(value => value == 0) >= 128
        && data.Distinct().Take(32).Count() == 32;

    private static readonly byte[] AtariPrexorRoutineSignature =
    {
        0xa9, 0x00, 0x85, 0xf0, 0x85, 0xf2, 0xa9, 0x35,
        0x85, 0xf1, 0xa9, 0x05, 0x85, 0xf3, 0xa0, 0x00,
        0xb1, 0xf0, 0x91, 0xf2, 0xa9, 0x00, 0x91, 0xf0
    };

    private static bool IsAtariPrexorPackedExecutable(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 512 }
            || ReadUInt16(data, 0) != 0xffff
            || data.Count < 6)
            return false;
        var start = ReadUInt16(data, 2);
        var end = ReadUInt16(data, 4);
        return end >= start
            && end - start + 1 > data.Count
            && ContainsSequence(data, AtariPrexorRoutineSignature);
    }

    private static bool IsAtariPrexorRoutine(IReadOnlyList<byte>? data) =>
        data is { Count: 328 }
        && data.Take(64).All(value => value == 0)
        && ContainsSequence(data, AtariPrexorRoutineSignature)
        && data.Skip(data.Count - 32).All(value => value == 0x60);

    private static bool IsAtariA000SelfExtractingExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: 1190 }
        && ReadUInt16(data, 0) == 0xffff
        && ReadUInt16(data, 2) == 0xa000
        && ReadUInt16(data, 4) == 0xbfff
        && data.Skip(6).Take(24).SequenceEqual(new byte[]
        {
            0xd8, 0xa9, 0xaa, 0x85, 0xc1, 0xa9, 0x00, 0x85,
            0xc0, 0xa9, 0x2a, 0x85, 0xc3, 0xa9, 0x00, 0x85,
            0xc2, 0xa0, 0x00, 0xb1, 0xc0, 0x91, 0xc2, 0xc8
        });

    private static bool IsAtariBurgersPackedExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: 7216 }
        && data.Take(16).SequenceEqual(new byte[]
        {
            0xff, 0xff, 0x00, 0x40, 0x30, 0x5f, 0x00, 0x3e,
            0x00, 0x20, 0x9c, 0x35, 0x4c, 0x9c, 0x35, 0x00
        })
        && data.Distinct().Take(128).Count() == 128;

    private static bool IsAtariMiner2049PackedExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: 17164 }
        && data.Take(16).SequenceEqual(new byte[]
        {
            0xff, 0xff, 0xfe, 0x60, 0x5d, 0x60, 0xa9, 0x23,
            0x8d, 0x30, 0x02, 0x8d, 0x02, 0xd4, 0xa9, 0x60
        })
        && data.Skip(data.Count - 6).SequenceEqual(new byte[] { 0xe2, 0x02, 0xe3, 0x02, 0x80, 0x7b });

    private static bool IsAtariUnicumLevel(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".uni", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 3044 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x00, 0x00, 0x32, 0x04, 0x12, 0x72, 0xd2, 0x72,
            0x52, 0x32, 0x92, 0x92, 0xa2, 0x32, 0x04, 0x62
        })
        && data.Distinct().Take(64).Count() == 64;

    private static bool IsAtariCassetteRecordProgram(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data)
            return false;

        return data.Take(16).SequenceEqual(new byte[]
            {
                0xea, 0xea, 0xea, 0xa0, 0x00, 0xa9, 0x10, 0x99,
                0x63, 0x06, 0xc8, 0xc0, 0x0f, 0xd0, 0xf8, 0xa9
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0xa9, 0x00, 0x85, 0xd0, 0x85, 0xcf, 0xa9, 0xc8,
                0x85, 0xcd, 0xa9, 0x20, 0x85, 0xce, 0xa2, 0x00
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0x4c, 0x1f, 0x60, 0x20, 0x96, 0x61, 0xad, 0x04,
                0x03, 0x8d, 0x52, 0x61, 0xad, 0x05, 0x03, 0x8d
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0x4c, 0xac, 0x4c, 0xa9, 0x00, 0x8d, 0x45, 0x06,
                0x8d, 0x02, 0x06, 0x8d, 0x03, 0x06, 0x8d, 0x04
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0xa9, 0x08, 0x8d, 0x07, 0xd4, 0xa9, 0x3e, 0x8d,
                0x2f, 0x02, 0xa9, 0x03, 0x8d, 0x1d, 0xd0, 0x8d
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0x4c, 0x62, 0x29, 0x4c, 0x71, 0x29, 0xce, 0x96,
                0x22, 0x10, 0x06, 0xa9, 0x06, 0x8d, 0x96, 0x22
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0xae, 0x00, 0x51, 0xe0, 0x10, 0xf0, 0x20, 0xad,
                0x06, 0x6c, 0xd0, 0x08, 0xce, 0x0a, 0x51, 0xa0
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0x4c, 0x22, 0x0b, 0x4c, 0x63, 0x0b, 0x4c, 0xa4,
                0x0b, 0xa5, 0x72, 0x8d, 0xc0, 0x0c, 0xa5, 0x73
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0x0f, 0xc9, 0x3a, 0xd0, 0x07, 0xa5, 0xb1, 0xf0,
                0x05, 0x8d, 0xc8, 0x05, 0xa9, 0x00, 0x18, 0x60
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0xc9, 0x02, 0x00, 0x05, 0x11, 0x85, 0xa2, 0x7f,
                0xbd, 0x00, 0x04, 0x9d, 0x00, 0x06, 0xca, 0x10
            })
            || data.Take(16).SequenceEqual(new byte[]
            {
                0xa2, 0x02, 0x00, 0x05, 0x00, 0xff, 0xa2, 0x7f,
                0xbd, 0x00, 0x04, 0x9d, 0x00, 0x06, 0xca, 0x10
            });
    }

    private static bool IsAtariCassetteGraphicRecordStream(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data) return false;
        return data is { Count: 8448 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x04, 0x04, 0x04, 0xa6, 0xa6, 0x04, 0x04, 0x04
                })
                && data.Distinct().Take(128).Count() == 128
            || data is { Count: 4096 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x02, 0x00, 0x4e, 0x40, 0x70, 0x56, 0x56, 0x80,
                    0x0c, 0x00, 0x0c, 0x01, 0x06, 0x00, 0xce, 0x8c
                })
                && data.Count(value => value == 0) >= 2048;
    }

    private static bool IsAtariCassetteDataRecordStream(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data) return false;
        return data is { Count: 6528 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x70, 0x70, 0x70, 0xc2, 0x60, 0x43, 0x0d, 0x0d,
                    0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d
                })
                && data.Distinct().Take(128).Count() == 128
            || data is { Count: 21760 }
                && data.Take(34).All(value => value == 0)
                && data.Skip(34).Take(2).SequenceEqual(new byte[] { 0x0f, 0xc0 })
                && data.Distinct().Take(128).Count() == 128;
    }

    private static bool IsCompleteAtariCassetteRecordStream(FileSystemEntry entry) =>
        entry.Content is { Count: >= 256 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCountText)
        && int.TryParse(fullCountText, out var fullCount)
        && fullCount >= 2 && data.Count == fullCount * 128
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "0"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd;

    private static bool IsAtariCassettePartialDataRecordStream(FileSystemEntry entry) =>
        entry.Content is { Count: 6460 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "50"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "1"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x28, 0x03, 0x01, 0x02, 0x20, 0xb2, 0x00, 0x0c,
            0x10, 0x07, 0x50, 0x03, 0x00, 0x02, 0x20, 0xba
        })
        && data.Distinct().Take(128).Count() == 128;

    private static bool IsAtariCassettePartialProgramStream(FileSystemEntry entry) =>
        entry.Content is { Count: 10667 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "83"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "1"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x01, 0x00, 0x02, 0x41, 0x02, 0xc6, 0x01, 0xc5,
            0x02, 0xc4, 0x01, 0xc5, 0x01, 0xc6, 0x0a, 0x00
        })
        && ContainsSequence(data, new byte[] { 0xad, 0x58, 0x1e, 0xd0, 0x14, 0xa0, 0x00, 0xae, 0xef, 0x1e });

    private static bool IsAtariCassetteAdventureDatabase(FileSystemEntry entry) =>
        entry.Content is { Count: 10031 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "78"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "1"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd
        && ContainsAscii(data, "in a dark hall", data.Count)
        && ContainsAscii(data, "SILVER CRUCIFIX", data.Count)
        && ContainsAscii(data, "North,South,East", data.Count);

    private static bool IsAtariCassetteCompilationDataStream(FileSystemEntry entry) =>
        entry.Content is { Count: 10362 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "80"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "1"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && hasEnd
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x70, 0xf0, 0x70, 0x4e, 0x50, 0x11, 0x0e, 0x0e,
            0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e
        })
        && data.Distinct().Take(128).Count() == 128;

    private static bool IsAtariCassetteAssemblerSourceStream(FileSystemEntry entry) =>
        IsCompleteAtariCassetteRecordStream(entry)
        && entry.Content is { Count: 16384 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x75, 0x00, 0x00, 0xd3, 0x5f, 0x01, 0x11, 0x07,
            0x08, 0x01, 0x00, 0x01, 0x80, 0x01, 0x00, 0x13
        })
        && ContainsAscii(data, "LDA", data.Count)
        && ContainsAscii(data, "STA", data.Count)
        && ContainsAscii(data, "WSYNC", data.Count)
        && ContainsAscii(data, "CHBASE", data.Count);

    private static bool IsAtariWhoDaresWinsGameData(FileSystemEntry entry) =>
        IsCompleteAtariCassetteRecordStream(entry)
        && entry.Content is { Count: 13824 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x00,
            0x00, 0x00, 0x00, 0x05, 0x06, 0x07, 0x08, 0x09
        })
        && ContainsAscii(data, "SCORE", data.Count)
        && ContainsAscii(data, "LIVES", data.Count)
        && ContainsAscii(data, "OUTPOST", data.Count)
        && ContainsAscii(data, "CAPTURED", data.Count);

    private static bool IsAtariWhoDaresWinsGraphics(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data)
            return false;

        return data is { Count: 24576 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x00, 0x05, 0xe6, 0xa2, 0x2a, 0xa0, 0x00, 0x05,
                    0xad, 0xa8, 0x2a, 0xa0, 0x01, 0x05, 0xad, 0xa6
                })
            || data is { Count: 8192 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x88, 0x85, 0x84, 0x89, 0x80, 0x83, 0x89, 0x87,
                    0x86, 0x81, 0x85, 0x86, 0x81, 0x84, 0x80, 0x84
                });
    }

    private static bool IsAtariPhantomGameData(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data)
            return false;

        return data is { Count: 512 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x04, 0x09, 0x0a, 0x0b, 0x0c, 0x0e, 0x11, 0x13,
                    0x14, 0x15, 0x16, 0x18, 0x00, 0x04, 0x09, 0x0e
                })
            || data is { Count: 1536 }
                && (data.Take(16).SequenceEqual(new byte[]
                    {
                        0x40, 0xcf, 0x05, 0x51, 0x05, 0x4f, 0x05, 0x4d,
                        0x05, 0x4b, 0x45, 0x69, 0x49, 0x29, 0x8d, 0x69
                    })
                    || data.Take(16).SequenceEqual(new byte[]
                    {
                        0x40, 0xcf, 0x82, 0x2e, 0xc6, 0x2e, 0x8a, 0x2e,
                        0xce, 0x2e, 0x95, 0x2e, 0xd9, 0x2e, 0x5b, 0x4c
                    })
                    || data.Take(16).SequenceEqual(new byte[]
                    {
                        0x66, 0xd0, 0x62, 0x2f, 0x5e, 0x2f, 0x9a, 0x2f,
                        0xd3, 0x2f, 0x4f, 0x2f, 0x1a, 0x4d, 0x1a, 0x4b
                    }));
    }

    private static bool IsAtariPhantomGraphics(FileSystemEntry entry)
    {
        if (!IsCompleteAtariCassetteRecordStream(entry) || entry.Content is not { } data)
            return false;

        return data is { Count: 6912 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0xf3, 0xa8, 0x79, 0xa8, 0x00, 0xa9, 0xf3, 0xa6,
                    0x79, 0xa6, 0x00, 0xa3, 0xf3, 0xa4, 0x79, 0xa4
                })
            || data is { Count: 2304 }
                && data.Take(24).All(value => value == 0)
                && data.Count(value => value == 0) == 772
            || data is { Count: 5376 }
                && data.Take(16).SequenceEqual(new byte[]
                {
                    0x66, 0x9a, 0xa9, 0x66, 0x9a, 0x9a, 0x66, 0xa9,
                    0x66, 0x9a, 0xa9, 0x66, 0x9a, 0x9a, 0x66, 0xa9
                })
            || data is { Count: 5376 }
                && ContainsAscii(data, "LOADING PHANTOM", data.Count)
                && ContainsAscii(data, "SET TAPE COUNTER TO ZERO", data.Count);
    }

    private static bool IsAnime4EverImage(FileSystemEntry entry) =>
        entry.Content is { Count: >= 512 } data
        && data[2] == 0x00
        && data[3] == 0x90
        && data[4] == 0x4f
        && data[^3] == 0xdb
        && data[^2] == 0x01
        && data[^1] == 0x00;

    private static bool IsAtariLcRasterImage(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".lc", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: 9440 };

    private static bool IsAtariDgtRawSample(FileSystemEntry entry) =>
        string.Equals(System.IO.Path.GetExtension(entry.Name), ".dgt", StringComparison.OrdinalIgnoreCase)
        && entry.Content is { Count: >= 4096 } data
        && data.Distinct().Take(128).Count() == 128;

    private static bool IsAtariGtfImage(FileSystemEntry entry) =>
        entry.Content is { Count: >= 11 } data
        && data[0] == (byte)'G'
        && data[1] == (byte)'T'
        && data[2] == (byte)'F'
        && data[3] == 0
        && data[4] > 0
        && data[5] > 0
        && data.Count == 10 + (data[4] * data[5] / 2);

    private static bool IsAtariTrackerAudio(FileSystemEntry entry)
    {
        if (entry.Content is not { } data)
            return false;

        var extension = System.IO.Path.GetExtension(entry.Name);
        if (extension.Equals(".md1", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".tmc", StringComparison.OrdinalIgnoreCase))
        {
            return data.Count >= 32 && data[0] == 0xff && data[1] == 0xff;
        }

        return extension.Equals(".d15", StringComparison.OrdinalIgnoreCase)
            && data is { Count: > 0 and <= 12288 }
            && data.Distinct().Take(4).Count() == 4;
    }

    private static bool IsAtariCharacterGeneratorCassetteProgram(FileSystemEntry entry) =>
        entry.Content is { Count: 2176 } data
        && entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
        && sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
        && entry.Metadata.TryGetValue("fullRecordCount", out var fullCount) && fullCount == "17"
        && entry.Metadata.TryGetValue("partialRecordCount", out var partialCount) && partialCount == "0"
        && entry.Metadata.TryGetValue("endRecordPresent", out var endPresent)
        && bool.TryParse(endPresent, out var hasEnd) && !hasEnd
        && ContainsAscii(data, "CHARACTER generator", data.Count)
        && ContainsAscii(data, "P.B. SOFTWARE", data.Count)
        && ContainsAscii(data, "EDIT CHARACTER", data.Count);

    private static bool IsAtariAstroChaseCassetteProgram(FileSystemEntry entry) =>
        IsCompleteAtariCassetteRecordStream(entry)
        && entry.Content is { Count: 16640 } data
        && data.Take(16).SequenceEqual(new byte[]
        {
            0x10, 0x01, 0x00, 0x10, 0x01, 0x01, 0x00, 0x11,
            0x01, 0x10, 0x00, 0x00, 0x00, 0x01, 0x10, 0x00
        })
        && ContainsAscii(data, "ASTRO CHASE", data.Count)
        && ContainsAscii(data, "FERNANDO HERRERA", data.Count)
        && ContainsAscii(data, "FIRST STAR SOFTWARE", data.Count);

    private static bool IsAtariLogoInterpreterMemoryImage(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: 16384 } data
            || !entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
            || !sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
            || !entry.Metadata.TryGetValue("recordCount", out var recordCount) || recordCount != "128"
            || !entry.Metadata.TryGetValue("fullRecordCount", out var fullRecordCount) || fullRecordCount != "128"
            || !entry.Metadata.TryGetValue("partialRecordCount", out var partialRecordCount) || partialRecordCount != "0"
            || !entry.Metadata.TryGetValue("endRecordPresent", out var endRecordPresent)
            || !bool.TryParse(endRecordPresent, out var hasEndRecord) || hasEndRecord)
            return false;
        return ContainsAtasciiText(data, "BRAK MIEJSCA")
            && ContainsAtasciiText(data, "NIE MOGE OTWORZYC")
            && ContainsAtasciiText(data, "PRZERWANE!");
    }

    private static bool IsAtari8BitAbcCompiledProgram(IReadOnlyList<byte>? data) =>
        data is { Count: >= 128 }
        && data[0] == 0xff && data[1] == 0xff
        && data[2] == 0x00 && data[3] == 0x26
        && data[4] == 0x26 && data[5] == 0x00
        && ContainsAscii(data, "RUNTIME ERROR", 512);

    private static bool IsAtari8BitAbcRuntimeInterpreter(IReadOnlyList<byte>? data) =>
        data is { Count: 5131 }
        && ReadUInt16(data, 0) == 0xffff
        && ReadUInt16(data, 2) == 0x2600
        && ReadUInt16(data, 4) == 0x37c5
        && ContainsAscii(data, "RUNTIME ERROR", 512);

    private static int ReadUInt16(IReadOnlyList<byte> data, int offset) =>
        data[offset] | data[offset + 1] << 8;

    private static bool HasFormType(IReadOnlyList<byte>? data, string type) =>
        data is { Count: >= 12 } &&
        data[0] == (byte)'F' && data[1] == (byte)'O' && data[2] == (byte)'R' && data[3] == (byte)'M' &&
        data.Skip(8).Take(4).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(type));

    private static bool HasAsciiPrefix(IReadOnlyList<byte>? data, string signature) =>
        data is not null && data.Count >= signature.Length &&
        data.Take(signature.Length).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(signature));

    private static bool ContainsSequence(IReadOnlyList<byte> data, IReadOnlyList<byte> sequence)
    {
        if (sequence.Count == 0 || sequence.Count > data.Count) return false;
        for (var offset = 0; offset <= data.Count - sequence.Count; offset++)
        {
            var matches = true;
            for (var index = 0; index < sequence.Count; index++)
            {
                if (data[offset + index] == sequence[index]) continue;
                matches = false;
                break;
            }
            if (matches) return true;
        }
        return false;
    }

    private static bool ContainsAscii(IReadOnlyList<byte> data, string text, int searchLength)
    {
        var expected = System.Text.Encoding.ASCII.GetBytes(text);
        var limit = Math.Min(data.Count, searchLength) - expected.Length;
        for (var offset = 0; offset <= limit; offset++)
            if (data.Skip(offset).Take(expected.Length).SequenceEqual(expected)) return true;
        return false;
    }

    private static bool ContainsAtasciiText(IReadOnlyList<byte> data, string text)
    {
        var expected = System.Text.Encoding.ASCII.GetBytes(text);
        for (var offset = 0; offset <= data.Count - expected.Length; offset++)
        {
            var matches = true;
            for (var index = 0; index < expected.Length; index++)
            {
                if ((data[offset + index] & 0x7f) == expected[index]) continue;
                matches = false;
                break;
            }
            if (matches) return true;
        }
        return false;
    }
}
