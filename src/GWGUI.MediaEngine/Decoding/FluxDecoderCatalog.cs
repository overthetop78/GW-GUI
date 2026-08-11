namespace GWGUI.MediaEngine.Decoding;

internal static class FluxDecoderCatalog
{
    public static IReadOnlyList<IFluxDecoder> CreateDefault() => [new IsoMfmDecoder(), new IsoFmDecoder(), new AmigaMfmDecoder(), new AppleIIGcrDecoder(), new AppleRwts18Decoder(), new AppleMacGcrDecoder(), new AppleLisaFileWareGcrDecoder(), new CommodoreGcrDecoder(), new HpMmfmDecoder(), new DataGeneralFmDecoder(), new MicropolisMfmDecoder(), new MembrainMfmDecoder(), new Aed6200pMfmDecoder(), new QdMo5MfmDecoder(), new CenturionMfmDecoder(), new NorthstarMfmDecoder(), new HeathkitFmDecoder(), new MicralNFmDecoder(), new EmuFmDecoder(), new TycomFmDecoder(), new DecRx02Decoder(), new ArburgDecoder(), new Victor9kGcrDecoder(), new Commodore900GcrDecoder(), new RawFluxDecoder()];
}
