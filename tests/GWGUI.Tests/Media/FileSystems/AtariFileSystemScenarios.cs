using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Atari.Dos;
using GWGUI.MediaEngine.SectorImages;
namespace GWGUI.Tests.Media.FileSystems;
internal static class AtariFileSystemScenarios
{
    public static void Variant(string format,int size,int damage)
    {
        var vtoc=new byte[size]; vtoc[0]=2;
        var directory=new byte[size]; directory[0]=0x40; directory[1]=damage==1?(byte)1:(byte)2; directory[3]=1; "FILE    BIN"u8.CopyTo(directory.AsSpan(5));
        var first=new byte[size]; first[0]=42; first[^2]=6; first[^1]=1;
        var second=new byte[size]; second[0]=93; second[^1]=1;
        if(damage==1) { first[^2]=0; first[^1]=0; }
        if(damage==2) first[^2]=1;
        if(damage==3) first[^3]=3;
        if(damage==4) second=[1,2];
        if(damage==5) first[^3]=4;
        if(damage==6) first[^1]=255;
        var blocks=new List<SectorBlock>{new(0,new(0,0,1),first),new(5,new(0,0,6),second),new(359,new(19,0,18),vtoc)};
        for(var index=360;index<368;index++) blocks.Add(new(index,new(index/18,0,index%18+1),index==360?directory:new byte[size]));
        var image=new SectorImage(format,size,40,1,format==DiskImageFormatIds.Atari130?26:18,blocks);
        var reader=new AtariDosFileSystemReader(); Assert.True(reader.CanRead(image)); var volume=reader.Read(image); var file=Assert.Single(volume.Entries);
        Assert.Equal("FILE.BIN",file.Name); Assert.Equal(damage<=1,file.MetadataValid);
        if(damage==0 || damage==5) Assert.Equal(new byte[]{42,93},file.Content);
        if(damage==1) Assert.Empty(file.Content!);
        if(damage is 2 or 3 or 4) Assert.Equal(new byte[]{42},file.Content);
        if(damage==6) Assert.Equal(Math.Min(255,size-3)+1,file.Size);
        if(damage<=1) Assert.Empty(volume.Warnings); else Assert.NotEmpty(volume.Warnings);
        directory[0]=0x80; var deleted=new SectorImage(format,size,40,1,18,blocks.Select(block=>block.LogicalBlock==360?new SectorBlock(360,block.Address,directory):block));
        Assert.Empty(reader.Read(deleted).Entries);
        vtoc[0]=0; var invalid=new SectorImage(format,size,40,1,18,blocks.Select(block=>block.LogicalBlock==359?new SectorBlock(359,block.Address,vtoc):block));
        Assert.False(reader.CanRead(invalid)); Assert.Throws<InvalidDataException>(()=>reader.Read(invalid));
    }
    public static void Dos(bool broken)
    {
        var vtoc = new byte[128]; vtoc[0] = 2; vtoc[3] = 10;
        var directory = new byte[128]; directory[0] = 0x40; directory[1] = 1; directory[3] = 1;
        "FILE    BIN"u8.CopyTo(directory.AsSpan(5));
        var data = new byte[128]; data[0] = 42; data[1] = 93; data[127] = 2; if (broken) data[126] = 1;
        var blocks = new List<SectorBlock> { new(0, new(0,0,1), data), new(359, new(19,0,18), vtoc) };
        for (var index = 360; index < 368; index++) blocks.Add(new(index, new(index/18,0,index%18+1), index == 360 ? directory : new byte[128]));
        var image = new SectorImage(DiskImageFormatIds.Atari90, 128, 40, 1, 18, blocks);
        var reader = new AtariDosFileSystemReader(); Assert.True(reader.CanRead(image));
        var volume = reader.Read(image); var file = Assert.Single(volume.Entries);
        Assert.Equal("FILE.BIN", file.Name); Assert.Equal(new byte[] { 42, 93 }, file.Content);
        Assert.Equal(!broken, file.MetadataValid); Assert.Equal(1280, volume.FreeBytes);
        if (broken) Assert.NotEmpty(volume.Warnings); else Assert.Empty(volume.Warnings);
    }
}
