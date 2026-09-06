using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Migration;
using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;
namespace GWGUI.Tests.Media.FileSystems;
internal static class Fat12FileSystemScenarios
{
    public static void Fragmented(int damage)
    {
        var blocks = new Dictionary<int, byte[]>();
        var boot = new byte[512]; blocks[0] = boot;
        void Word(int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(offset), value);
        Word(11,512); boot[13]=1; Word(14,1); boot[16]=1; Word(17,16); Word(19,320); boot[21]=0xfe; Word(22,1); Word(24,8); Word(26,1);
        var fat = new byte[512]; blocks[1]=fat; fat[0]=0xfe; fat[1]=fat[2]=0xff;
        void Link(int cluster,int value)
        {
            int offset=cluster*3/2;
            if(cluster%2==0) { fat[offset]=(byte)value; fat[offset+1]=(byte)((fat[offset+1]&0xf0)|(value>>8)); }
            else { fat[offset]=(byte)((fat[offset]&15)|(value<<4)); fat[offset+1]=(byte)(value>>4); }
        }
        Link(2,damage==1?2:damage==2?400:5); Link(3,0xfff); Link(5,0xfff);
        var root=new byte[512]; blocks[2]=root;
        void Entry(byte[] directory,int offset,string name,byte attr,ushort cluster,int size)
        {
            System.Text.Encoding.ASCII.GetBytes(name).CopyTo(directory,offset); directory[offset+11]=attr;
            BinaryPrimitives.WriteUInt16LittleEndian(directory.AsSpan(offset+26),cluster);
            BinaryPrimitives.WriteInt32LittleEndian(directory.AsSpan(offset+28),size);
        }
        Entry(root,0,"FILE    BIN",0,2,700); Entry(root,32,"DIR        ",16,3,0);
        var nested=new byte[512]; blocks[4]=nested; Entry(nested,0,"EMPTY   TXT",0,0,0);
        byte[] payload=Enumerable.Range(0,700).Select(i=>(byte)(i*7)).ToArray();
        blocks[3]=payload[..512]; blocks[6]=new byte[512]; payload.AsSpan(512).CopyTo(blocks[6]);
        if(damage==3) blocks.Remove(6);
        if(damage==4) blocks.Remove(1);
        var image=new SectorImage("ibm.160",512,40,1,8,blocks.Select(p=>new SectorBlock(p.Key,new(p.Key/8,0,p.Key%8),p.Value)));
        var reader=new Fat12FileSystemReader();
        if(damage==4) { Assert.False(reader.CanRead(image)); Assert.Throws<InvalidDataException>(()=>reader.Read(image)); return; }
        Assert.True(reader.CanRead(image)); var volume=reader.Read(image);
        Assert.Equal(2,volume.Entries.Count); Assert.Equal("DIR",volume.Entries[0].Name);
        var empty=Assert.Single(volume.Entries[0].Children); Assert.Equal("EMPTY.TXT",empty.Name); Assert.Empty(empty.Content!);
        var file=volume.Entries[1]; Assert.Equal("FILE.BIN",file.Name); Assert.Equal(700,file.Size); Assert.Equal(damage==0,file.MetadataValid);
        if(damage==0) { Assert.Equal(payload,file.Content); Assert.Empty(volume.Warnings); }
        else Assert.NotEmpty(volume.Warnings);
    }

    public static void Volume()
    {
        var payload=Enumerable.Range(0,700).Select(i=>(byte)(i%251)).ToArray();
        var entry=new MigrationEntry("TEST.BIN","TEST.BIN",FileSystemEntryKind.File,payload,null,"",0,true,[]);
        var plan=new MigrationPlan("synthetic","fat12","TEST",[entry]);
        var image=new Fat12VolumeWriter().Create(plan,"ibm.160");
        Assert.Equal(163840,image.Capacity);
        var boot=image.AvailableBlocks.Single(b=>b.LogicalBlock==0).Data.ToArray();
        Assert.Equal(512,BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(11)));
        Assert.Equal(320,BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(19)));
        Assert.Equal(8,BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(24)));
        var reader=new Fat12FileSystemReader();
        Assert.True(reader.CanRead(image));
        var volume=reader.Read(image);
        Assert.Equal("TEST",volume.Name);
        var file=Assert.Single(volume.Entries,e=>e.Kind==FileSystemEntryKind.File);
        Assert.Equal("TEST.BIN",file.Name);Assert.Equal(700,file.Size);Assert.Equal(payload,file.Content);
        Assert.True(volume.FreeBytes>0 && volume.FreeBytes<volume.Capacity);
        Assert.Throws<InvalidDataException>(()=>new Fat12VolumeWriter().Create(new("synthetic","fat12","TEST",[new("bad/name","bad/name",FileSystemEntryKind.File,[1],null,"",0,true,[])]),"ibm.160"));
        Assert.Throws<InvalidDataException>(()=>new Fat12VolumeWriter().Create(new("synthetic","fat12","TEST",[new("BIG.BIN","BIG.BIN",FileSystemEntryKind.File,new byte[200000],null,"",0,true,[])]),"ibm.160"));
    }
}
