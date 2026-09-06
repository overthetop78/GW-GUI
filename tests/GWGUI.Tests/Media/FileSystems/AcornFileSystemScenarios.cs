using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using GWGUI.MediaEngine.FileSystems.Acorn.Adfs;
using GWGUI.MediaEngine.FileSystems.Acorn.BbcDfs;
using GWGUI.MediaEngine.SectorImages;
namespace GWGUI.Tests.Media.FileSystems;
internal static class AcornFileSystemScenarios
{
    public static void Adfs(bool newMap, int damage)
    {
        var blocks = new Dictionary<int, byte[]>();
        var map = new byte[1024]; blocks[0] = map;
        int rootBlock = newMap ? 2 : 1, fileBlock = newMap ? 4 : 10;
        int rootAddress = newMap ? 0x200 : 4, fileAddress = newMap ? 0x300 : 40;
        if (newMap)
        {
            map[4] = 10; map[8] = 15; map[9] = 7; map[13] = 1;
            BinaryPrimitives.WriteInt32LittleEndian(map.AsSpan(16), rootAddress);
            BinaryPrimitives.WriteInt32LittleEndian(map.AsSpan(20), 819200);
            "TEST"u8.CopyTo(map.AsSpan(26));
            // Five 16-bit fragments, with file 3 split around unrelated fragment 4.
            foreach (var (id, index) in new[] { (1, 0), (2, 1), (3, 2), (4, 3), (3, 4) })
                BinaryPrimitives.WriteUInt16LittleEndian(map.AsSpan(64 + index * 2), (ushort)(0x8000 | id));
        }
        else
        {
            map[247] = (byte)'T'; map[502] = (byte)'E'; map[248] = (byte)'S'; map[503] = (byte)'T'; map[256] = 10;
        }
        var root = Directory();
        Entry(root, 0, "FILE", damage == 4 ? 0x7fffff : fileAddress, 3000);
        Entry(root, 1, "EMPTY", fileAddress, 0);
        Entry(root, 2, "DIR", rootAddress, 0, true);
        if (damage == 3) root[2043] = 0;
        blocks[rootBlock] = root[..1024]; blocks[rootBlock + 1] = root[1024..];
        byte[] expected = Enumerable.Range(0, 3000).Select(i => (byte)(i * 7)).ToArray();
        for (int i = 0; i < 3; i++)
        {
            var data = new byte[1024]; expected.AsSpan(i * 1024, Math.Min(1024, expected.Length - i * 1024)).CopyTo(data);
            blocks[newMap && i == 2 ? 8 : fileBlock + i] = data;
        }
        if (damage == 1) blocks.Remove(newMap ? 8 : 12);
        if (damage == 2) blocks.Remove(rootBlock + 1);
        var image = new SectorImage(DiskImageFormatIds.AcornAdfs800, 1024, 80, 2, 5,
            blocks.Select(pair => new SectorBlock(pair.Key, new(pair.Key / 10, pair.Key / 5 % 2, pair.Key % 5), pair.Value)));
        var reader = new AcornAdfsFileSystemReader();
        if (damage is 2 or 3)
        {
            Assert.False(reader.CanRead(image)); Assert.Throws<InvalidDataException>(() => reader.Read(image)); return;
        }
        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Equal("TEST", volume.Name); Assert.Equal(819200, volume.Capacity);
        Assert.Equal(newMap ? 0 : 2560, volume.FreeBytes);
        Assert.Equal(3, volume.Entries.Count);
        var file = Assert.Single(volume.Entries, entry => entry.Name == "FILE");
        Assert.Equal(3000, file.Size); Assert.Equal(damage == 0, file.MetadataValid);
        if (damage == 0) Assert.Equal(expected, file.Content); else Assert.Null(file.Content);
        Assert.Empty(Assert.Single(volume.Entries, entry => entry.Name == "EMPTY").Content!);
        Assert.Empty(Assert.Single(volume.Entries, entry => entry.Name == "DIR").Children);
        Assert.NotEmpty(volume.Warnings); // Directory cycle is reported without recursing forever.
    }

    private static byte[] Directory()
    {
        var bytes = new byte[2048]; "Hugo"u8.CopyTo(bytes.AsSpan(1)); "Hugo"u8.CopyTo(bytes.AsSpan(2043));
        "ROOT"u8.CopyTo(bytes.AsSpan(2032)); return bytes;
    }

    private static void Entry(byte[] directory, int index, string name, int address, int length, bool folder = false)
    {
        int offset = 5 + index * 26;
        System.Text.Encoding.ASCII.GetBytes(name).CopyTo(directory, offset);
        BinaryPrimitives.WriteInt32LittleEndian(directory.AsSpan(offset + 18), length);
        directory[offset + 22] = (byte)address; directory[offset + 23] = (byte)(address >> 8); directory[offset + 24] = (byte)(address >> 16);
        directory[offset + 25] = folder ? (byte)8 : (byte)0;
    }

    public static void Dfs(bool missing)
    {
        var names = new byte[256]; "VOLUME  "u8.CopyTo(names); "FILE   "u8.CopyTo(names.AsSpan(8)); names[15] = (byte)'$';
        var metadata = new byte[256]; metadata[5] = 8; metadata[6] = 1; metadata[7] = 144; metadata[12] = 2; metadata[15] = 2;
        var content = new byte[256]; content[0] = 42; content[1] = 93;
        var blocks = new List<SectorBlock> { new(0, new(0,0,0), names), new(1, new(0,0,1), metadata) };
        if (!missing) blocks.Add(new(2, new(0,0,2), content));
        var image = new SectorImage(DiskImageFormatIds.AcornDfsSingleSided, 256, 40, 1, 10, blocks);
        var reader = new BbcDfsFileSystemReader(); Assert.True(reader.CanRead(image));
        var volume = reader.Read(image); var file = Assert.Single(volume.Entries);
        Assert.Equal("VOLUME", volume.Name); Assert.Equal("FILE", file.Name); Assert.Equal(2, file.Size);
        Assert.Equal(!missing, file.MetadataValid);
        if (missing) Assert.NotEmpty(volume.Warnings); else { Assert.Empty(volume.Warnings); Assert.Equal(new byte[] { 42, 93 }, file.Content); }
        Assert.Equal(102400, volume.Capacity); Assert.Equal(397 * 256, volume.FreeBytes);
    }
}
