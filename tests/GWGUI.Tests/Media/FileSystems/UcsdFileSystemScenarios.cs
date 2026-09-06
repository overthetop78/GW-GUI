using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Ucsd;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
namespace GWGUI.Tests.Media.FileSystems;
internal static class UcsdFileSystemScenarios
{
    public static void Directory(int end,int damage)
    {
        var bytes=new byte[16*512]; var directory=bytes.AsMemory(1024);
        void Word(int offset,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(directory.Span[offset..],value);
        Word(2,(ushort)end); Word(14,16); Word(16,damage==5?(ushort)0:(ushort)2); directory.Span[6]=4; "TEST"u8.CopyTo(directory.Span[7..]);
        Word(26,(ushort)end); Word(28,(ushort)(end+2)); directory.Span[32]=3; "ONE"u8.CopyTo(directory.Span[33..]); Word(48,2);
        Word(52,(ushort)(end+4)); Word(54,(ushort)(end+5)); directory.Span[58]=3; "TWO"u8.CopyTo(directory.Span[59..]); Word(74,1);
        bytes.AsSpan(end*512,512).Fill(42); bytes[end*512+512]=93; bytes[end*512+513]=94; bytes[(end+4)*512]=17;
        if(damage==1) Word(54,17);
        if(damage==2) { Word(52,(ushort)end); Word(54,(ushort)(end+1)); }
        if(damage==3) Word(74,513);
        var blocks=Enumerable.Range(0,16).Select(i=>new SectorBlock(i,new(0,0,i),damage==4 && i==2?new byte[20]:bytes.AsSpan(i*512,512).ToArray()));
        var image=new SectorImage(DiskImageFormatIds.UcsdIbmMfm,512,1,1,16,blocks); var reader=new UcsdFileSystemReader();
        if(damage==4) { Assert.False(reader.CanRead(image)); Assert.Throws<InvalidDataException>(()=>reader.Read(image)); return; }
        Assert.True(reader.CanRead(image)); var volume=reader.Read(image); Assert.Equal("TEST",volume.Name);
        if(damage==5) { Assert.Empty(volume.Entries); Assert.Empty(volume.Warnings); Assert.Equal((16-end)*512L,volume.FreeBytes); return; }
        var first=Assert.Single(volume.Entries,item=>item.Name=="ONE"); Assert.Equal(Enumerable.Repeat((byte)42,512).Concat(new byte[]{93,94}),first.Content); Assert.True(first.MetadataValid);
        if(damage==0) { Assert.Equal(new byte[]{17},Assert.Single(volume.Entries,item=>item.Name=="TWO").Content); Assert.Empty(volume.Warnings); Assert.Equal((16-end-3)*512L,volume.FreeBytes); }
        else { Assert.NotEmpty(volume.Warnings); Assert.Equal(0,volume.FreeBytes); Assert.DoesNotContain(volume.Entries,item=>item.Name=="TWO" && item.MetadataValid); }
    }
    public static void Volume(bool bigEndian)
    {
        var directory = new byte[2048];
        void Word(int offset, ushort value) { if (bigEndian) BinaryPrimitives.WriteUInt16BigEndian(directory.AsSpan(offset), value); else BinaryPrimitives.WriteUInt16LittleEndian(directory.AsSpan(offset), value); }
        Word(2, 6); Word(14, 8); Word(16, 1); directory[6] = 4; "TEST"u8.CopyTo(directory.AsSpan(7));
        Word(26, 6); Word(28, 7); Word(30, 5); directory[32] = 4; "FILE"u8.CopyTo(directory.AsSpan(33)); Word(48, 2);
        var blocks = Enumerable.Range(0, 4).Select(index => new SectorBlock(index + 2, new(0,0,index+2), directory.AsSpan(index * 512, 512).ToArray())).ToList();
        var payload = new byte[512]; payload[0] = 42; payload[1] = 93; blocks.Add(new(6, new(0,0,6), payload));
        var image = new SectorImage(DiskImageFormatIds.UcsdIbmMfm, 512, 1, 1, 8, blocks);
        var reader = new UcsdFileSystemReader(); Assert.True(reader.CanRead(image));
        var volume = reader.Read(image); var file = Assert.Single(volume.Entries);
        Assert.Equal(512L, volume.FreeBytes);
        Assert.Equal("TEST", volume.Name); Assert.Equal("FILE", file.Name); Assert.Equal(2, file.Size); Assert.Equal(new byte[] { 42, 93 }, file.Content); Assert.True(file.MetadataValid); Assert.Empty(volume.Warnings);
        var incomplete = new SectorImage(image.FormatId, 512, 1, 1, 8, blocks.Where(block => block.LogicalBlock != 6));
        var damaged = reader.Read(incomplete); Assert.False(Assert.Single(damaged.Entries).MetadataValid); Assert.NotEmpty(damaged.Warnings); Assert.Equal(0, damaged.FreeBytes);
    }
}
