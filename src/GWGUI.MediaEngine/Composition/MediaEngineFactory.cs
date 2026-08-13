using GWGUI.MediaEngine.Containers.Acorn.BbcDfs;
using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Conversion.Amstrad;
using GWGUI.MediaEngine.Conversion.Epson;
using GWGUI.MediaEngine.Conversion.Dec;
using GWGUI.MediaEngine.Containers.Epson.Raw;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Containers.Atari.Atr;
using GWGUI.MediaEngine.Containers.Atari.Msa;
using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Containers.Coherent;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.Containers.Commodore;
using GWGUI.MediaEngine.Containers.Cp2;
using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.Containers.I86f;
using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.Containers.ImageDisk;
using GWGUI.MediaEngine.Containers.Msx.Raw;
using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Containers.TeleDisk;
using GWGUI.MediaEngine.Containers.Ucsd.Raw;
using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Conversion.Amiga;
using GWGUI.MediaEngine.Conversion.Ibm;
using GWGUI.MediaEngine.Conversion.Msx;
using GWGUI.MediaEngine.Conversion.Ucsd;
using GWGUI.MediaEngine.Conversion.Hfe;
using GWGUI.MediaEngine.Conversion.Flux;
using GWGUI.MediaEngine.Conversion.Scp;
using GWGUI.MediaEngine.Conversion.Acorn;
using GWGUI.MediaEngine.Conversion.Atari;
using GWGUI.MediaEngine.Conversion.Commodore;
using GWGUI.MediaEngine.Encoding.Apple;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Documents;
using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.Exploration.Interpretation.Normalizers;
using GWGUI.MediaEngine.Exploration.Interpretation.Policies;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;
using GWGUI.MediaEngine.Recognition.Msx;
using GWGUI.MediaEngine.Recognition.Scp;
using GWGUI.MediaEngine.Reconstruction.Amiga;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.Reconstruction.Atari;
using GWGUI.MediaEngine.Reconstruction.Commodore;
using GWGUI.MediaEngine.Reconstruction.Dec;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Scp;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Compose les services partagÃ©s constituant le moteur d'exploration des mÃ©dias.</summary>
public static class MediaEngineFactory
{
    /// <summary>Crée le service de conversion ADF Amiga avec ses Reader et Writer partagés.</summary>
    public static AmigaAdfConversionService CreateAmigaAdfConversionService()
    {
        var scpReader = CreateScpReader();
        return new(new AmigaScpSectorImageReader(scpReader, CreateFluxDecoders()), new Containers.Adf.AdfReader(), new Containers.Adf.AmigaAdfWriter());
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
        return new(new IsoScpSectorImageReader(scpReader, CreateFluxDecoders()), new Containers.Adf.AdfReader(), new Containers.Adf.AcornAdfWriter());
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
    public static HfeConversionService CreateHfeConversionService() => new(CreateDefaultExplorer(), new SectorImageTrackEncoder(), new Containers.Hfe.HfeWriter());

    /// <summary>Crée le service de conversion directe entre conteneurs de flux.</summary>
    public static FluxContainerConversionService CreateFluxContainerConversionService() => new(
        CreateScpReader(),
        new ScpWriter(),
        new Containers.Hfe.HfeReader(),
        new Containers.Hfe.HfeWriter());
    /// <summary>Crée le service commun de reconstruction SCP depuis les images sectorielles.</summary>
    public static SectorImageScpConversionService CreateSectorImageScpConversionService() => new(new SectorImageTrackEncoder(), new ScpEncodedTrackFluxService(), new ScpWriter());

    /// <summary>Crée le service strict de réinterprétation entre formats FAT12 compatibles.</summary>
    public static Conversion.Fat12.Fat12ReinterpretationService CreateFat12ReinterpretationService()
    {
        var linear = new Containers.Raw.LinearSectorImageWriter();
        var writer = new Conversion.Fat12.Fat12TargetImageWriter(new Containers.Atari.St.AtariStWriter(linear), new Containers.Ibm.Raw.IbmRawImageWriter(linear), new Containers.Msx.Raw.MsxRawImageWriter(linear));
        return new(CreateDefaultExplorer(), writer);
    }

    /// <summary>Crée le service de migration de fichiers entre FAT12 et AmigaDOS.</summary>
    public static Migration.Fat12AmigaDosMigrationService CreateFat12AmigaDosMigrationService()
    {
        var linear = new Containers.Raw.LinearSectorImageWriter();
        var fatWriter = new Conversion.Fat12.Fat12TargetImageWriter(new Containers.Atari.St.AtariStWriter(linear), new Containers.Ibm.Raw.IbmRawImageWriter(linear), new Containers.Msx.Raw.MsxRawImageWriter(linear));
        return new(new Containers.Adf.AmigaAdfWriter(), fatWriter);
    }

    /// <summary>Crée le service de migration vers Apple DOS, ProDOS et SOS.</summary>
    public static Migration.AppleFileSystemMigrationService CreateAppleFileSystemMigrationService() => new(new Containers.Apple.Raw.AppleRawImageWriter(), new Containers.Apple.TwoImg.TwoImgWriter(), new Containers.Apple.AppleDiskImageWriter());

    /// <summary>Crée le service de migration vers les volumes Commodore DOS D64, D71 et D81.</summary>
    public static Migration.CommodoreDosMigrationService CreateCommodoreDosMigrationService() => new(new Containers.Commodore.CommodoreDosContainerWriter(), new Containers.Commodore.D81.D81Writer(new Containers.Raw.LinearSectorImageWriter()));

    /// <summary>Crée le service unifié de migration entre systèmes de fichiers.</summary>
    public static Migration.FileSystemMigrationService CreateFileSystemMigrationService() => new(CreateFat12AmigaDosMigrationService(), CreateAppleFileSystemMigrationService(), CreateCommodoreDosMigrationService());

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
        var recognition = CreateRecognition(decoders, scpExploration, fileSystems);
        return new(recognition, fileSystems, scpExploration, interpretations, documents);
    }

