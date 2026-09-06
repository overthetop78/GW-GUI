using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Coherent;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
namespace GWGUI.Tests.Media.FileSystems;
internal static class CoherentFileSystemScenarios
{
    public static void Volume(int damage)
    {
        var bytes = new byte[3072];
        void Word(int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), value);
        void Canonical(int offset, uint value) { bytes[offset] = (byte)(value >> 16); bytes[offset+1] = (byte)(value >> 24); bytes[offset+2] = (byte)value; bytes[offset+3] = (byte)(value >> 8); }
        Word(512, 3); Canonical(514, 6); "noname"u8.CopyTo(bytes.AsSpan(996)); "nopack"u8.CopyTo(bytes.AsSpan(1002));
        Word(1088, 0x4000); Canonical(1096, 32); bytes[1101] = 3; // Root inode 2, directory in block 3.
        Word(1152, 0x81a4); Canonical(1160, 700); bytes[1165] = damage == 3 ? (byte)255 : (byte)4; bytes[1168] = 5;
        Word(1216, 0x8000); // Empty inode 4.
        Word(1536, 3); "FILE"u8.CopyTo(bytes.AsSpan(1538)); Word(1552, damage == 2 ? (ushort)2 : (ushort)4); (damage == 2 ? "LOOP"u8 : "EMPTY"u8).CopyTo(bytes.AsSpan(1554));
        var payload = Enumerable.Range(0,700).Select(i => (byte)(i % 251)).ToArray(); payload.CopyTo(bytes, 2048);
        var blocks = Enumerable.Range(0,6).Where(index => damage != 1 || index != 5).Select(index => new SectorBlock(index, new(0,0,index), bytes.AsSpan(index*512,512).ToArray()));
        var image = new SectorImage(DiskImageFormatIds.Commodore900Coherent,512,1,1,6,blocks); var reader = new CoherentFileSystemReader(); Assert.True(reader.CanRead(image));
        var volume = reader.Read(image); Assert.Empty(volume.Name); Assert.Equal(3072, volume.Capacity); Assert.Equal(2, volume.Entries.Count);
        var file = Assert.Single(volume.Entries, entry => entry.Name == "FILE"); Assert.Equal(700, file.Size); Assert.Equal((uint)0x1a4,file.RawAttributes);
        Assert.Equal(damage is 0 or 2, file.MetadataValid); if (damage is 0 or 2) Assert.Equal(payload,file.Content);
        if (damage == 2) { var loop = Assert.Single(volume.Entries, entry => entry.Name == "LOOP"); Assert.False(loop.MetadataValid); Assert.Empty(loop.Children); }
        else Assert.Empty(Assert.Single(volume.Entries, entry => entry.Name == "EMPTY").Content!);
        if (damage == 0) Assert.Empty(volume.Warnings); else Assert.NotEmpty(volume.Warnings);
        var missingSuperblock = new SectorImage(image.FormatId,512,1,1,6,image.AvailableBlocks.Where(block => block.LogicalBlock != 1));
        Assert.Throws<InvalidDataException>(() => reader.Read(missingSuperblock));
    }
}
