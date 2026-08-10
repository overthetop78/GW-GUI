namespace GWGUI.MediaEngine.Encoding;

public sealed class FluxEncoderRegistry
{
    public IReadOnlyList<ITrackEncoder> Encoders { get; } =
    [
        new IsoMfmTrackEncoder(), new IsoFmTrackEncoder(), new AmigaMfmTrackEncoder(),
        new AppleIIGcrTrackEncoder(), new AppleRwts18TrackEncoder(), new AppleMacGcrTrackEncoder(), new AppleLisaFileWareGcrTrackEncoder(), new CommodoreGcrTrackEncoder(),
        new HpMmfmTrackEncoder(), new DataGeneralFmTrackEncoder(), new MicropolisMfmTrackEncoder(),
        new MembrainMfmTrackEncoder(), new Aed6200pMfmTrackEncoder(), new QdMo5MfmTrackEncoder(),
        new CenturionMfmTrackEncoder(), new NorthstarMfmTrackEncoder(), new HeathkitFmTrackEncoder(),
        new MicralNFmTrackEncoder(), new EmuFmTrackEncoder(), new TycomFmTrackEncoder(),
        new DecRx02TrackEncoder(), new ArburgTrackEncoder(), new Victor9kGcrTrackEncoder(),
        new Commodore900GcrTrackEncoder()
    ];

    public ITrackEncoder Get(string id) => Encoders.First(encoder => encoder.Id == id);
    public EncodedTrack Encode(string id, TrackEncodeRequest request) => Get(id).Encode(request);
}
