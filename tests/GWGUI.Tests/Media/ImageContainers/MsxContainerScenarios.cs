using GWGUI.MediaEngine.Containers.Msx.Raw;
using System.Buffers.Binary;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class MsxContainerScenarios
{
    public static async Task Read(int size,int cylinders,int heads,byte descriptor)
    {
        var data=new byte[size];
        "MSX     "u8.CopyTo(data.AsSpan(3));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(11),512);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(19),(ushort)(size/512));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(24),9);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(26),(ushort)heads);
        data[21]=descriptor;data[^1]=42;
        var reader=new MsxRawImageReader((_,token)=>{token.ThrowIfCancellationRequested();return Task.FromResult(data);});
        var image=await reader.ReadAsync("virtual");
        Assert.Equal(cylinders,image.Cylinders);Assert.Equal(heads,image.Heads);
        Assert.Equal(size,image.Capacity);
        Assert.Equal(42,image.AvailableBlocks.Single(b=>b.LogicalBlock==image.BlockCount-1).Data[^1]);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        await new MsxRawImageWriter(new GWGUI.MediaEngine.Containers.Raw.LinearSectorImageWriter(files)).WriteAsync(image,"output.dsk",image.FormatId);
        Assert.Equal(data,files.Files["output.dsk"]);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync("virtual",new CancellationToken(true)));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>new MsxRawImageWriter(new GWGUI.MediaEngine.Containers.Raw.LinearSectorImageWriter(files)).WriteAsync(image,"cancelled.dsk",image.FormatId,new CancellationToken(true)));
        Assert.False(files.Files.ContainsKey("cancelled.dsk"));
        data[3]=0;
        await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"));
    }
}
