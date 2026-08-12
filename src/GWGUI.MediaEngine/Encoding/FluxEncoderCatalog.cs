namespace GWGUI.MediaEngine.Encoding;

/// <summary>Construit le catalogue par défaut des encodeurs de pistes fournis par MediaEngine.</summary>
internal static class FluxEncoderCatalog
{
    /// <summary>Crée les encodeurs par défaut dans leur ordre public de déclaration.</summary>
    /// <returns>Nouvelle collection contenant chaque encodeur fourni une seule fois.</returns>
    public static IReadOnlyList<ITrackEncoder> CreateDefault() =>
    [
        new IsoMfmTrackEncoder(),
        new IsoFmTrackEncoder(),
        new AmigaMfmTrackEncoder(),
        new AppleIIGcrTrackEncoder(),
        new AppleRwts18TrackEncoder(),
        new AppleMacGcrTrackEncoder(),
        new AppleLisaFileWareGcrTrackEncoder(),
        new CommodoreGcrTrackEncoder(),
        new HpMmfmTrackEncoder(),
        new DataGeneralFmTrackEncoder(),
        new MicropolisMfmTrackEncoder(),
        new MembrainMfmTrackEncoder(),
        new Aed6200pMfmTrackEncoder(),
        new QdMo5MfmTrackEncoder(),
        new CenturionMfmTrackEncoder(),
        new NorthstarMfmTrackEncoder(),
        new HeathkitFmTrackEncoder(),
        new MicralNFmTrackEncoder(),
        new EmuFmTrackEncoder(),
        new TycomFmTrackEncoder(),
        new DecRx02TrackEncoder(),
        new ArburgTrackEncoder(),
        new Victor9kGcrTrackEncoder(),
        new Commodore900GcrTrackEncoder()
    ];
}
