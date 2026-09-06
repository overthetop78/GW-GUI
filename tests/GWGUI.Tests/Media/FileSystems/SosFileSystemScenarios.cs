using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Apple.ProDos;
using GWGUI.MediaEngine.FileSystems.Sos;
using GWGUI.MediaEngine.Migration;
namespace GWGUI.Tests.Media.FileSystems;
internal static class SosFileSystemScenarios
{
    public static void Volume()
    {
        var plan=new MigrationPlan("synthetic","sos","TEST",[new("FILE","FILE",FileSystemEntryKind.File,new byte[]{42,93},null,"",0,true,[])]);
        var image=new SosVolumeWriter().Create(plan);
        Assert.Equal(143360,image.Capacity);
        Assert.Equal(GWGUI.MediaEngine.Definitions.DiskImageFormatIds.AppleIIISos,image.FormatId);
        Assert.Equal(new byte[]{83,79,83},image.AvailableBlocks.Single(block=>block.LogicalBlock==0).Data.Skip(8).Take(3));
        var reader=new ProDosFileSystemReader();
        Assert.True(reader.CanRead(image));
        var file=Assert.Single(reader.Read(image).Entries);
        Assert.Equal("FILE",file.Name);Assert.Equal(new byte[]{42,93},file.Content);
    }
}
