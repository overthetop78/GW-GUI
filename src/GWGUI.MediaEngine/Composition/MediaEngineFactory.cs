using GWGUI.MediaEngine.Images.Conversion.Apple;
using GWGUI.MediaEngine.Images.Conversion.Flux;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Conversion;
using GWGUI.MediaEngine.Images.Conversion.Acorn;
using GWGUI.MediaEngine.Images.Conversion.Atari;
using GWGUI.MediaEngine.Images.Conversion.Commodore;
using GWGUI.MediaEngine.Images.Formats.Floppy.Apple.Encoding;
using GWGUI.MediaEngine.Images.Writing.Encoding;
using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Documents;
using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.Exploration.Interpretation.Normalizers;
using GWGUI.MediaEngine.Exploration.Interpretation.Policies;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.Images.Reading.Metadata;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Recognition.Policies;
using GWGUI.MediaEngine.Images.Reading.Recognition.Msx;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Recognition;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Apple;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Atari;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Iso;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Images.Reading.Decoding.I86f;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;
using GWGUI.MediaEngine.Images.Formats.Floppy.Apple;
using GWGUI.MediaEngine.Images.Formats.Floppy.AcornAtom;
using GWGUI.MediaEngine.Images.Formats.Floppy.Apridisk;
using GWGUI.MediaEngine.Images.Formats.Floppy.Atr;
using GWGUI.MediaEngine.Images.Formats.Floppy.Atx;
using GWGUI.MediaEngine.Images.Formats.Floppy.BbcDfs;
using GWGUI.MediaEngine.Images.Formats.Floppy.CommodoreDos;
using GWGUI.MediaEngine.Images.Formats.Floppy.Cp2;

using GWGUI.MediaEngine.Images.Formats.Floppy.CpcDsk;

using GWGUI.MediaEngine.Images.Formats.Floppy.D64;

using GWGUI.MediaEngine.Images.Formats.Floppy.D71;

using GWGUI.MediaEngine.Images.Formats.Floppy.D81;

using GWGUI.MediaEngine.Images.Formats.Floppy.DiskCopy;

using GWGUI.MediaEngine.Images.Formats.Floppy.I86f;

using GWGUI.MediaEngine.Images.Formats.Floppy.ImageDisk;

using GWGUI.MediaEngine.Images.Formats.Floppy.Msa;

using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

using GWGUI.MediaEngine.Images.Formats.Floppy.Rx02;

using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

using GWGUI.MediaEngine.Images.Formats.Floppy.St;

using GWGUI.MediaEngine.Images.Formats.Floppy.TeleDisk;

using GWGUI.MediaEngine.Images.Formats.Floppy.TwoImg;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading;

