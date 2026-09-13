using GWGUI.MediaEngine.FileSystems.Atari.Dos;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.Tests.Media.FileSystems;
internal static class AtariFileSystemScenarios
{
    public static void Variant(string format,int size,int damage)
    {
        var vtoc=new byte[size]; vtoc[0]=2; vtoc[1]=0xc3; vtoc[2]=2;
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
        vtoc[0]=1; var invalid=new SectorImage(format,size,40,1,18,blocks.Select(block=>block.LogicalBlock==359?new SectorBlock(359,block.Address,vtoc):block));
        Assert.False(reader.CanRead(invalid)); Assert.Throws<InvalidDataException>(()=>reader.Read(invalid));
    }
    public static void VtocMarker(byte marker)
    {
        var vtoc = new byte[128]; vtoc[0] = marker; vtoc[1] = 0xc3; vtoc[2] = 2; vtoc[3] = 10;
        var directory = new byte[128]; directory[0] = 0x42; directory[1] = 1; directory[3] = 1;
        "FILE    BIN"u8.CopyTo(directory.AsSpan(5));
        var data = new byte[128]; data[0] = 42; data[127] = 1;
        var blocks = new List<SectorBlock> { new(0, new(0,0,1), data), new(359, new(19,0,18), vtoc) };
        for (var index=360; index<368; index++) blocks.Add(new(index,new(index/18,0,index%18+1),index==360?directory:new byte[128]));
        var image = new SectorImage(DiskImageFormatIds.Atari90, 128, 40, 1, 18, blocks);
        var reader = new AtariDosFileSystemReader();
        Assert.True(reader.CanRead(image));
        Assert.Equal("FILE.BIN", Assert.Single(reader.Read(image).Entries).Name);
    }
    public static void Dos(bool broken)
    {
        var vtoc = new byte[128]; vtoc[0] = 2; vtoc[1] = 0xc3; vtoc[2] = 2; vtoc[3] = 10;
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

    public static void NamedEmptyFile()
    {
        var vtoc = new byte[128]; vtoc[0] = 2; vtoc[1] = 0xc3; vtoc[2] = 2;
        var directory = new byte[128];
        directory[0] = 0x40;
        "EMPTY   TXT"u8.CopyTo(directory.AsSpan(5));
        directory[16] = 0x40; directory[17] = 1; directory[19] = 1;
        "FILE    BIN"u8.CopyTo(directory.AsSpan(21));
        var data = new byte[128]; data[0] = 42; data[127] = 1;
        var blocks = new List<SectorBlock> { new(0, new(0,0,1), data), new(359, new(19,0,18), vtoc) };
        for (var index = 360; index < 368; index++) blocks.Add(new(index, new(index/18,0,index%18+1), index == 360 ? directory : new byte[128]));
        var image = new SectorImage(DiskImageFormatIds.Atari90, 128, 40, 1, 18, blocks);
        var reader = new AtariDosFileSystemReader(); Assert.True(reader.CanRead(image));
        var entries = reader.Read(image).Entries;
        Assert.Equal(2, entries.Count);
        var empty = Assert.Single(entries, entry => entry.Name == "EMPTY.TXT");
        Assert.Equal(0, empty.Size); Assert.Empty(empty.Content!); Assert.True(empty.MetadataValid);
        Assert.Contains(entries, entry => entry.Name == "FILE.BIN");

        directory[1] = 1;
        var inconsistent = new SectorImage(DiskImageFormatIds.Atari90, 128, 40, 1, 18, blocks.Select(block=>block.LogicalBlock==360?new SectorBlock(360,block.Address,directory):block));
        Assert.False(reader.CanRead(inconsistent));
    }
    public static void StopsAtFirstUnusedDirectoryEntry()
    {
        var vtoc = new byte[128]; vtoc[0] = 2; vtoc[1] = 0xc3; vtoc[2] = 2;
        var directory = new byte[128]; directory[0] = 0x42; directory[1] = 1; directory[3] = 1;
        "FILE    BIN"u8.CopyTo(directory.AsSpan(5));
        var garbage = new byte[128]; garbage[0] = 0x55; garbage[1] = 0x55; garbage[2] = 0x55; garbage[3] = 0x55; garbage[4] = 0x55;
        "GARBAGE BIN"u8.CopyTo(garbage.AsSpan(5));
        var data = new byte[128]; data[0] = 42; data[127] = 1;
        var blocks = new List<SectorBlock> { new(0, new(0,0,1), data), new(359, new(19,0,18), vtoc), new(360, new(20,0,1), directory), new(361, new(20,0,2), garbage) };
        for (var index=362; index<368; index++) blocks.Add(new(index,new(index/18,0,index%18+1),new byte[128]));
        var image = new SectorImage(DiskImageFormatIds.Atari90, 128, 40, 1, 18, blocks);
        var entries = new AtariDosFileSystemReader().Read(image).Entries;
        Assert.Equal("FILE.BIN", Assert.Single(entries).Name);
    }

    public static void RejectsFalseEmptyCatalogAndStopsBeforeReusedDirectorySectors()
    {
        var vtoc = new byte[128]; vtoc[0] = 2; vtoc[1] = 0xc3; vtoc[2] = 2; vtoc[3] = 0xc3; vtoc[4] = 2;
        var emptyDirectory = new byte[128];
        var blocks = new List<SectorBlock> { new(359, new(19, 0, 18), vtoc), new(360, new(20, 0, 1), emptyDirectory) };
        for (var index = 361; index < 368; index++) blocks.Add(new(index, new(index / 18, 0, index % 18 + 1), new byte[128]));
        var empty = new SectorImage(DiskImageFormatIds.Atari90, 128, 40, 1, 18, blocks);
        Assert.False(new AtariDosFileSystemReader().CanRead(empty));

        var directory = new byte[128]; directory[0] = 0x42; directory[1] = 1; directory[3] = 1; "FILE    BIN"u8.CopyTo(directory.AsSpan(5));
        var reused = Enumerable.Repeat((byte)0xa5, 128).ToArray();
        var data = new byte[128]; data[127] = 1;
        var occupiedBlocks = blocks.Select(block => block.LogicalBlock == 360 ? new SectorBlock(360, block.Address, directory) : block).Append(new SectorBlock(0, new(0, 0, 1), data)).ToList();
        occupiedBlocks[2] = new SectorBlock(361, occupiedBlocks[2].Address, reused);
        var protectedDisk = new SectorImage(DiskImageFormatIds.Atari90, 128, 40, 1, 18, occupiedBlocks);
        Assert.Equal("FILE.BIN", Assert.Single(new AtariDosFileSystemReader().Read(protectedDisk).Entries).Name);
    }

    public static void EmptyVtoc()
    {
        var blocks = new List<SectorBlock>();
        for (var index = 0; index < 368; index++) blocks.Add(new(index, new(index / 18, 0, index % 18 + 1), new byte[128]));
        var image = new SectorImage(DiskImageFormatIds.Atari90, 128, 40, 1, 18, blocks);
        Assert.False(new AtariDosFileSystemReader().CanRead(image));
    }

    public static void OpenEntry()
    {
        var vtoc = new byte[128]; vtoc[0] = 2; vtoc[1] = 0xf2; vtoc[2] = 3; vtoc[3] = 1;
        var directory = new byte[128]; directory[0] = 0x03; directory[1] = 1; directory[3] = 1;
        "OPEN    DAT"u8.CopyTo(directory.AsSpan(5));
        var data = new byte[128]; data[0] = 42; data[127] = 1;
        var blocks = new List<SectorBlock> { new(0, new(0,0,1), data), new(359, new(12,0,24), vtoc) };
        for (var index=360; index<368; index++) blocks.Add(new(index,new(12,0,index-359),index==360?directory:new byte[128]));
        var image = new SectorImage(DiskImageFormatIds.Atari140, 128, 40, 1, 28, blocks);
        var reader = new AtariDosFileSystemReader();
        Assert.True(reader.CanRead(image));
        var entry = Assert.Single(reader.Read(image).Entries);
        Assert.Equal("OPEN.DAT", entry.Name);
        Assert.False(entry.MetadataValid);
    }
}
