using System.Text;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Formats.Floppy.Apridisk;
using GWGUI.MediaEngine.FileSystems.Fat12;

namespace GWGUI.Tests.Media.ImageContainers;

internal static class ApridiskContainerScenarios
{
    public static async Task ReadContainer()
    {
        var data = CreateImage();
        var reader = new ApridiskReader((path, token) =>
        {
            Assert.Equal("apricot.dsk", path);
            token.ThrowIfCancellationRequested();
            return Task.FromResult(data);
        });

        var image = await reader.ReadAsync("apricot.dsk");
        Assert.Equal(DiskImageFormatIds.ApricotPcXi315, image.FormatId);
        Assert.Equal(70, image.Cylinders);
        Assert.Equal(1, image.Heads);
        Assert.Equal(9, image.SectorsPerTrack);
        Assert.Equal(630, image.AvailableBlocks.Count);
        Assert.Equal(512, image.GetBlock(0).Length);
        Assert.All(image.AvailableBlocks, block => Assert.True(block.IntegrityValid));
        var volume = new Fat12FileSystemReader().Read(image);
        var entry = Assert.Single(volume.Entries);
        Assert.Equal("HELLO.TXT", entry.Name);
        Assert.Equal(new byte[] { 0x41, 0x42, 0x43 }, entry.Content);

        data[0] = 0;
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync("apricot.dsk"));
    }

    internal static byte[] CreateImage()
    {
        var sectors = Enumerable.Range(0, ApridiskReader.BlockCount).Select(_ => new byte[ApridiskReader.BlockSize]).ToArray();
        new byte[]
        {
            0x56, 0x42, 0x20, 0x31, 0x2E, 0x33, 0x39, 0x35,
            0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02,
            0x09, 0x00, 0x46, 0x00, 0x00, 0x00, 0x01, 0x01,
            0x01, 0x00, 0x23, 0x00, 0x00, 0x00, 0x5A, 0x00
        }.CopyTo(sectors[0], 0);
        sectors[1][0] = 0xFC;
        sectors[1][1] = 0xFF;
        sectors[1][2] = 0xFF;
        sectors[1][3] = 0xFF;
        sectors[1][4] = 0x0F;
        Array.Copy(sectors[1], sectors[3], sectors[1].Length);
        "HELLO   TXT"u8.CopyTo(sectors[5]);
        sectors[5][11] = 0x20;
        sectors[5][26] = 2;
        sectors[5][28] = 3;
        sectors[9][0] = 0x41;
        sectors[9][1] = 0x42;
        sectors[9][2] = 0x43;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("ACT Apricot disk image\u001a\u0004"u8);
        writer.Write(new byte[ApridiskReader.HeaderSize - 24]);
        WriteRecord(writer, 0xE31D0000, 0, 0, 1, []);
        for (var logical = 0; logical < sectors.Length; logical++)
        {
            var cylinder = logical / ApridiskReader.SectorsPerTrack;
            var sector = logical % ApridiskReader.SectorsPerTrack + 1;
            WriteRecord(writer, 0xE31D0001, cylinder, 0, sector, sectors[logical]);
        }
        return stream.ToArray();
    }

    private static void WriteRecord(BinaryWriter writer, uint type, int cylinder, int head, int sector, byte[] data)
    {
        var compressed = data.Length == ApridiskReader.BlockSize && data.All(value => value == data[0]);
        var payload = compressed ? new byte[] { 0x00, 0x02, data[0] } : data;
        writer.Write(type);
        writer.Write(compressed ? (ushort)0x3E5A : (ushort)0x9E90);
        writer.Write((ushort)18);
        writer.Write((uint)payload.Length);
        writer.Write((byte)head);
        writer.Write((byte)sector);
        writer.Write((ushort)cylinder);
        writer.Write((ushort)0);
        writer.Write(payload);
    }
}
