using GWGUI.MediaEngine.FileSystems.Cpm;
using GWGUI.MediaEngine.SectorImages;
namespace GWGUI.Tests.Media.FileSystems;
internal static class CpmFileSystemScenarios
{
    public static void Amstrad(int firstSector, bool pcw, int damage)
    {
        var origin = pcw ? 4608 : firstSector == 0xc1 ? 0 : 9216;
        var bytes = new byte[origin + 8192];
        if (pcw) { bytes[2]=40; bytes[3]=9; bytes[4]=2; bytes[5]=1; bytes[6]=3; bytes[7]=2; }
        bytes.AsSpan(origin,2048).Fill(0xe5);
        void Entry(int index,string name,int records,int allocation,int extent=0)
        {
            var entry=bytes.AsSpan(origin+index*32,32); entry.Clear();
            System.Text.Encoding.ASCII.GetBytes(name,entry[1..]); entry[12]=(byte)extent; entry[15]=(byte)records; entry[16]=(byte)allocation;
        }
        // Directory order differs from extent order and the allocations are fragmented.
        Entry(0,"file    bin",2,5,1); Entry(1,"file    bin",8,2); Entry(2,"EMPTY      ",0,0);
        bytes.AsSpan(origin+2048,1024).Fill(42); bytes.AsSpan(origin+5120,256).Fill(93);
        if(damage==1) bytes[origin+16]=99;
        if(damage==2) bytes[origin+16]=2;
        if(damage==4) bytes.AsSpan(origin,2048).Fill(0xe5);
        var blocks=Enumerable.Range(0,bytes.Length/512).Select(i=>new SectorBlock(i,new(0,0,firstSector+i),bytes.AsSpan(i*512,512).ToArray())).ToArray();
        var format=pcw?GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AmstradPcw:GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AmstradCpc;
        var image=new SectorImage(format,512,1,1,blocks.Length,blocks.Where(block=>damage!=3 || block.LogicalBlock!=(origin+5120)/512));
        var reader=new AmstradCpmFileSystemReader();
        if(damage==4) { Assert.Equal(pcw,reader.CanRead(image)); if(pcw) Assert.Empty(reader.Read(image).Entries); else Assert.Throws<InvalidDataException>(()=>reader.Read(image)); return; }
        Assert.True(reader.CanRead(image)); var volume=reader.Read(image);
        var file=Assert.Single(volume.Entries,entry=>entry.Name=="file.bin");
        Assert.Empty(Assert.Single(volume.Entries,entry=>entry.Name=="EMPTY").Content!);
        var expected=Enumerable.Repeat((byte)42,1024).Concat(Enumerable.Repeat(damage is 1 or 3?(byte)0:damage==2?(byte)42:(byte)93,256));
        Assert.Equal(expected,file.Content); Assert.Equal(damage is 0 or 2,file.MetadataValid);
        if(damage==0) Assert.Empty(volume.Warnings); else Assert.NotEmpty(volume.Warnings);
    }
    public static void Volume(string format, int origin, int allocationSize, int entryCount, bool wide)
    {
        var dataAllocation = wide ? 257 : 2;
        var totalAllocations = wide ? 258 : 4;
        var bytes = new byte[origin+allocationSize*totalAllocations];
        bytes.AsSpan(origin,entryCount*32).Fill(0xe5);
        for (var index=0;index<4;index++)
        {
            var entry=bytes.AsSpan(origin+index*32,32); entry.Clear(); entry[0]=(byte)index; "FILE    BIN"u8.CopyTo(entry[1..]);
            if(index==0) { entry[15]=6; entry[16]=(byte)dataAllocation; if(wide) entry[17]=1; }
        }
        var label=bytes.AsSpan(origin+128,32); label.Clear(); label[0]=0x20; "VOLUME  "u8.CopyTo(label[1..]);
        var payload=Enumerable.Range(0,768).Select(i=>(byte)(i%251)).ToArray(); payload.CopyTo(bytes,origin+allocationSize*dataAllocation);
        var blocks=Enumerable.Range(0,bytes.Length/256).Select(index=>new SectorBlock(index,new(0,0,index),bytes.AsSpan(index*256,256).ToArray())).ToArray();
        var image=new SectorImage(format,256,1,1,blocks.Length,blocks); var reader=new CpmFileSystemReader(); Assert.True(reader.CanRead(image)); var volume=reader.Read(image);
        Assert.Equal("VOLUME",volume.Name); Assert.Equal(4,volume.Entries.Count);
        var file=Assert.Single(volume.Entries,entry=>entry.RawAttributes==0); Assert.Equal("FILE.BIN",file.Name); Assert.Equal(768,file.Size); Assert.Equal(payload,file.Content); Assert.True(file.MetadataValid);
        Assert.All(volume.Entries.Where(entry=>entry.RawAttributes!=0),entry=>{ Assert.Equal(0,entry.Size); Assert.Empty(entry.Content!); }); Assert.Empty(volume.Warnings);
        Assert.Equal((wide ? 255L : 1L)*allocationSize,volume.FreeBytes);
        // The directory remains recognizable when one data block is missing.
        var damaged=new SectorImage(format,256,1,1,blocks.Length,blocks.Where(block=>block.LogicalBlock!=(origin+allocationSize*dataAllocation)/256));
        var result=reader.Read(damaged); Assert.NotEmpty(result.Warnings); Assert.DoesNotContain(result.Entries,entry=>entry.RawAttributes==0 && entry.MetadataValid);
    }
}
