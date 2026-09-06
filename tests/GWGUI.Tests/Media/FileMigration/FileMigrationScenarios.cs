using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.Containers.Adf;
using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Containers.Msx.Raw;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Conversion.Fat12;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.FileSystems.Apple.ProDos;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.MediaEngine.Containers.Commodore;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.FileSystems.Apple.Dos;
using GWGUI.MediaEngine.SectorImages;
namespace GWGUI.Tests.Media.FileMigration;
internal static class FileMigrationScenarios
{
    private static FileSystemVolume Source()
    {
        var payload=Enumerable.Range(0,700).Select(i=>(byte)(i%251)).ToArray();
        var file=new MigrationEntry("DIR/FILE.BIN","FILE.BIN",FileSystemEntryKind.File,payload,null,"",0,true,[]);
        var directory=new MigrationEntry("DIR","DIR",FileSystemEntryKind.Directory,null,null,"",0,true,[file]);
        var image=new Fat12VolumeWriter().Create(new("synthetic","fat12","VOLUME",[directory]),"ibm.160");
        return new Fat12FileSystemReader().Read(image);
    }
    private static FileSystemMigrationService Service(MemoryImageFiles files)
    {
        var linear=new LinearSectorImageWriter(files);
        return new(new Fat12AmigaDosMigrationService(new AmigaAdfWriter(linear),new Fat12TargetImageWriter(new AtariStWriter(linear),new IbmRawImageWriter(linear),new MsxRawImageWriter(linear))),
            new AppleFileSystemMigrationService(new AppleRawImageWriter(files),new TwoImgWriter(files),new AppleDiskImageWriter()),
            new CommodoreDosMigrationService(new CommodoreDosContainerWriter(files),new D81Writer(linear)));
    }
    private static FileSystemVolume Flat(int length = 700) => new("VOLUME", "synthetic", length, 0, null, null,
        [new("FILE", FileSystemEntryKind.File, length, null, "", 0, 0, true, [], Enumerable.Range(0,length).Select(i=>(byte)(i%251)).ToArray()),
         new("EMPTY", FileSystemEntryKind.File, 0, null, "", 0, 0, true, [], [])], []);

