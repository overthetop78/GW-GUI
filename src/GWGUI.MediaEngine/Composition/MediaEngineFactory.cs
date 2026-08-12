using GWGUI.MediaEngine.Containers.Acorn.BbcDfs;
using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Atari.Atr;
using GWGUI.MediaEngine.Containers.Atari.Msa;
using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Containers.Coherent;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.Containers.Cp2;
using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.Containers.I86f;
using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.Containers.ImageDisk;
using GWGUI.MediaEngine.Containers.Msx.Raw;
using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Containers.TeleDisk;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Images.ScpDetection;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;
using GWGUI.MediaEngine.Reconstruction.Amiga;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.Reconstruction.Atari;
using GWGUI.MediaEngine.Reconstruction.Commodore;
using GWGUI.MediaEngine.Reconstruction.Dec;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Compose les services partagés constituant le moteur d'exploration des médias.</summary>
internal static class MediaEngineFactory
{
    /// <summary>Crée un explorateur complet avec les registres et services par défaut.</summary>
    public static DiskImageExplorer CreateDefaultExplorer()
    {
        var scpReader = CreateScpReader();
        var decoders = CreateFluxDecoders();
        var fileSystems = CreateFileSystems();
        var interpretations = CreateInterpretations(fileSystems);
        var candidates = CreateScpCandidates(scpReader, decoders);
        var scpExploration = CreateScpExploration(scpReader, decoders, candidates, fileSystems, interpretations);
        var recognition = CreateRecognition(decoders, scpExploration, fileSystems);
        return new(recognition, fileSystems, scpExploration, interpretations);
    }

    /// <summary>Crée l'unique lecteur de conteneur SCP partagé par les reconstructeurs.</summary>
    private static ScpReader CreateScpReader() => new();

    /// <summary>Crée l'unique registre des décodeurs de flux partagé par les reconstructeurs.</summary>
    private static FluxDecoderRegistry CreateFluxDecoders() => new(FluxDecoderCatalog.CreateDefault());

    /// <summary>Crée l'unique registre des lecteurs de systèmes de fichiers.</summary>
    private static FileSystemRegistry CreateFileSystems() => new(FileSystemReaderCatalog.CreateDefault());

    /// <summary>Crée le service d'interprétation partagé par les explorateurs général et SCP.</summary>
    private static DiskImageInterpretationService CreateInterpretations(FileSystemRegistry fileSystems) => new(fileSystems);

    /// <summary>Crée les reconstructeurs SCP dans leur ordre explicite et les réunit dans leur registre.</summary>
    private static ScpCandidateRegistry CreateScpCandidates(ScpReader scpReader, FluxDecoderRegistry decoders)
    {
        var isoReader = new IsoScpSectorImageReader(scpReader, decoders);
        return new(new AmigaScpSectorImageReader(scpReader, decoders), isoReader, new AtariScpSectorImageReader(scpReader, decoders), new CommodoreScpSectorImageReader(scpReader, decoders), new AppleScpSectorImageReader(scpReader, decoders), new DecRx02ScpSectorImageReader(scpReader, decoders));
    }

    /// <summary>Crée la détection de famille et les deux parcours d'exploration SCP avec leurs instances partagées.</summary>
    private static ScpImageExplorationService CreateScpExploration(ScpReader scpReader, FluxDecoderRegistry decoders, ScpCandidateRegistry candidates, FileSystemRegistry fileSystems, DiskImageInterpretationService interpretations) => new(candidates, new ScpFamilyProbe(scpReader, decoders), fileSystems, interpretations);

    /// <summary>Crée le registre des politiques de reconnaissance dans l'ordre historique conservé.</summary>
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
