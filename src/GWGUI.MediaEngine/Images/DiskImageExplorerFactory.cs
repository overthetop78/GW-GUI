using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Containers.Atari.Atr;
using GWGUI.MediaEngine.Containers.Coherent;
using GWGUI.MediaEngine.Containers.Cp2;
using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.ScpDetection;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

internal static class DiskImageExplorerFactory
{
    public static DiskImageExplorer CreateDefault()
    {
        var scp = new ScpReader();
        var decoders = new FluxDecoderRegistry();
        var fileSystems = new FileSystemRegistry();
        var iso = new IsoScpSectorImageReader(scp, decoders);
        var candidates = new ScpCandidateRegistry(
            new AmigaScpSectorImageReader(scp, decoders), iso,
            new AtariScpSectorImageReader(scp, decoders), new AmstradScpSectorImageReader(scp, decoders),
            new BbcScpSectorImageReader(scp, decoders), new IbmPcScpSectorImageReader(scp, decoders),
            new EpsonQx10ScpSectorImageReader(scp, decoders), new UcsdScpSectorImageReader(scp, decoders),
            new CommodoreScpSectorImageReader(scp, decoders), new AppleScpSectorImageReader(scp, decoders),
            new DecRx02ScpSectorImageReader(scp, decoders));
        var scpExploration = new ScpImageExplorationService(candidates, new ScpFamilyProbe(scp, decoders), fileSystems);
        var apple = new AppleDiskImageReader();
        var containers = new DiskImageRecognitionRegistry(
        [
            new ExtensionHintRecognitionPolicy(new AdfImageReader().ReadAsync, DiskImageFileExtensions.Adf),
            new ExtensionHintRecognitionPolicy(new BbcDfsImageReader().ReadAsync, DiskImageFileExtensions.Ssd, DiskImageFileExtensions.Dsd),
            new CoherentImageRecognitionPolicy(new CoherentRawImageReader()),
            new DecRx02ImageRecognitionPolicy(new DecRx02Reader()),
            new ExtensionHintRecognitionPolicy(new AtariStImageReader().ReadAsync, DiskImageFileExtensions.St),
            new ExtensionHintRecognitionPolicy(new MsaImageReader().ReadAsync, DiskImageFileExtensions.Msa),
            new ExtensionHintRecognitionPolicy(new AtrReader().ReadAsync, DiskImageFileExtensions.Atr),
            new ExtensionHintRecognitionPolicy(new CommodoreD64ImageReader().ReadAsync, DiskImageFileExtensions.D64),
            new ExtensionHintRecognitionPolicy(new CommodoreD71ImageReader().ReadAsync, DiskImageFileExtensions.D71),
            new ExtensionHintRecognitionPolicy(new CommodoreD81ImageReader().ReadAsync, DiskImageFileExtensions.D81),
            new AppleImageRecognitionPolicy(apple),
            new MsxImageRecognitionPolicy(new MsxImageReader()),
            new AmstradImageRecognitionPolicy(new CpcDskReader()),
            new RawImgRecognitionPolicy(new RawImgReader()),
            new ExtensionHintRecognitionPolicy(new IbmPcImageReader().ReadAsync, DiskImageFileExtensions.Ima),
            new ExtensionHintRecognitionPolicy(new Td0ImageReader().ReadAsync, DiskImageFileExtensions.Td0),
            new ExtensionHintRecognitionPolicy(new I86fImageReader(decoders).ReadAsync, DiskImageFileExtensions.I86f),
            new ExtensionHintRecognitionPolicy(new Cp2Reader().ReadAsync, DiskImageFileExtensions.Cp2),
            new ExtensionHintRecognitionPolicy(new ImdImageReader().ReadAsync, DiskImageFileExtensions.Imd),
            new ScpRecognitionPolicy(scpExploration, fileSystems.SupportedFormatIds)
        ]);
        return new(containers, fileSystems, scpExploration);
    }
}
