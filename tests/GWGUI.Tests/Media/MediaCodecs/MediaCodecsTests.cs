namespace GWGUI.Tests.Media.MediaCodecs;
public class MediaCodecsTests
{
    [Fact] public void RawCodecReportsTimingWithoutInventingSectors() => TrackCodecScenarios.Raw();
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public Task AppleScpReconstructionPreservesSectorOrderAndTags(int kind) => SectorReconstructionScenarios.Apple(kind);
    [Fact] public void AmigaHighDensityNeedsSeveralCredibleTracks() => SectorReconstructionScenarios.AmigaDensity();
    [Theory] [InlineData("amiga",false)] [InlineData("amiga-hd",false)] [InlineData("dec",false)] [InlineData("1541",false)]
    [InlineData("1571",false)] [InlineData("1581",false)] [InlineData("900",false)] [InlineData("amiga",true)] [InlineData("dec",true)] [InlineData("1541",true)]
    public Task ScpSpecializedReadersKeepBestDataAndMissingBlocks(string kind,bool empty) => SectorReconstructionScenarios.Scp(kind,empty);
    [Theory] [InlineData("synthetic",1,128)] [InlineData("synthetic",0,128)]
    [InlineData("amstrad.cpc.data",0xc1,512)] [InlineData("ibm.160",1,512)] [InlineData("atarist.720",1,512)]
    [InlineData("atari.90",1,128)] [InlineData("epson.qx10.320",1,256)]
    [InlineData(GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AcornDfsSingleSided,0,256)]
    [InlineData(GWGUI.MediaEngine.Definitions.DiskImageFormatIds.UcsdIbmMfm,1,512)]
    public void IsoPoliciesPreserveBestRevolutionAndMissingSectorPositions(string format,int first,int size) => SectorReconstructionScenarios.Iso(format,first,size);
    [Theory]
    [InlineData("applemac.gcr",512,false)] [InlineData("applelisa.fileware.gcr",512,false)]
    [InlineData("hp.mmfm",256,false)] [InlineData("datageneral.fm",512,false)] [InlineData("micropolis.mfm",256,false)]
    [InlineData("membrain.mfm",512,false)] [InlineData("aed6200p.mfm",129,false)] [InlineData("qdmo5.mfm",128,false)]
    [InlineData("centurion.mfm",257,false)] [InlineData("northstar.mfm",512,false)] [InlineData("heathkit.fm",256,false)]
    [InlineData("micraln.fm",128,false)] [InlineData("emu.fm",3584,false)] [InlineData("tycom.fm",128,false)]
    [InlineData("dec.rx02",128,false)] [InlineData("dec.rx02",256,false)] [InlineData("arburg",2558,false)] [InlineData("arburg",3838,true)]
    [InlineData("victor9k.gcr",512,false)] [InlineData("commodore900.gcr",512,false)]
    public void SpecializedRegistryCodecsDecodeDataAndRejectCorruption(string id,int length,bool system) => TrackCodecScenarios.Specialized(id,length,system);
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] public void RealTrackCodecsPreserveSyntheticSectors(int kind)=>TrackCodecScenarios.EncodeAndDecode(kind);
    [Fact] public void SectorSelectionPrefersValidIntegrityAndStableTies()=>SectorReconstructionScenarios.BestCandidate();
    [Fact] public void CrcMatchesKnownVectorAndDetectsChangedByte()=>BitAndChecksumScenarios.Crc();
    [Theory]
    [InlineData(0,0)]
    [InlineData(1,128)]
    [InlineData(128,1)]
    [InlineData(150,105)]
    [InlineData(255,255)]
    public void BitOrderMatchesExpected(byte input,byte expected)=>BitAndChecksumScenarios.Reverse(input,expected);
}
