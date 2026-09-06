using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;
namespace GWGUI.Tests.Media.FileSystems;
internal static class CommodoreFileSystemScenarios
{
    public static void Chains(string format, int damage)
    {
        byte[] payload=Enumerable.Range(0,700).Select(i=>(byte)(i*7)).ToArray();
        var original=new CommodoreDosVolumeWriter().Create(new("synthetic","commodore-dos","TEST",[new("FILE","FILE",FileSystemEntryKind.File,payload,null,"",0,true,[])]),format);
        var blocks=original.AvailableBlocks.ToDictionary(b=>b.LogicalBlock,b=>b.Data.ToArray());
        int At(int track,int sector)=>CommodoreDosGeometry.ToLogicalBlock(original,track,sector);
        var directory=blocks[At(format=="commodore.1581"?40:18,format=="commodore.1581"?3:1)];
        directory[3]=1; directory[4]=0;
        foreach(var (sector,index) in new[]{(0,0),(5,1),(9,2)})
        {
            var data=new byte[256]; blocks[At(1,sector)]=data;
            data[0]=index==2?(byte)0:(byte)1; data[1]=index==0?(byte)5:index==1?(byte)9:(byte)193;
            payload.AsSpan(index*254,Math.Min(254,700-index*254)).CopyTo(data.AsSpan(2));
        }
        if(damage==1) { blocks[At(1,5)][0]=1; blocks[At(1,5)][1]=0; }
        if(damage==2) blocks[At(1,5)][0]=250;
        if(damage==3) blocks.Remove(At(1,9));
        if(damage==4) blocks[At(1,9)][1]=0;
        var image=new SectorImage(format,256,original.Cylinders,original.Heads,original.SectorsPerTrack,
            blocks.Select(p=>new SectorBlock(p.Key,new(0,0,0),p.Value)),capacity:original.Capacity,logicalBlockCount:original.BlockCount);
        var reader=new CommodoreDosFileSystemReader(); Assert.True(reader.CanRead(image)); var volume=reader.Read(image);
        var file=Assert.Single(volume.Entries); Assert.Equal("FILE",file.Name); Assert.Equal(damage==0,file.MetadataValid);
        if(damage==0) { Assert.Equal(700,file.Size); Assert.Equal(payload,file.Content); Assert.Empty(volume.Warnings); }
        else { Assert.NotEmpty(volume.Warnings); Assert.Equal(payload.Take(508),file.Content); }
    }

    public static void Volume(string format, long capacity)
    {
        var payload = Enumerable.Range(0, 700).Select(i => (byte)(i % 251)).ToArray();
        var file = new MigrationEntry("FILE", "FILE", FileSystemEntryKind.File, payload, null, "", 0, true, []);
        var empty = new MigrationEntry("EMPTY", "EMPTY", FileSystemEntryKind.File, [], null, "", 0, true, []);
        var plan = new MigrationPlan("synthetic", "commodore-dos", "VOLUME", [file, empty]);
        var writer = new CommodoreDosVolumeWriter(); var image = writer.Create(plan, format); Assert.Equal(capacity, image.Capacity); Assert.Equal(256, image.BlockSize);
        var reader = new CommodoreDosFileSystemReader(); Assert.True(reader.CanRead(image)); var volume = reader.Read(image);
        Assert.Equal("VOLUME", volume.Name); Assert.Equal(2, volume.Entries.Count);
        var actual = Assert.Single(volume.Entries, x => x.Name == "FILE"); Assert.Equal(700, actual.Size); Assert.Equal(payload, actual.Content); Assert.True(actual.MetadataValid);
        var actualEmpty = Assert.Single(volume.Entries, x => x.Name == "EMPTY"); Assert.Equal(0, actualEmpty.Size); Assert.Empty(actualEmpty.Content!);
        Assert.True(volume.FreeBytes > 0 && volume.FreeBytes < image.Capacity); Assert.Empty(volume.Warnings);
        Assert.Throws<InvalidDataException>(() => writer.Create(new("synthetic", "commodore-dos", "VOLUME", [new MigrationEntry("DIR", "DIR", FileSystemEntryKind.Directory, [], null, "", 0, true, [file])]), format));
        Assert.Throws<InvalidDataException>(() => writer.Create(new("synthetic", "commodore-dos", "VOLUME", [new MigrationEntry("BIG", "BIG", FileSystemEntryKind.File, new byte[capacity], null, "", 0, true, [])]), format));
    }
}
