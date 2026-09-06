using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Flux;
namespace GWGUI.Tests.Media.MediaCodecs;
internal static class SectorReconstructionScenarios
{
    public static async Task Apple(int kind)
    {
        string id=kind>=4?"applemac.gcr":kind==3?"apple2.rwts18":"apple2.gcr";
        int size=kind>=4?512:kind==3?768:256;
        int[] numbers=kind==3?[0,1,2,3,4,5]:kind==2?[0,2]:kind==1?[14]:[0];
        var sectors=numbers.Select(number=>new GWGUI.MediaEngine.Encoding.TrackSector(number,Enumerable.Repeat((byte)(42+number),size).ToArray(),Attributes:new Dictionary<string,int>{{"tag0",93}})).ToArray();
        var encoded=new GWGUI.MediaEngine.Encoding.FluxEncoderRegistry().Encode(id,new(0,0,sectors,new Dictionary<string,int>{{"sectorsPerTrack",kind==0?13:16}}));
        var scp=new ScpImage(new(0x24,0,1,0,0,ScpFlags.None,ScpBitCellEncoding.Default16Bit,ScpHeadSelection.Both,0,0),[new(0,0,0,[new(encoded.Revolution,(uint)encoded.Revolution.FluxIntervals.Count)])],true,0);
        string format=kind switch {0=>GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AppleIIDos32,1=>GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AppleIIDos33,2=>GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AppleIIProDos140,3=>GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AppleIIRwts18,4=>GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AppleMacGcr,_=>GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AppleLisaOffice};
        var reader=new GWGUI.MediaEngine.Reconstruction.Apple.AppleScpSectorImageReader(new MemoryCapture(scp),new());
        var image=await reader.ReadAsync("virtual",format); Assert.Equal(kind==3?6:1,image.AvailableBlocks.Count); Assert.NotEmpty(image.MissingBlocks);
        if(kind==2) Assert.Equal(Enumerable.Repeat((byte)42,256).Concat(Enumerable.Repeat((byte)44,256)),Assert.Single(image.AvailableBlocks).Data);
        else foreach(var sector in sectors) Assert.Equal(sector.Data,image.AvailableBlocks.Single(b=>b.Address.Number==sector.Number).Data);
        Assert.All(image.AvailableBlocks,b=>{Assert.True(b.IntegrityValid); Assert.Equal(1,b.Revolution);});
        if(kind>=4) Assert.Equal(93,Assert.Single(image.AvailableBlocks).Tag![0]);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync("virtual",format,new CancellationToken(true)));
    }
    private sealed class MemoryCapture(ScpImage image):IScpReader
    {
        public Task<ScpImage> ReadAsync(string path,CancellationToken cancellationToken=default) { cancellationToken.ThrowIfCancellationRequested(); Assert.Equal("virtual",path); return Task.FromResult(image); }
    }

    public static async Task Scp(string kind,bool empty)
    {
        string codec=kind switch {"amiga" or "amiga-hd"=>"amiga.mfm","dec"=>"dec.rx02","1541" or "1571"=>"commodore.gcr","1581"=>"iso.mfm",_=>"commodore900.gcr"};
        int size=kind is "1541" or "1571" or "dec"?256:512;
        byte cylinder=kind is "1541" or "1571" or "dec"?(byte)1:(byte)0;
        int[] numbers=kind=="dec"?[1,3]:kind is "1581" or "900"?[1]:kind=="amiga-hd"?[11]:[0];
        var decoder=new Decoder(codec,flux=>empty?[]:numbers.Select(number=>new DecodedSector(cylinder,0,number,0,size,flux.FluxIntervals[0]==2,0,Data:Enumerable.Repeat(flux.FluxIntervals[0]==2?(byte)(42+number):(byte)1,size).ToArray())).ToArray());
        var registry=new FluxDecoderRegistry([decoder]);
        var source=new Capture(kind=="dec"?1:0);
        Task<SectorImage> Read(CancellationToken token)=>kind switch
        {
            "amiga" or "amiga-hd"=>new GWGUI.MediaEngine.Reconstruction.Amiga.AmigaScpSectorImageReader(source,registry).ReadAsync("virtual",token),
            "dec"=>new GWGUI.MediaEngine.Reconstruction.Dec.DecRx02ScpSectorImageReader(source,registry).ReadAsync("virtual",token),
            _=>new GWGUI.MediaEngine.Reconstruction.Commodore.CommodoreScpSectorImageReader(source,registry).ReadAsync("virtual",kind=="900"?GWGUI.MediaEngine.Definitions.DiskImageFormatIds.Commodore900Coherent:"commodore."+kind,token)
        };
        if(empty||kind=="amiga-hd") { await Assert.ThrowsAsync<InvalidDataException>(()=>Read(CancellationToken.None)); return; }
        var image=await Read(CancellationToken.None); Assert.Equal(kind=="1581"?2:1,image.AvailableBlocks.Count);
        Assert.All(image.AvailableBlocks,block=>{Assert.True(block.IntegrityValid); Assert.Equal(2,block.Revolution);});
        if(kind=="dec") Assert.Equal(Enumerable.Repeat((byte)43,256).Concat(Enumerable.Repeat((byte)45,256)),Assert.Single(image.AvailableBlocks).Data);
        else Assert.All(image.AvailableBlocks,block=>Assert.Equal(Enumerable.Repeat((byte)(42+numbers[0]),block.Data.Count),block.Data));
        Assert.NotEmpty(image.MissingBlocks);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>Read(new CancellationToken(true)));
    }

    public static void AmigaDensity()
    {
        var track=Enumerable.Range(0,22).Select(number=>new SectorAddress(0,0,number)).ToArray();
        Assert.Equal(11,GWGUI.MediaEngine.Reconstruction.Amiga.AmigaScpSectorImageReader.InferSectorsPerTrack(track));
        Assert.Equal(22,GWGUI.MediaEngine.Reconstruction.Amiga.AmigaScpSectorImageReader.InferSectorsPerTrack(track.Concat(track.Select(address=>new SectorAddress(1,0,address.Number)))));
        Assert.Equal(11,GWGUI.MediaEngine.Reconstruction.Amiga.AmigaScpSectorImageReader.InferSectorsPerTrack([new(0,0,21),new(1,0,21)]));
    }

    private sealed class Decoder(string id,Func<FluxRevolution,IReadOnlyList<DecodedSector>> decode):IFluxDecoder
    {
        public string Id=>id; public string DisplayName=>id;
        public FluxDecodeResult Decode(FluxRevolution flux)=>new(Id,Id,1,80,[],[],decode(flux));
    }
    private sealed class Capture(int cylinder):IScpReader
    {
        public Task<ScpImage> ReadAsync(string path,CancellationToken cancellationToken=default)
        {
            cancellationToken.ThrowIfCancellationRequested(); Assert.Equal("virtual",path);
            return Task.FromResult(new ScpImage(new(0x24,0,2,(byte)(cylinder*2),(byte)(cylinder*2),ScpFlags.None,ScpBitCellEncoding.Default16Bit,ScpHeadSelection.Both,0,0),
                [new((byte)(cylinder*2),cylinder,0,[new(100,1,new uint[]{1}),new(100,1,new uint[]{2})])],true,0));
        }
    }

    public static void Iso(string format,int first,int size)
    {
        var candidates=new Dictionary<SectorAddress,List<IsoSectorCandidate>>();
        foreach(var (cylinder,number) in new[]{(0,first),(0,first+1),(0,first+2),(2,first),(2,first+2)})
        {
            var address=new SectorAddress(cylinder,0,number);
            DecodedSector Sector(bool valid,byte value)=>new((byte)cylinder,0,number,0,size,valid,0,Data:Enumerable.Repeat(value,size).ToArray());
            candidates[address]=[new(Sector(false,1),1),new(Sector(true,(byte)(number-first+42)),2),new(Sector(true,99),3)];
        }
        var image=IsoScpSectorImagePolicyRegistry.Resolve(format).Build(format,new(candidates,candidates));
        Assert.Equal(5,image.AvailableBlocks.Count);
        foreach(var block in image.AvailableBlocks)
        {
            Assert.True(block.IntegrityValid); Assert.Equal(2,block.Revolution);
            Assert.Equal(Enumerable.Repeat((byte)(block.Address.Number-first+42),size),block.Data);
        }
        Assert.NotEmpty(image.MissingBlocks);
        Assert.DoesNotContain(image.AvailableBlocks,b=>b.Address.Cylinder==1);
        Assert.DoesNotContain(image.AvailableBlocks,b=>b.Address.Cylinder==2&&b.Address.Number==first+1);
        Assert.Equal(Enumerable.Repeat((byte)42,size),IsoSectorImageBuilder.BestData(candidates,new(0,0,first)));
        Assert.Empty(IsoSectorImageBuilder.BestData(candidates,new(1,0,first)));
    }

    public static void BestCandidate()
    {
        var bad=new DecodedSector(0,0,1,0,128,false,0,Data:[1]);
        var unknown=new DecodedSector(0,0,1,0,128,null,1,Data:[2]);
        var good=new DecodedSector(0,0,1,0,128,true,2,Data:[3]);
        Assert.Same(good,SectorCandidateSelector.Best(new[]{bad,unknown,good},x=>x.IntegrityValid));
        Assert.Same(unknown,SectorCandidateSelector.Best(new[]{bad,unknown},x=>x.IntegrityValid));
        Assert.Same(good,SectorCandidateSelector.Best(new[]{good,good with{BitOffset=3}},x=>x.IntegrityValid));
        Assert.Throws<InvalidOperationException>(()=>SectorCandidateSelector.Best(Array.Empty<DecodedSector>(),x=>x.IntegrityValid));
    }
}
