using GWGUI.Scp.Containers.Scp;
using GWGUI.Scp.Containers.Amstrad.CpcDsk;
using GWGUI.Scp.Decoding;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.Images.Containers;
using GWGUI.Scp.Images.ScpDetection;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

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
        var scpExploration = new ScpImageExplorationService(
            candidates, new ScpFamilyProbe(scp, decoders), fileSystems);
        var apple = new AppleDiskImageReader();
        var containers = new DiskImageContainerRegistry(
        [
            new DirectContainerPolicy(new AdfImageReader(), ".adf"),
            new DirectContainerPolicy(new BbcDfsImageReader(), ".ssd", ".dsd"),
            new CoherentContainerPolicy(new CoherentImageReader()),
            new DecRx02ContainerPolicy(new DecRx02ImageReader()),
            new DirectContainerPolicy(new AtariStImageReader(), ".st"),
            new DirectContainerPolicy(new MsaImageReader(), ".msa"),
            new DirectContainerPolicy(new AtrImageReader(), ".atr"),
            new DirectContainerPolicy(new CommodoreD64ImageReader(), ".d64"),
            new DirectContainerPolicy(new CommodoreD71ImageReader(), ".d71"),
            new DirectContainerPolicy(new CommodoreD81ImageReader(), ".d81"),
            new AppleContainerPolicy(apple),
            new MsxContainerPolicy(new MsxImageReader()),
            new AmstradContainerPolicy(new CpcDskReader()),
            new RawImgContainerPolicy(),
            new DirectContainerPolicy(new IbmPcImageReader(), ".ima"),
            new DirectContainerPolicy(new Td0ImageReader(), ".td0"),
            new DelegatingContainerPolicy(new I86fImageReader(decoders).ReadAsync, ".86f"),
            new DelegatingContainerPolicy(new Cp2ImageReader().ReadAsync, ".cp2"),
            new DirectContainerPolicy(new ImdImageReader(), ".imd"),
            new ScpContainerPolicy(scpExploration, fileSystems.SupportedFormatIds)
        ]);
        return new(containers, fileSystems, scpExploration);
    }
}
