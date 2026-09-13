using System.Text;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class VmdkImageSetTests
{
    [Theory]
    [InlineData(VmdkImageSetKind.VmfsFlat)] [InlineData(VmdkImageSetKind.VmfsSparse)]
    public void DescriptorReferencesEmittedExtent(VmdkImageSetKind kind)
    {
        var names=new List<string>();var contents=new Dictionary<string,byte[]>();
        VmdkImageSetWriter.Write(4L<<20,"disk",kind,(name,source)=>
        {
            names.Add(name);using var copy=new MemoryStream();source.CopyTo(copy);contents.Add(name,copy.ToArray());
        },content=>
        {
            using var formatted=new SparseMemoryStream();
            DiskImageBuilder.Write(formatted,new(4L<<20,"raw","none",[new(0,4L<<20,"fat12","DATA")]));
            formatted.Position=0;formatted.CopyTo(content);
        });
        Assert.Equal(2,names.Count);Assert.Equal("disk.vmdk",names[^1]);
        var descriptor=Encoding.ASCII.GetString(contents["disk.vmdk"]);
        Assert.Contains(names[0],descriptor);Assert.Contains("RW 8192",descriptor);
        if(kind==VmdkImageSetKind.VmfsFlat)
        {
            Assert.Contains("VMFS",descriptor);
            using var data=new MemoryStream(contents[names[0]]);
            using var fs=new DiscUtils.Fat.FatFileSystem(data);Assert.Equal("DATA",fs.VolumeLabel.Trim());
        }
        else
        {
            Assert.Contains("VMFSSPARSE",descriptor);
            Assert.True(contents[names[0]].Length>512);
        }
        // The pinned library keeps its FileLocator constructor internal; use it only in this
        // test to substitute memory streams for every descriptor/extent file access.
        using var layer=(DiscUtils.Vmdk.DiskImageFile)Activator.CreateInstance(typeof(DiscUtils.Vmdk.DiskImageFile),
            System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic,null,
            [new MemoryLocator(contents),"disk.vmdk",FileAccess.Read],null)!;
        using var logical=layer.OpenContent(null!,Ownership.None);
        using var reopened=new DiscUtils.Fat.FatFileSystem(logical);
        Assert.Equal("DATA",reopened.VolumeLabel.Trim());
    }
    [Fact]
    public void InvalidNameAndInitializerEmitNothing()
    {
        var emitted=0;
        Assert.ThrowsAny<ArgumentException>(()=>VmdkImageSetWriter.Write(4096,"../disk",VmdkImageSetKind.VmfsFlat,(_,_)=>emitted++));
        Assert.Throws<InvalidOperationException>(()=>VmdkImageSetWriter.Write(4096,"disk",VmdkImageSetKind.VmfsFlat,(_,_)=>emitted++,s=>s.SetLength(8192)));
        Assert.Equal(0,emitted);
    }

    internal sealed class MemoryLocator(IReadOnlyDictionary<string,byte[]> contents):DiscUtils.FileLocator
    {
        public override bool Exists(string name)=>contents.ContainsKey(Path.GetFileName(name));
        protected override Stream OpenFile(string name,FileMode mode,FileAccess access,FileShare share)
        {
            Assert.Equal(FileMode.Open,mode);Assert.Equal(FileAccess.Read,access);
            return new MemoryStream(contents[Path.GetFileName(name)],writable:false);
        }
        public override DiscUtils.FileLocator GetRelativeLocator(string path)=>this;
        public override string GetFullPath(string path)=>Path.GetFileName(path);
        public override string? GetDirectoryFromPath(string path)=>"";
        public override string GetFileFromPath(string path)=>Path.GetFileName(path);
        public override DateTime GetLastWriteTimeUtc(string path)=>DateTime.UnixEpoch;
        public override bool HasCommonRoot(DiscUtils.FileLocator other)=>ReferenceEquals(this,other);
        public override string ResolveRelativePath(string path)=>Path.GetFileName(path);
    }
}
