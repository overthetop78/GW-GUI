using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Containers.Atari.Atr;
using GWGUI.MediaEngine.Containers.Coherent;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.Containers;
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
            new DirectContainerPolicy(new AdfImageReader(), DiskImageFileExtensions.Adf),
            new DirectContainerPolicy(new BbcDfsImageReader(), DiskImageFileExtensions.Ssd, DiskImageFileExtensions.Dsd),
            new CoherentImageRecognitionPolicy(new CoherentRawImageReader()),
            new DecRx02ImageRecognitionPolicy(new DecRx02ImageReader()),
            new DirectContainerPolicy(new AtariStImageReader(), DiskImageFileExtensions.St),
            new DirectContainerPolicy(new MsaImageReader(), DiskImageFileExtensions.Msa),
            new DirectContainerPolicy(new AtrReader(), DiskImageFileExtensions.Atr),
            new DirectContainerPolicy(new CommodoreD64ImageReader(), DiskImageFileExtensions.D64),
            new DirectContainerPolicy(new CommodoreD71ImageReader(), DiskImageFileExtensions.D71),
            new DirectContainerPolicy(new CommodoreD81ImageReader(), DiskImageFileExtensions.D81),
            new AppleImageRecognitionPolicy(apple),
            new MsxContainerPolicy(new MsxImageReader()),
            new AmstradImageRecognitionPolicy(new CpcDskReader()),
            new RawImgContainerPolicy(),
            new DirectContainerPolicy(new IbmPcImageReader(), DiskImageFileExtensions.Ima),
            new DirectContainerPolicy(new Td0ImageReader(), DiskImageFileExtensions.Td0),
            new DelegatingContainerPolicy(new I86fImageReader(decoders).ReadAsync, DiskImageFileExtensions.I86f),
            new DelegatingContainerPolicy(new Cp2ImageReader().ReadAsync, DiskImageFileExtensions.Cp2),
            new DirectContainerPolicy(new ImdImageReader(), DiskImageFileExtensions.Imd),
            new ScpContainerPolicy(scpExploration, fileSystems.SupportedFormatIds)
        ]);
        return new(containers, fileSystems, scpExploration);
    }
}