    /// <summary>CrÃ©e l'unique lecteur de conteneur SCP partagÃ© par les reconstructeurs.</summary>
    private static ScpReader CreateScpReader() => new();

    /// <summary>CrÃ©e l'unique registre des dÃ©codeurs de flux partagÃ© par les reconstructeurs.</summary>
    private static FluxDecoderRegistry CreateFluxDecoders() => new(FluxDecoderCatalog.CreateDefault());

    /// <summary>CrÃ©e l'unique registre des lecteurs de systÃ¨mes de fichiers.</summary>
    private static FileSystemRegistry CreateFileSystems() => new(FileSystemReaderCatalog.CreateDefault());

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
        var isoAutomatic = new ScpSectorImageCandidate(ScpCandidateIds.IsoAutomatic, ScpFormatFamily.Iso, (path, _, token) => isoReader.ReadAsync(path, null, token));
        var isoSelected = new ScpSectorImageCandidate(ScpCandidateIds.IsoSelected, ScpFormatFamily.Iso, (path, format, token) => isoReader.ReadAsync(path, format, token));
        var amiga = new ScpSectorImageCandidate(ScpCandidateIds.Amiga, ScpFormatFamily.Amiga, (path, _, token) => amigaReader.ReadAsync(path, token));
        var atari = new ScpSectorImageCandidate(ScpCandidateIds.Atari, ScpFormatFamily.Iso, (path, format, token) => atariReader.ReadAsync(path, format, token));
        var atariSt720 = new ScpSectorImageCandidate(
            ScpCandidateIds.AtariSt720,
            ScpFormatFamily.Iso,
            (path, _, token) => atariReader.ReadAsync(path, DiskImageFormatIds.AtariSt720, token));
        var commodoreAutomatic = new ScpSectorImageCandidate(ScpCandidateIds.CommodoreAutomatic, ScpFormatFamily.Commodore, (path, _, token) => commodoreReader.ReadAsync(path, null, token));
        var commodore1581 = new ScpSectorImageCandidate(ScpCandidateIds.Commodore1581, ScpFormatFamily.Iso, (path, _, token) => commodoreReader.ReadAsync(path, DiskImageFormatIds.Commodore1581, token));
        var apple = new ScpSectorImageCandidate(ScpCandidateIds.Apple, ScpFormatFamily.Apple, (path, format, token) => appleReader.ReadAsync(path, format, token));
        var dec = new ScpSectorImageCandidate(ScpCandidateIds.Dec, ScpFormatFamily.Dec, (path, _, token) => decReader.ReadAsync(path, token));
        ScpSectorImageCandidate Iso(string format) => new(ScpCandidateIds.IsoFormat(format), ScpFormatFamily.Iso, (path, _, token) => isoReader.ReadAsync(path, format, token));
        var acornAdfs = Iso(DiskImageFormatIds.AcornAdfs800);
        var amstradCpc = Iso(DiskImageFormatIds.AmstradCpc);
        var amstradPcw = Iso(DiskImageFormatIds.AmstradPcw);
        var ibmScan = Iso(DiskImageFormatIds.IbmScan);
        var ucsd = Iso(DiskImageFormatIds.UcsdIbmMfm);
        var epson = EpsonQx10GeometryCatalog.ScpCandidateFormatIds.Select(Iso).ToArray();
        var isoFamily = new[]
            {
                isoAutomatic,
                atariSt720,
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
        return new(selections, defaults, families, [ScpFormatFamily.Iso, ScpFormatFamily.Amiga, ScpFormatFamily.Commodore, ScpFormatFamily.Apple, ScpFormatFamily.Dec], isoSelected);
    }

