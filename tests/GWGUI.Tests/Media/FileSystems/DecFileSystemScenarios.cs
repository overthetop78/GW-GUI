using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Dec.Rt11;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
namespace GWGUI.Tests.Media.FileSystems;
internal static class DecFileSystemScenarios
{
    public static void Rt11(int damage)
    {
        var home = new byte[512]; "DECRT11"u8.CopyTo(home.AsSpan(496)); "VOLUME"u8.CopyTo(home.AsSpan(472)); BinaryPrimitives.WriteUInt16LittleEndian(home.AsSpan(468), 6);
        var directory = new byte[1024];
        void Word(int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(directory.AsSpan(offset), value);
        Word(0, 1); Word(8, 8); Word(10, 0x8400); Word(12, 1600); Word(16, 3574); Word(18, 1);
        Word(24, 0x200); Word(32, 2); Word(38, 0x800);
        if (damage == 1) Word(2, 1); // Segment links to itself.
        if (damage == 2) Word(2, 32); // Segment outside the allowed range.
        if (damage == 3) Word(6, 200); // Impossible directory entry size.
        if (damage == 6) Word(18, 0); // Valid empty file consumes no data blocks.
        var payload = new byte[512]; payload[0] = 42; payload[^1] = 93;
        var blocks = new List<SectorBlock> { new(1, new(0,0,1), home), new(6, new(0,0,6), directory[..512]), new(7, new(0,0,7), directory[512..]) };
        if (damage != 4) blocks.Add(new(8, new(0,0,8), payload));
        if (damage == 5) blocks.RemoveAt(2);
        var image = new SectorImage(DiskImageFormatIds.DecRx02, 512, 1, 1, 12, blocks); var reader = new Rt11FileSystemReader(); Assert.True(reader.CanRead(image));
        var volume = reader.Read(image); Assert.Equal("VOLUME", volume.Name);
        if (damage is not (3 or 5))
        {
            var file = Assert.Single(volume.Entries); Assert.Equal("A.BIN", file.Name); Assert.Equal(damage == 6 ? 0 : 512, file.Size); Assert.Equal((uint)1, file.RawAttributes);
            Assert.Equal(damage != 4, file.MetadataValid); if (damage != 4) Assert.Equal(damage == 6 ? [] : payload, file.Content);
        }
        else Assert.Empty(volume.Entries);
        Assert.Equal(damage is 0 or 6 ? 1024 : 0, volume.FreeBytes);
        if (damage is 0 or 6) Assert.Empty(volume.Warnings); else Assert.NotEmpty(volume.Warnings);
        home[496] = 0;
        var invalid = new SectorImage(image.FormatId, 512, 1, 1, 12, [new(1, new(0,0,1), home)]);
        Assert.False(reader.CanRead(invalid)); Assert.Throws<InvalidDataException>(() => reader.Read(invalid));
    }
}
