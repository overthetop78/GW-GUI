using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Conversion.Migration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaFileSystems.Definitions;

namespace GWGUI.Tests.Media;

public sealed class CommodoreDosMigrationImageTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.Commodore1541)]
    [InlineData(DiskImageFormatIds.Commodore1571)]
    [InlineData(DiskImageFormatIds.Commodore1581)]
    public void MigratedFileCanBeReadFromCommodoreDosImage(string formatId)
    {
        var plan = new MigrationPlan(
            FileSystemIds.Fat12,
            FileSystemIds.CommodoreDos,
            "VOLUME",
            [new MigrationEntry("/HELLO", "HELLO", FileSystemEntryKind.File,
                new byte[] { 1, 2, 3 }, null, string.Empty, 0, true, [])]);

        var image = new CommodoreDosMigrationImageBuilder().Create(plan, formatId);
        var reader = Assert.Single(FileSystemReaderCatalog.CreateDefault(), item => item.Id == FileSystemIds.CommodoreDos);
        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Equal("VOLUME", volume.Name);
        var file = Assert.Single(volume.Entries);
        Assert.Equal("HELLO", file.Name);
        Assert.Equal(new byte[] { 1, 2, 3 }, Assert.IsAssignableFrom<IReadOnlyList<byte>>(file.Content));
    }
}