    public static async Task CatalogTarget(string format, string extension)
    {
        var source = Flat(); var files = new MemoryImageFiles(); var service = Service(files);
        if (extension == ".do") source = new("DOS-007", source.FileSystemId, 700, 0, null, null, source.Entries, []);
        var path = "virtual" + extension;
        Assert.True(service.Validate(source, format, true).CanExecute);
        Assert.True((await service.WriteAsync(source, path, format, true)).CanExecute);
        Task<byte[]> Bytes(string requested, CancellationToken token) { token.ThrowIfCancellationRequested(); return Task.FromResult(files.Files[requested]); }
        SectorImage image = extension switch
        {
            ".adf" => await new AdfReader(Bytes).ReadAsync(path),
            ".st" => await new AtariStReader(Bytes).ReadAsync(path),
            ".img" => await new IbmRawImageReader(Bytes).ReadAsync(path),
            ".dsk" => await new MsxRawImageReader(Bytes).ReadAsync(path),
            ".d64" => await new D64Reader(Bytes).ReadAsync(path),
            ".d71" => await new D71Reader(Bytes).ReadAsync(path),
            ".d81" => await new D81Reader(Bytes).ReadAsync(path),
            ".2mg" => TwoImgReader.Read(files.Files[path]),
            _ => AppleRawImageReader.Read(files.Files[path], extension).Image
        };
        IFileSystemReader reader = extension switch
        {
            ".adf" => new AmigaDosFileSystemReader(),
            ".st" or ".img" or ".dsk" => new Fat12FileSystemReader(),
            ".d64" or ".d71" or ".d81" => new CommodoreDosFileSystemReader(),
            ".do" => new AppleDosFileSystemReader(),
            _ => new ProDosFileSystemReader()
        };
        var volume = reader.Read(image);
        Assert.Equal(source.Name, volume.Name);
        Assert.Equal(2, volume.Entries.Count);
        Assert.Equal(source.Entries[0].Content, Assert.Single(volume.Entries,e=>e.Name=="FILE").Content);
        Assert.Empty(Assert.Single(volume.Entries,e=>e.Name=="EMPTY").Content!);
        if (extension == ".do")
        {
            Assert.Contains(service.Validate(volume, "ibm.720", true).Losses, loss=>loss.Kind==MigrationLossKind.InvalidName && loss.Path=="/");
            volume = new("VOLUME", volume.FileSystemId, volume.Capacity, volume.FreeBytes, null, null, volume.Entries, []);
        }
        await service.WriteAsync(volume, "returned.img", "ibm.720", true);
        var returned = new Fat12FileSystemReader().Read(await new IbmRawImageReader(Bytes).ReadAsync("returned.img"));
        Assert.Equal(source.Entries[0].Content, Assert.Single(returned.Entries,e=>e.Name=="FILE").Content);
        Assert.Equal(source.Entries[0].Content, Flat().Entries[0].Content);
    }
    public static async Task Rejected(int failure)
    {
        var source = failure == 0 ? Flat(200000) : Flat();
        if (failure == 1) source = new(source.Name, source.FileSystemId, 1000, 0, null, null, [source.Entries[0], source.Entries[0]], []);
        var files = new MemoryImageFiles(); files.Files["virtual.po"] = [42,93]; var service = Service(files);
        if (failure == 1)
        {
            var report = service.Validate(source, "apple2.prodos.140", true);
            Assert.False(report.CanExecute); Assert.Contains(report.Losses, loss=>loss.Kind==MigrationLossKind.NameCollision);
        }
        if(failure==0) await Assert.ThrowsAsync<InvalidDataException>(()=>service.WriteAsync(source,"virtual.po","apple2.prodos.140",true));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>service.WriteAsync(source,"virtual.po","apple2.prodos.140",true));
        Assert.Empty(files.Calls); Assert.Equal(new byte[]{42,93},files.Files["virtual.po"]);
    }
    public static async Task Transfer(bool apple)
    {
        var source=Source(); var files=new MemoryImageFiles(); var service=Service(files);
        var target=apple?"apple2.prodos.140":"amiga.amigados"; var path=apple?"virtual.po":"virtual.adf";
        var report=await service.WriteAsync(source,path,target,acceptMetadataLoss:true); Assert.True(report.CanExecute); Assert.Equal(path,Assert.Single(files.Calls));
        FileSystemVolume volume;
        if(apple)
        {
            var bytes=files.Files[path]; Assert.Equal(143360,bytes.Length);
            var image=new GWGUI.MediaEngine.SectorImages.SectorImage(target,512,35,1,8,Enumerable.Range(0,280).Select(index=>new GWGUI.MediaEngine.SectorImages.SectorBlock(index,new(index/8,0,index%8),bytes.AsSpan(index*512,512).ToArray())));
            volume=new ProDosFileSystemReader().Read(image);
        }
        else volume=new AmigaDosFileSystemReader().Read(await new AdfReader((requested,token)=>{ Assert.Equal(path,requested); token.ThrowIfCancellationRequested(); return Task.FromResult(files.Files[requested]); }).ReadAsync(path));
        Assert.Equal("VOLUME",volume.Name); var directory=Assert.Single(volume.Entries); Assert.Equal("DIR",directory.Name);
        var migrated=Assert.Single(directory.Children); Assert.Equal("FILE.BIN",migrated.Name); Assert.Equal(700,migrated.Size);
        Assert.Equal(Assert.Single(Assert.Single(source.Entries).Children).Content,migrated.Content);
        await service.WriteAsync(volume,"returned.img","ibm.720",acceptMetadataLoss:true);
        var returned=new Fat12FileSystemReader().Read(await new IbmRawImageReader((requested,token)=>{token.ThrowIfCancellationRequested();return Task.FromResult(files.Files[requested]);}).ReadAsync("returned.img"));
        Assert.Equal(migrated.Content,Assert.Single(Assert.Single(returned.Entries).Children).Content);
    }
    public static async Task Failure(int failure)
    {
        var source=Source(); var files=new MemoryImageFiles(); files.Files["virtual.adf"]=[42,93]; var service=Service(files);
        using var cancellation=new CancellationTokenSource();
        if(failure==0) cancellation.Cancel(); else files.FailCommit=true;
        if(failure==0) await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>service.WriteAsync(source,"virtual.adf","amiga.amigados",true,cancellation.Token));
        else await Assert.ThrowsAsync<IOException>(()=>service.WriteAsync(source,"virtual.adf","amiga.amigados",true,cancellation.Token));
        Assert.Equal(new byte[]{42,93},files.Files["virtual.adf"]); Assert.Single(files.Files);
        Assert.Equal(700,Assert.Single(Assert.Single(source.Entries).Children).Content!.Count);
        files.FailCommit=false; await service.WriteAsync(source,"virtual.adf","amiga.amigados",true); Assert.Equal(901120,files.Files["virtual.adf"].Length);
    }
    public static void Plan()
    {
        var bytes=new byte[]{42,93};
        var child=new FileSystemEntry("FILE",FileSystemEntryKind.File,2,null,"",0,0,true,[],bytes);
        var folder=new FileSystemEntry("DIR",FileSystemEntryKind.Directory,0,null,"",0,0,true,[child]);
        var volume=new FileSystemVolume("TEST","synthetic",100,90,null,null,[folder],[]);
        var plan=MigrationPlanner.Create(volume,"target");
        var nested=Assert.Single(Assert.Single(plan.Entries).Children);
        Assert.Equal("DIR/FILE",nested.SourcePath);Assert.Equal("FILE",nested.TargetName);
        bytes[0]=0;
        Assert.Equal(new byte[]{42,93},nested.Content);
    }
    public static void Losses()
    {
        var entry=new MigrationEntry("file","file",FileSystemEntryKind.File,[42],null,"comment",0,true,[]);
        var target=new MigrationTargetCapabilities("target",8,10,true,false,false,false,false,false,"/");
        var plan=new MigrationPlan("synthetic","target","TEST",[entry]);
        var unaccepted=MigrationValidator.Validate(plan,target);
        Assert.False(unaccepted.CanExecute);
        Assert.Equal(MigrationLossKind.Comment,Assert.Single(unaccepted.Losses).Kind);
        Assert.True(MigrationValidator.Validate(plan,target,true).CanExecute);
        var collision=new MigrationPlan("synthetic","target","TEST",[entry,entry]);
        Assert.False(MigrationValidator.Validate(collision,target,true).CanExecute);
        Assert.Throws<InvalidOperationException>(()=>MigrationValidator.EnsureExecutable(MigrationValidator.Validate(collision,target,true)));
    }
}