    /// <summary>CrÃ©e la dÃ©tection de famille et les deux parcours d'exploration SCP avec leurs instances partagÃ©es.</summary>
    private static ScpImageExplorationService CreateScpExploration(ScpReader scpReader, FluxDecoderRegistry decoders, ScpCandidateRegistry candidates, FileSystemRegistry fileSystems, DiskImageInterpretationService interpretations, DiskImageDocumentFactory documents)
    {
        var automatic = new ScpAutomaticImageExplorer(candidates, new ScpFamilyProbe(scpReader, decoders), new ScpCandidateInspector(fileSystems, interpretations), documents);
        return new(automatic, new ScpSectorImageReader(candidates, fileSystems));
    }

    /// <summary>CrÃ©e le registre des politiques de reconnaissance dans l'ordre historique conservÃ©.</summary>
    private static DiskImageRecognitionRegistry CreateRecognition(FluxDecoderRegistry decoders, ScpImageExplorationService scpExploration, FileSystemRegistry fileSystems)
    {
        var appleReader = new AppleDiskImageReader();
        return new(
        [
            new ExtensionHintRecognitionPolicy(new Containers.Adf.AdfReader().ReadAsync, DiskImageFileExtensions.Adf),
            new ExtensionHintRecognitionPolicy(new BbcDfsReader().ReadAsync, DiskImageFileExtensions.Ssd, DiskImageFileExtensions.Dsd),
            new CoherentImageRecognitionPolicy(new CoherentRawImageReader()),
            new DecRx02ImageRecognitionPolicy(new DecRx02Reader()),
            new ExtensionHintRecognitionPolicy(new AtariStReader().ReadAsync, DiskImageFileExtensions.St),
            new ExtensionHintRecognitionPolicy(new MsaReader().ReadAsync, DiskImageFileExtensions.Msa),
            new ExtensionHintRecognitionPolicy(new AtrReader().ReadAsync, DiskImageFileExtensions.Atr),
            new ExtensionHintRecognitionPolicy(new D64Reader().ReadAsync, DiskImageFileExtensions.D64),
            new ExtensionHintRecognitionPolicy(new D71Reader().ReadAsync, DiskImageFileExtensions.D71),
            new ExtensionHintRecognitionPolicy(new D81Reader().ReadAsync, DiskImageFileExtensions.D81),
            new AppleImageRecognitionPolicy(appleReader),
            new MsxImageRecognitionPolicy(new MsxRawImageReader()),
            new AmstradImageRecognitionPolicy(new CpcDskReader()),
            new RawImgRecognitionPolicy(new RawImgReader()),
            new ExtensionHintRecognitionPolicy(new IbmRawImageReader().ReadAsync, DiskImageFileExtensions.Ima),
            new ExtensionHintRecognitionPolicy(new Td0Reader().ReadAsync, DiskImageFileExtensions.Td0),
            new ExtensionHintRecognitionPolicy(new I86fSectorImageReader(new I86fReader(), decoders).ReadAsync, DiskImageFileExtensions.I86f),
            new ExtensionHintRecognitionPolicy(new Cp2Reader().ReadAsync, DiskImageFileExtensions.Cp2),
            new ExtensionHintRecognitionPolicy(new ImdReader().ReadAsync, DiskImageFileExtensions.Imd),
            new ScpRecognitionPolicy(scpExploration, fileSystems.SupportedFormatIds)
        ]);
    }
}
