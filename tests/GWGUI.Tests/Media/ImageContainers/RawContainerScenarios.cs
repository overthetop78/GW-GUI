using GWGUI.MediaEngine.Containers.Raw;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class RawContainerScenarios
{
    public static async Task Interpretation(bool directory, bool specification, bool fat)
    {
        var bytes = new byte[184320];
        if (directory)
        {
            bytes.AsSpan(9216, 2048).Fill(0xe5);
            bytes.AsSpan(9216, 32).Clear();
            "FILE    BIN"u8.CopyTo(bytes.AsSpan(9217));
        }
        if (specification)
        {
            bytes[2] = 40; bytes[3] = 9; bytes[4] = 2; bytes[5] = 1; bytes[6] = 3; bytes[7] = 2;
        }
        if (fat)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(11), 512);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(19), 360);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(24), 9);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(26), 1);
        }
        var reader = new RawImgReader((_, token) => { token.ThrowIfCancellationRequested(); return Task.FromResult(bytes); });
        var image = await reader.ReadAsync("memory.img");
        var expected = fat ? "ibm.180" : directory ? GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AmstradCpc : specification ? GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AmstradPcw : "ibm.180";
        Assert.Equal(expected, image.FormatId);
        Assert.Equal(bytes, image.AvailableBlocks.OrderBy(block => block.LogicalBlock).SelectMany(block => block.Data));
    }
    public static async Task Write(int failure)
    {
        var files=new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles(); files.Files["output"]=[91];
        var geometry=new GWGUI.MediaEngine.Reconstruction.RegularSectorGeometry("synthetic",4,1,1,2);
        var blocks=new List<GWGUI.MediaEngine.SectorImages.SectorBlock> { new(1,new(0,0,1),[5,6,7,8]),new(0,new(0,0,0),[1,2,3,4]) };
        if(failure==1) blocks.RemoveAt(0);
        if(failure==2) blocks[0]=new(1,new(0,0,1),[5,6]);
        var image=new GWGUI.MediaEngine.SectorImages.SectorImage(failure==3?"wrong":"synthetic",4,1,1,2,blocks);
        var writer=new LinearSectorImageWriter(files);
        if(failure==0) { await writer.WriteAsync(image,"output",geometry); Assert.Equal(new byte[]{1,2,3,4,5,6,7,8},files.Files["output"]); }
        else { await Assert.ThrowsAsync<InvalidDataException>(()=>writer.WriteAsync(image,"output",geometry)); Assert.Equal(new byte[]{91},files.Files["output"]); }
        Assert.Equal(failure==3?0:1,files.Calls.Count);
        using var cancelled=new CancellationTokenSource(); cancelled.Cancel();
        if(failure==0) await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(image,"output",geometry,cancelled.Token));
    }
    public static async Task Read(string extension,int length,int cylinders,int heads,int sectors,int blockSize)
    {
        var data=new byte[length];data[0]=42;data[^1]=93;
        var calls=0;
        var reader=new RawImgReader((path,token)=>{
            Assert.Equal("virtual"+extension,path);token.ThrowIfCancellationRequested();calls++;return Task.FromResult(data);
        });
        var image=await reader.ReadAsync("virtual"+extension);
        Assert.Equal(1,calls);
        Assert.Equal(cylinders,image.Cylinders);Assert.Equal(heads,image.Heads);
        Assert.Equal(sectors,image.SectorsPerTrack);Assert.Equal(blockSize,image.BlockSize);
        Assert.Equal(length,image.Capacity);Assert.Empty(image.MissingBlocks);
        var ordered=image.AvailableBlocks.OrderBy(b=>b.LogicalBlock).ToArray();
        Assert.Equal(42,ordered[0].Data[0]);Assert.Equal(93,ordered[^1].Data[^1]);
        using var cancelled=new CancellationTokenSource();cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync("virtual"+extension,cancelled.Token));
        data=new byte[3];
        await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"+extension));
    }
}
