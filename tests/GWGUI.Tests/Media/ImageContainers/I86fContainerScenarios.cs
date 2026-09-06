using GWGUI.MediaEngine.Containers.I86f;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class I86fContainerScenarios
{
    public static async Task Extended(int damage)
    {
        var bytes=new byte[2068]; "86BF"u8.CopyTo(bytes);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(6),0x1088);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12),2056);
        bytes[2056]=8; System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(2058),9);
        bytes[2066]=0x80; bytes[2067]=0x80;
        if(damage==1) bytes[0]=0;
        if(damage==2) System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(2058),0);
        if(damage==3) System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(2058),33);
        if(damage==4) bytes[2066]=bytes[2067]=0;
        if(damage==5) bytes[15]=0xff;
        var reader=new I86fReader();
        if(damage is 1 or 2 or 3 or 5) { await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync(bytes.AsMemory())); return; }
        var image=await reader.ReadAsync(bytes.AsMemory());
        if(damage==4) Assert.Empty(image.Tracks);
        else { var track=Assert.Single(image.Tracks); Assert.Equal(9,track.Bits.Count); Assert.Equal(new[]{true,false,false,false,false,false,false,false,true},track.Bits); }
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync(bytes.AsMemory(),new CancellationToken(true)));
    }

    public static async Task Read(bool reverse)
    {
        var bytes = new byte[1040]; "86BF"u8.CopyTo(bytes);
        var flags = reverse ? I86fFileFlags.ReverseByteOrder : I86fFileFlags.None;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(6), (ushort)flags);
        bytes[8] = 8; bytes[9] = 4; bytes[1038] = 0x80; bytes[1039] = 1;
        var reader = new I86fReader(); var image = await reader.ReadAsync(bytes.AsMemory());
        var track = Assert.Single(image.Tracks);
        Assert.Equal(16, track.Bits.Count);
        Assert.Equal(reverse ? new[] { 7, 8 } : new[] { 0, 15 }, track.Bits.Select((bit, index) => (bit, index)).Where(value => value.bit).Select(value => value.index));
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(bytes.AsMemory(0, 1039)));
        bytes[8] = 255; bytes[9] = 127;
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(bytes.AsMemory()));
    }
}
