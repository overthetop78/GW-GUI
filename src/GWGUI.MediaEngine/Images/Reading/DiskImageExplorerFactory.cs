using GWGUI.MediaEngine.Constants;
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
using GWGUI.MediaEngine.Images.Formats.Floppy.I86f;
using GWGUI.MediaEngine.Images.Formats.Floppy.ImageDisk;
using GWGUI.MediaEngine.Images.Formats.Floppy.Msa;
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;
using GWGUI.MediaEngine.Images.Formats.Floppy.Rx02;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Recognition;
using GWGUI.MediaEngine.Images.Formats.Floppy.St;
using GWGUI.MediaEngine.Images.Formats.Floppy.TeleDisk;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Interfaces.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Images.Reading.Documents;
using GWGUI.MediaEngine.Images.Reading.Metadata;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Recognition.Msx;
using GWGUI.MediaEngine.Images.Reading.Recognition.Normalizers;
using GWGUI.MediaEngine.Images.Reading.Recognition.Policies;
using GWGUI.MediaEngine.Images.Reading.Reconstruction;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Apple;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Atari;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Iso;
using FileSystemRegistry = GWGUI.MediaFileSystems.Exploration.SectorFileSystemRegistry;

namespace GWGUI.MediaEngine.Images.Reading;

/// <summary>Assemble la lecture de l'image et les services auxquels l'explorateur délègue.</summary>
internal static class DiskImageExplorerFactory
{
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
    private static FileSystemRegistry CreateFileSystems() => new();

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
