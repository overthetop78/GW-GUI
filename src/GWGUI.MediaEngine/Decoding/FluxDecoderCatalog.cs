namespace GWGUI.MediaEngine.Decoding;

/// <summary>Construit le catalogue ordonné des décodeurs de flux fournis par défaut.</summary>
internal static class FluxDecoderCatalog
{
    /// <summary>Crée une nouvelle collection de décodeurs dans leur ordre stable de sélection.</summary>
    /// <returns>Décodeurs par défaut.</returns>
    public static IReadOnlyList<IFluxDecoder> CreateDefault() => [new IsoMfmDecoder(), new IsoFmDecoder(), new AmigaMfmDecoder(), new AppleIIGcrDecoder(), new AppleRwts18Decoder(), new AppleMacGcrDecoder(), new AppleLisaFileWareGcrDecoder(), new CommodoreGcrDecoder(), new HpMmfmDecoder(), new DataGeneralFmDecoder(), new MicropolisMfmDecoder(), new MembrainMfmDecoder(), new Aed6200pMfmDecoder(), new QdMo5MfmDecoder(), new CenturionMfmDecoder(), new NorthstarMfmDecoder(), new HeathkitFmDecoder(), new MicralNFmDecoder(), new EmuFmDecoder(), new TycomFmDecoder(), new DecRx02Decoder(), new ArburgDecoder(), new Victor9kGcrDecoder(), new Commodore900GcrDecoder(), new RawFluxDecoder()];
}
