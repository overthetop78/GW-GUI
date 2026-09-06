using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Decoding;
namespace GWGUI.Tests.Media.MediaCodecs;
internal static class TrackCodecScenarios
{
    public static void Raw()
    {
        Assert.Equal("None",RawFluxDecoder.Classify(1,80,0).ToString());
        Assert.Equal("ShortPulse",RawFluxDecoder.Classify(1,80,1).ToString());
        Assert.Equal("LongInterval",RawFluxDecoder.Classify(801,80,1).ToString());
        Assert.Equal("None",RawFluxDecoder.Classify(800,80,1).ToString());
        Assert.Equal(64,RawFluxDecoder.ConvertToCellCount(100000,80));
        var result=new FluxDecoderRegistry().Decode("raw",new GWGUI.MediaEngine.Flux.FluxRevolution(10000,new uint[]{160,160,160,160,10000}));
        Assert.Empty(result.Sectors); Assert.Empty(result.DecodedBytes); Assert.Equal(.05,result.Confidence); Assert.NotEmpty(result.Structures);
    }

    public static void Specialized(string id,int length,bool system=false)
    {
        var payload=Enumerable.Range(0,length).Select(i=>(byte)(i%251)).ToArray();
        var encoder=new FluxEncoderRegistry().Get(id); var decoders=new FluxDecoderRegistry();
        var request=new TrackEncodeRequest(2,0,[new TrackSector(3,payload,Attributes:new Dictionary<string,int>{{"system",system?1:0}})]);
        var encoded=encoder.Encode(request); var decoded=decoders.Decode(id,encoded.Revolution);
        var sector=Assert.Single(decoded.Sectors); Assert.True(sector.IntegrityValid); Assert.Equal(payload,sector.Data!.Take(length));
        Assert.Same(decoded,decoders.Decode(id,encoded.Revolution));
        var damaged=encoded.Bits.ToArray(); for(int index=damaged.Length/2;index<damaged.Length/2+16;index++) damaged[index]=!damaged[index];
        var corrupted=decoders.Decode(id,GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create(damaged,request.BitCellTicks,request.IndexTimeTicks));
        Assert.DoesNotContain(corrupted.Sectors,s=>s.IntegrityValid==true&&s.Data is not null&&s.Data.Take(length).SequenceEqual(payload));
        Assert.ThrowsAny<ArgumentException>(()=>encoder.Encode(new(-1,0,[new(3,payload)])));
        Assert.ThrowsAny<ArgumentException>(()=>encoder.Encode(new(0,0,[])));
    }

    public static void EncodeAndDecode(int kind)
    {
        ITrackEncoder encoder=kind switch { 0=>new IsoMfmTrackEncoder(),1=>new IsoFmTrackEncoder(),2=>new AmigaMfmTrackEncoder(),_=>new CommodoreGcrTrackEncoder() };
        IFluxDecoder decoder=kind switch { 0=>new IsoMfmDecoder(),1=>new IsoFmDecoder(),2=>new AmigaMfmDecoder(),_=>new CommodoreGcrDecoder() };
        var length=kind switch {2=>512,3=>256,_=>128}; var payload=Enumerable.Range(0,length).Select(i=>(byte)(i%251)).ToArray();
        var encoded=encoder.Encode(new(2,0,[new TrackSector(3,payload)])); Assert.NotEmpty(encoded.Bits); Assert.NotEmpty(encoded.Revolution.FluxIntervals);
        var result=decoder.Decode(encoded.Revolution); var sector=Assert.Single(result.Sectors);
        Assert.Equal(3,sector.Number); Assert.Equal(kind==3?3:2,sector.Cylinder); // Commodore headers store disk tracks starting at one.
        Assert.Equal(0,sector.Head); Assert.Equal(length,sector.SizeBytes); Assert.True(sector.IntegrityValid); Assert.Equal(payload,sector.Data);
        Assert.ThrowsAny<ArgumentException>(()=>encoder.Encode(new(2,0,[new TrackSector(3,[1,2,3])])));
        Assert.ThrowsAny<ArgumentException>(()=>encoder.Encode(new(2,0,[])));
        Assert.ThrowsAny<ArgumentException>(()=>encoder.Encode(new(-1,0,[new TrackSector(3,payload)])));
    }
}
