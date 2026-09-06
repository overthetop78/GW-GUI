using GWGUI.MediaEngine.Containers.Scp;
using System.Buffers.Binary;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class ScpContainerScenarios
{
    public static void Revolutions(int damage)
    {
        var bytes=new byte[726]; "SCP"u8.CopyTo(bytes); bytes[5]=2; bytes[6]=bytes[7]=3; bytes[9]=16; bytes[10]=2;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(28),688); "TRK"u8.CopyTo(bytes.AsSpan(688)); bytes[691]=3;
        void Word(int offset,uint value)=>BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset),value);
        Word(692,4000000); Word(696,3); Word(700,28); Word(704,4000001); Word(708,2); Word(712,34);
        new byte[]{0,0,0,1,0,0,0,42,0,93}.CopyTo(bytes,716);
        if(damage==1) bytes[691]=2;
        if(damage==2) Word(712,5000);
        if(damage==3) bytes[9]=8;
        if(damage==4) bytes[8]=64;
        if(damage==5) bytes[10]=3;
        var reader=new ScpReader();
        if(damage is 3 or 4) { Assert.Throws<NotSupportedException>(()=>reader.Read(bytes)); return; }
        if(damage!=0) { Assert.Throws<InvalidDataException>(()=>reader.Read(bytes)); return; }
        var image=reader.Read(bytes); var track=Assert.Single(image.Tracks); Assert.Equal(1,track.Cylinder); Assert.Equal(1,track.Head);
        Assert.Equal(2,track.Revolutions.Count); Assert.Equal(new uint[]{65537,65536},track.Revolutions[0].FluxIntervals);
        Assert.Equal(new uint[]{42,93},track.Revolutions[1].FluxIntervals); Assert.Equal(4000001u,track.Revolutions[1].IndexTimeTicks);
    }

    private static ScpImage Image()=>new(new ScpHeader(0x24,0,1,0,0,ScpFlags.None,ScpBitCellEncoding.Default16Bit,ScpHeadSelection.Both,0,0),
        [new ScpTrack(0,0,0,[new ScpRevolution(100000,2,new uint[]{80,160})])],true,0);
    public static async Task Write()
    {
        using var stream=new MemoryStream();
        await ScpWriter.WriteAsync(stream,Image());
        Assert.True(stream.CanRead);
        var bytes=stream.ToArray();
        Assert.Equal(new byte[]{83,67,80},bytes[..3]);
        Assert.Equal(688u,BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(16,4)));
        Assert.All(bytes[20..688],value=>Assert.Equal(0,value));
        Assert.Equal(new byte[]{84,82,75,0},bytes[688..692]);
        Assert.Equal(100000u,BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(692,4)));
        Assert.Equal(2u,BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(696,4)));
        Assert.Equal(16u,BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(700,4)));
        Assert.Equal(new byte[]{0,80,0,160},bytes[704..]);
        Assert.Equal((uint)bytes.Skip(16).Sum(x=>(int)x),BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(12,4)));
        var parsed=new ScpReader().Read(bytes);
        Assert.True(parsed.ChecksumValid);
        Assert.Equal(new uint[]{80,160},Assert.Single(Assert.Single(parsed.Tracks).Revolutions).FluxIntervals);
        bytes[^1]^=1;
        Assert.False(new ScpReader().Read(bytes).ChecksumValid);
    }
    public static void Invalid(int change)
    {
        var bytes=new byte[688];
        bytes[0]=83;bytes[1]=67;bytes[2]=80;bytes[5]=1;
        switch(change)
        {
            case 0: bytes[0]=0;break;
            case 1: bytes[5]=0;break;
            case 2: bytes[7]=168;break;
            case 3: BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(16,4),10000);break;
        }
        Assert.Throws<InvalidDataException>(()=>new ScpReader().Read(bytes));
    }
    public static async Task Reject()
    {
        using var stream=new MemoryStream();
        using var cancelled=new CancellationTokenSource();cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>ScpWriter.WriteAsync(stream,Image(),cancelled.Token));
        Assert.Equal(0,stream.Length);
        var image=Image();
        var duplicate=new ScpImage(image.Header,[image.Tracks[0],image.Tracks[0]],true,0);
        await Assert.ThrowsAsync<InvalidDataException>(()=>ScpWriter.WriteAsync(stream,duplicate));
        Assert.Equal(0,stream.Length);
    }
}