using GWGUI.MediaEngine.Images.Reading.Reconstruction;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Compose les services partagÃ©s constituant le moteur d'exploration des mÃ©dias.</summary>
public static class MediaEngineFactory
{
    /// <summary>Crée le service de conversion ADF Amiga avec ses Reader et Writer partagés.</summary>
    public static AmigaAdfConversionService CreateAmigaAdfConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new AmigaScpSectorImageReader(scpReader, CreateFluxDecoders()), new GWGUI.MediaEngine.Images.Formats.Floppy.Adf.AdfReader(), new GWGUI.MediaEngine.Images.Formats.Floppy.Adf.AmigaAdfWriter());
    }
    /// <summary>Crée le service de conversion IBM brute avec ses Reader et Writer partagés.</summary>
    public static IbmRawConversionService CreateIbmRawConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new IsoScpSectorImageReader(scpReader, CreateFluxDecoders()), new IbmRawImageReader(), new IbmRawImageWriter());
    }
    /// <summary>Crée le service de conversion MSX brute avec ses Reader et Writer partagés.</summary>
    public static MsxRawConversionService CreateMsxRawConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new IsoScpSectorImageReader(scpReader, CreateFluxDecoders()), new MsxRawImageReader(), new MsxRawImageWriter());
    }
    /// <summary>Crée le service de conversion ADF Acorn avec ses Reader et Writer partagés.</summary>
    public static AcornAdfConversionService CreateAcornAdfConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new IsoScpSectorImageReader(scpReader, CreateFluxDecoders()), new GWGUI.MediaEngine.Images.Formats.Floppy.Adf.AdfReader(), new GWGUI.MediaEngine.Images.Formats.Floppy.Adf.AcornAdfWriter());
    }
    /// <summary>Crée le service de conversion BBC DFS avec ses Reader et Writer partagés.</summary>
    public static BbcDfsConversionService CreateBbcDfsConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new IsoScpSectorImageReader(scpReader, CreateFluxDecoders()), new BbcDfsReader(), new BbcDfsImageWriter());
    }
    /// <summary>Crée le service de conversion NIB et WOZ avec ses Readers et Writers partagés.</summary>
    public static AppleNibbleConversionService CreateAppleNibbleConversionService()
    {
        var scpReader = CreateScpReader();
        var decoders = CreateFluxDecoders();
        return new(new AppleDiskImageReader(), new AppleScpSectorImageReader(scpReader, decoders), new AppleDiskImageWriter(new AppleRwts18TrackEncodingService(), new AppleIITrackEncodingService()));
    }
    /// <summary>Crée le service de conversion sectorielle Apple avec ses Readers et Writers partagés.</summary>
    public static AppleSectorConversionService CreateAppleSectorConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new AppleDiskImageReader(), new AppleScpSectorImageReader(scpReader, CreateFluxDecoders()), new AppleRawImageWriter(), new TwoImgWriter());
    }

    /// <summary>Crée le service Macintosh brut et DiskCopy avec ses Readers et Writers partagés.</summary>
    public static MacintoshConversionService CreateMacintoshConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new AppleDiskImageReader(), new AppleScpSectorImageReader(scpReader, CreateFluxDecoders()), new MacintoshRawImageWriter(), new DiskCopyWriter());
    }
    /// <summary>Crée le service Lisa DiskCopy avec ses Readers et son Writer partagés.</summary>
    public static LisaConversionService CreateLisaConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new AppleDiskImageReader(), new AppleScpSectorImageReader(scpReader, CreateFluxDecoders()), new DiskCopyWriter());
    }
    /// <summary>Crée le service HFE sectoriel avec l'explorateur et l'encodeur de pistes communs.</summary>
    public static HfeConversionService CreateHfeConversionService() => new(CreateDefaultExplorer(), new SectorImageTrackEncoder(), new GWGUI.MediaEngine.Images.Formats.Floppy.Hfe.HfeWriter());

    /// <summary>Crée le service de conversion directe entre conteneurs de flux.</summary>
    public static FluxContainerConversionService CreateFluxContainerConversionService() => new(
        CreateScpReader(),
        new ScpWriter(),
        new GWGUI.MediaEngine.Images.Formats.Floppy.Hfe.HfeReader(),
        new GWGUI.MediaEngine.Images.Formats.Floppy.Hfe.HfeWriter());
    /// <summary>Crée le service commun de reconstruction SCP depuis les images sectorielles.</summary>
    public static SectorImageScpConversionService CreateSectorImageScpConversionService() => new(new SectorImageTrackEncoder(), new ScpEncodedTrackFluxService(), new ScpWriter());

    /// <summary>Crée le service strict de réinterprétation entre formats FAT12 compatibles.</summary>
    public static GWGUI.MediaEngine.Images.Conversion.Fat12.Fat12ReinterpretationService CreateFat12ReinterpretationService()
    {
        var linear = new GWGUI.MediaEngine.Images.Formats.Floppy.Raw.LinearSectorImageWriter();
        var writer = new GWGUI.MediaEngine.Images.Conversion.Fat12.Fat12TargetImageWriter(new GWGUI.MediaEngine.Images.Formats.Floppy.St.AtariStWriter(linear), new GWGUI.MediaEngine.Images.Formats.Floppy.Raw.IbmRawImageWriter(linear), new GWGUI.MediaEngine.Images.Formats.Floppy.Raw.MsxRawImageWriter(linear));
        return new(CreateDefaultExplorer(), writer);
    }

    /// <summary>Crée l'entrée média pour l'export des fichiers vers une image cible.</summary>
    public static Images.Creation.FileSystemMigrationService CreateFileSystemMigrationService() => new();

    /// <summary>Crée le service reconnaissant une image sectorielle avant de la reconstruire en SCP.</summary>
    public static SectorImageScpFileConversionService CreateSectorImageScpFileConversionService()
    {
        var scpReader = CreateScpReader();
        var decoders = CreateFluxDecoders();
        var fileSystems = CreateFileSystems();
        var (interpretations, documents) = CreateInterpretations(fileSystems);
        var candidates = CreateScpCandidates(scpReader, decoders);
        var scpExploration = CreateScpExploration(scpReader, decoders, candidates, fileSystems, interpretations, documents);
        return new(CreateRecognition(decoders, scpExploration, fileSystems), CreateSectorImageScpConversionService());
    }
    /// <summary>Crée le service de conversion sectorielle Atari ST avec ses Reader et Writer partagés.</summary>
    public static AtariStConversionService CreateAtariStConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new AtariScpSectorImageReader(scpReader, CreateFluxDecoders()), new AtariStReader(), new MsaReader(), new AtariStWriter(new LinearSectorImageWriter()), new MsaWriter());
    }
    /// <summary>Crée le service de conversion ATR avec ses Reader et Writer partagés.</summary>
    public static AtrConversionService CreateAtrConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new AtariScpSectorImageReader(scpReader, CreateFluxDecoders()), new AtrReader(), new AtrWriter());
    }
    /// <summary>Crée le convertisseur temporaire SCP vers ATR ou ST utilisé par l'émulation Atari.</summary>
    public static AtariScpRuntimeConversionService CreateAtariScpRuntimeConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new AtariScpSectorImageReader(scpReader, CreateFluxDecoders()), new AtrWriter(),
            new AtariStWriter(new LinearSectorImageWriter()));
    }
    /// <summary>Crée le service de conversion Commodore 1581 avec ses Reader et Writer partagés.</summary>
    public static D81ConversionService CreateD81ConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new CommodoreScpSectorImageReader(scpReader, CreateFluxDecoders()), new D81Reader(), new D81Writer(new LinearSectorImageWriter()));
    }
    /// <summary>Crée le service de conversion D64/D71 avec son Writer zoné commun.</summary>
    public static CommodoreDosConversionService CreateCommodoreDosConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new CommodoreScpSectorImageReader(scpReader, CreateFluxDecoders()), new D64Reader(), new D71Reader(), new CommodoreDosContainerWriter());
    }

    /// <summary>Crée le service de conversion Commodore 900 COHERENT avec son ordre zoné commun.</summary>
    public static CoherentConversionService CreateCoherentConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new CommodoreScpSectorImageReader(scpReader, CreateFluxDecoders()), new CoherentRawImageReader(), new CoherentRawImageWriter());
    }

    /// <summary>Crée le service de conversion CPCEMU DSK/EDSK avec son modèle de conteneur partagé.</summary>
    public static AmstradDskConversionService CreateAmstradDskConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new IsoScpSectorImageReader(scpReader, CreateFluxDecoders()), new CpcDskReader(), new CpcDskWriter());
    }

    /// <summary>Crée le service de conversion Epson IMG/IMD avec ses modèles de géométrie partagés.</summary>
    public static EpsonQx10ConversionService CreateEpsonQx10ConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new IsoScpSectorImageReader(scpReader, CreateFluxDecoders()), new EpsonQx10RawImageReader(), new EpsonQx10RawImageWriter(), new ImdReader(), new ImdWriter());
    }

    /// <summary>Crée le service de conversion DEC RX02 avec son ordre physique partagé.</summary>
    public static DecRx02ConversionService CreateDecRx02ConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new DecRx02ScpSectorImageReader(scpReader, CreateFluxDecoders()), new DecRx02Reader(), new DecRx02Writer());
    }

    /// <summary>Crée le service de conversion UCSD IMG avec sa géométrie sectorielle explicite.</summary>
    public static UcsdImgConversionService CreateUcsdImgConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new IsoScpSectorImageReader(scpReader, CreateFluxDecoders()), new UcsdRawImageReader(), new Td0Reader(), new LinearSectorImageWriter(), new Td0Writer());
    }
    /// <summary>CrÃ©e un explorateur complet avec les registres et services par dÃ©faut.</summary>
    public static DiskImageExplorer CreateDefaultExplorer()
    {
        var scpReader = CreateScpReader();
        var decoders = CreateFluxDecoders();
        var fileSystems = CreateFileSystems();
        var (interpretations, documents) = CreateInterpretations(fileSystems);
        var candidates = CreateScpCandidates(scpReader, decoders);
        var scpExploration = CreateScpExploration(scpReader, decoders, candidates, fileSystems, interpretations, documents);
        var readingService = CreateMediaImageReadingService();
        return new(readingService, fileSystems, scpExploration, interpretations, documents);
    }

    /// <summary>CrÃ©e l'unique lecteur de conteneur SCP partagÃ© par les reconstructeurs.</summary>
    private static ScpReader CreateScpReader() => new();

    /// <summary>CrÃ©e l'unique registre des dÃ©codeurs de flux partagÃ© par les reconstructeurs.</summary>
    private static FluxDecoderRegistry CreateFluxDecoders() => new(FluxDecoderCatalog.CreateDefault());

    /// <summary>CrÃ©e l'unique registre des lecteurs de systÃ¨mes de fichiers.</summary>
    private static FileSystemRegistry CreateFileSystems() => new(MediaFileSystemsReaderAdapter.CreateDefaultCatalog());

    /// <summary>CrÃ©e le service d'interprÃ©tation partagÃ© par les explorateurs gÃ©nÃ©ral et SCP.</summary>
    private static (DiskImageInterpretationService Interpretations, DiskImageDocumentFactory Documents) CreateInterpretations(FileSystemRegistry fileSystems)
    {
        var msxInterpreter = new MsxSectorImageInterpreter();
        IRecognizedImageNormalizer[] normalizerPolicies = [new MacRecognizedImageNormalizer(), new MsxRecognizedImageNormalizer(msxInterpreter), new AtariRecognizedImageNormalizer()];
        IAdditionalImageInterpretationPolicy[] additionalPolicies = [new IbmAdditionalImageInterpretationPolicy(fileSystems.SupportedFormatIds), new MsxAdditionalImageInterpretationPolicy(msxInterpreter), new CompatibleFormatInterpretationPolicy()];
        var normalizers = new RecognizedImageNormalizerRegistry(normalizerPolicies);
        var additionalInterpretations = new AdditionalImageInterpretationRegistry(additionalPolicies);
        var metadata = new DiskImageMetadataFactory(new DiskSystemResolver(), new DiskProtectionResolver());
        return (new(normalizers, additionalInterpretations), new(metadata));
    }

    /// <summary>CrÃ©e les reconstructeurs SCP dans leur ordre explicite et les rÃ©unit dans leur registre.</summary>
    private static ScpCandidateRegistry CreateScpCandidates(ScpReader scpReader, FluxDecoderRegistry decoders)
    {
        var isoReader = new IsoScpSectorImageReader(scpReader, decoders);
        var amigaReader = new AmigaScpSectorImageReader(scpReader, decoders);
        var atariReader = new AtariScpSectorImageReader(scpReader, decoders);
        var commodoreReader = new CommodoreScpSectorImageReader(scpReader, decoders);
        var appleReader = new AppleScpSectorImageReader(scpReader, decoders);
        var decReader = new DecRx02ScpSectorImageReader(scpReader, decoders);
        ScpSectorImageCandidate IsoCandidate(string id, Func<string?, string?> selectFormat) => new(
            id,
            ScpFormatFamily.Iso,
            (path, format, token) => isoReader.ReadAsync(path, selectFormat(format), token),
            (path, format, progress, token) => isoReader.ReadAsync(path, selectFormat(format), progress, token));
        var isoAutomatic = IsoCandidate(ScpCandidateIds.IsoAutomatic, _ => null);
        var isoSelected = IsoCandidate(ScpCandidateIds.IsoSelected, format => format);
        var amiga = new ScpSectorImageCandidate(ScpCandidateIds.Amiga, ScpFormatFamily.Amiga, (path, _, token) => amigaReader.ReadAsync(path, token));
        var atari = new ScpSectorImageCandidate(ScpCandidateIds.Atari, ScpFormatFamily.Iso, (path, format, token) => atariReader.ReadAsync(path, format, token));
        var commodoreAutomatic = new ScpSectorImageCandidate(ScpCandidateIds.CommodoreAutomatic, ScpFormatFamily.Commodore, (path, _, token) => commodoreReader.ReadAsync(path, null, token));
        var commodore1581 = new ScpSectorImageCandidate(ScpCandidateIds.Commodore1581, ScpFormatFamily.Iso, (path, _, token) => commodoreReader.ReadAsync(path, DiskImageFormatIds.Commodore1581, token));
        var apple = new ScpSectorImageCandidate(ScpCandidateIds.Apple, ScpFormatFamily.Apple, (path, format, token) => appleReader.ReadAsync(path, format, token));
        var dec = new ScpSectorImageCandidate(ScpCandidateIds.Dec, ScpFormatFamily.Dec, (path, _, token) => decReader.ReadAsync(path, token));
        ScpSectorImageCandidate Iso(string format) => IsoCandidate(ScpCandidateIds.IsoFormat(format), _ => format);
        var acornAdfs = Iso(DiskImageFormatIds.AcornAdfs800);
        var amstradCpc = Iso(DiskImageFormatIds.AmstradCpc);
        var amstradPcw = Iso(DiskImageFormatIds.AmstradPcw);
        var ibmScan = Iso(DiskImageFormatIds.IbmScan);
        var ucsd = Iso(DiskImageFormatIds.UcsdIbmMfm);
        var epson = EpsonQx10GeometryCatalog.ScpCandidateFormatIds.Select(Iso).ToArray();
        var isoFamily = new[]
            {
                isoAutomatic,
                acornAdfs,
                amstradCpc,
                amstradPcw,
                ibmScan,
                ucsd,
                commodore1581
            }
            .Concat(epson)
            .ToArray();
        var defaults = new[] { isoAutomatic, amiga, commodore1581, commodoreAutomatic, amstradCpc, amstradPcw, ibmScan }.Concat(epson).Append(apple).ToArray();
        var selections = new[]
        {
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase), amiga),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.CommodorePrefix, StringComparison.OrdinalIgnoreCase), commodoreAutomatic),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AmstradPrefix, StringComparison.OrdinalIgnoreCase), isoSelected),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) || id.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase), isoSelected),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AcornAdfsPrefix, StringComparison.OrdinalIgnoreCase), isoSelected),
            new ScpFormatSelection(id => id.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase), dec),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase) || id.Equals(DiskImageFormatIds.UcsdIbmMfm, StringComparison.OrdinalIgnoreCase), isoSelected),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase), atari),
            new ScpFormatSelection(id => id.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) || id.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase), apple)
        };
        KeyValuePair<ScpFormatFamily, IReadOnlyList<ScpSectorImageCandidate>>[] families = [new(ScpFormatFamily.Iso, isoFamily), new(ScpFormatFamily.Amiga, [amiga]), new(ScpFormatFamily.Commodore, [commodoreAutomatic]), new(ScpFormatFamily.Apple, [apple]), new(ScpFormatFamily.Dec, [dec])];
        return new(selections, defaults, families, [ScpFormatFamily.Amiga, ScpFormatFamily.Iso, ScpFormatFamily.Commodore, ScpFormatFamily.Apple, ScpFormatFamily.Dec], isoSelected);
    }

    /// <summary>CrÃ©e la dÃ©tection de famille et les deux parcours d'exploration SCP avec leurs instances partagÃ©es.</summary>
    private static ScpImageExplorationService CreateScpExploration(ScpReader scpReader, FluxDecoderRegistry decoders, ScpCandidateRegistry candidates, FileSystemRegistry fileSystems, DiskImageInterpretationService interpretations, DiskImageDocumentFactory documents)
    {
        var automatic = new ScpAutomaticImageExplorer(scpReader, candidates, new ScpFamilyProbe(scpReader, decoders), new ScpCandidateInspector(fileSystems, interpretations), documents);
        return new(automatic, new ScpSectorImageReader(candidates, fileSystems));
    }

    /// <summary>Creates the temporary sector-only adapter over the common media reader chain.</summary>
    private static DiskImageRecognitionRegistry CreateRecognition(FluxDecoderRegistry decoders, ScpImageExplorationService scpExploration, FileSystemRegistry fileSystems)
    {
        _ = decoders;
        _ = scpExploration;
        _ = fileSystems;
        return new(CreateMediaImageReadingService());
    }

    /// <summary>Creates the common recognition and reading service used during the migration.</summary>
    private static MediaImageReadingService CreateMediaImageReadingService() => new(new MediaRecognitionRegistry(CreateMediaImageReaders()));

    /// <summary>Registers every existing media image reader in deterministic order.</summary>
    private static IReadOnlyList<IMediaImageReader> CreateMediaImageReaders() =>
    [
        new GWGUI.MediaEngine.Images.Formats.Floppy.Adf.AdfReader(),
        new AcornAtomDskReader(),
        new ApridiskReader(),
        new BbcDfsReader(),
        new CoherentRawImageReader(),
        new DecRx02Reader(),
        new AtariStReader(),
        new MsaReader(),
        new AtrReader(),
        new GWGUI.MediaEngine.Images.Formats.Floppy.Xfd.XfdReader(),
        new AtxReader(),
        new D64Reader(),
        new D71Reader(),
        new D81Reader(),
        new AppleDiskImageReader(),
        new MsxRawImageReader(),
        new CpcDskReader(),
        new RawImgReader(),
        new IbmRawImageReader(),
        new Td0Reader(),
        new I86fReader(),
        new Cp2Reader(),
        new ImdReader(),
        new EpsonQx10RawImageReader(),
        new UcsdRawImageReader(),
        new GWGUI.MediaEngine.Images.Formats.Floppy.Hfe.HfeReader(),
        new ScpReader()
    ];
}
