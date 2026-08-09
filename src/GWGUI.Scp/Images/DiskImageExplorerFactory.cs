using GWGUI.Scp.Decoding;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

internal static class DiskImageExplorerFactory
{
    public static DiskImageExplorer CreateDefault()
    {
        var scp = new ScpReader();
        var decoders = new FluxDecoderRegistry();
        var fileSystems = new FileSystemRegistry();
        var scpExploration = new ScpImageExplorationService(
            new AmigaScpSectorImageReader(scp, decoders), new IsoScpSectorImageReader(scp, decoders),
            new AtariScpSectorImageReader(scp, decoders), new AmstradScpSectorImageReader(scp, decoders),
            new BbcScpSectorImageReader(scp, decoders), new IbmPcScpSectorImageReader(scp, decoders),
            new EpsonQx10ScpSectorImageReader(scp, decoders), new UcsdScpSectorImageReader(scp, decoders),
            new CommodoreScpSectorImageReader(scp, decoders), new AppleScpSectorImageReader(scp, decoders),
            new DecRx02ScpSectorImageReader(scp, decoders), fileSystems, scp, decoders);
        return new(new AdfImageReader(), new AtariStImageReader(), new MsaImageReader(), new AtrImageReader(),
            new CommodoreD64ImageReader(), new CommodoreD71ImageReader(), new CommodoreD81ImageReader(),
            new AmstradDskImageReader(), new MsxImageReader(), new IbmPcImageReader(), new AppleDiskImageReader(),
            new BbcDfsImageReader(), new CoherentImageReader(), new DecRx02ImageReader(), new Td0ImageReader(),
            new I86fImageReader(decoders), new Cp2ImageReader(), new ImdImageReader(), fileSystems, scpExploration);
    }
}
