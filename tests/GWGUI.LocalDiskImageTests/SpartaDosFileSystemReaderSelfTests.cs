using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Exploration;
using GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

namespace GWGUI.MediaAudit;

internal static class SpartaDosFileSystemReaderSelfTests
{
    private const int SectorSize = 128;
    private const int SectorCount = 64;
    private const int RootMapSector = 5;
    private const int RootDataSector = 6;
    private const int FileMapSector = 7;
    private const int FileDataSector = 8;
    private const int DirectoryMapSector = 9;
    private const int DirectoryDataSector = 10;
    private const int SparseMapSector = 11;
    private const int SparseDataSector = 12;

    internal static void Run()
    {
        var image = CreateImage();
        var registry = new SectorFileSystemRegistry([new SpartaDosFileSystemReader()]);
        var candidates = registry.ReadDistinctCandidates([image]);
        if (candidates.Count != 1) throw new InvalidOperationException("A dynamic ATR identifier must still be probed for a SpartaDOS structure.");
        var volume = candidates[0].Match.Volume;
        if (volume.FileSystemId != FileSystemIds.AtariSpartaDos || volume.Name != "TESTVOL" || volume.FreeBytes != 50 * SectorSize)
            throw new InvalidOperationException("The SpartaDOS header must provide the file-system id, volume name and free space.");
        if (volume.Entries.Count != 2) throw new InvalidOperationException("The SpartaDOS root directory must expose its file and subdirectory.");
        var file = volume.Entries.Single(entry => entry.Name == "HELLO.TXT");
        if (file.Content is null || !file.Content.SequenceEqual(new byte[] { 0x41, 0x42, 0x43 }))
            throw new InvalidOperationException("A regular SpartaDOS file must be reconstructed from its sector map.");
        var directory = volume.Entries.Single(entry => entry.Name == "DATA");
        var sparse = directory.Children.Single(entry => entry.Name == "SPARSE.BIN");
        if (sparse.Content is null || sparse.Content.Count != SectorSize * 2 || sparse.Content[0] != 0x5a || sparse.Content.Skip(SectorSize).Any(value => value != 0))
            throw new InvalidOperationException("A sparse SpartaDOS file must preserve allocated data and zero-fill its unallocated sector.");
        if (!sparse.Attributes.Contains("sparse"))
            throw new InvalidOperationException("A sparse SpartaDOS file must expose its sparse attribute.");
    }

    private static SectorImage CreateImage()
    {
        var boot = new byte[SectorSize];
        boot[7] = 0x80;
        WriteUInt16(boot, 9, RootMapSector);
        WriteUInt16(boot, 11, SectorCount);
        WriteUInt16(boot, 13, 50);
        boot[15] = 1;
        WriteUInt16(boot, 16, 4);
        WriteUInt16(boot, 18, FileDataSector);
        WriteUInt16(boot, 20, RootDataSector);
        Encoding.ASCII.GetBytes("TESTVOL ").CopyTo(boot, 22);
        boot[30] = 1;
        boot[31] = SectorSize;
        boot[32] = 0x20;
        WriteUInt16(boot, 40, FileMapSector);

        var root = new byte[SectorSize];
        WriteLength(root, 3, 69);
        Encoding.ASCII.GetBytes("MAIN    ").CopyTo(root, 6);
        WriteEntry(root, 23, 0x08, FileMapSector, 3, "HELLO", "TXT");
        WriteEntry(root, 46, 0x28, DirectoryMapSector, 46, "DATA", string.Empty);

        var child = new byte[SectorSize];
        WriteLength(child, 3, 46);
        Encoding.ASCII.GetBytes("DATA    ").CopyTo(child, 6);
        WriteEntry(child, 23, 0x08, SparseMapSector, SectorSize * 2, "SPARSE", "BIN");

        var sparseData = Enumerable.Repeat((byte)0x5a, SectorSize).ToArray();
        return new(
            "atari.atr.128.64",
            SectorSize,
            1,
            1,
            SectorCount,
            [
                Block(1, boot),
                Block(4, new byte[SectorSize]),
                Block(RootMapSector, Map(RootDataSector)),
                Block(RootDataSector, root),
                Block(FileMapSector, Map(FileDataSector)),
                Block(FileDataSector, [0x41, 0x42, 0x43, .. new byte[SectorSize - 3]]),
                Block(DirectoryMapSector, Map(DirectoryDataSector)),
                Block(DirectoryDataSector, child),
                Block(SparseMapSector, Map(SparseDataSector, 0)),
                Block(SparseDataSector, sparseData)
            ],
            capacity: SectorCount * SectorSize,
            logicalBlockCount: SectorCount,
            addressingKind: SectorImageAddressingKind.Logical);
    }

    private static byte[] Map(params int[] dataSectors)
    {
        var map = new byte[SectorSize];
        for (var index = 0; index < dataSectors.Length; index++)
            WriteUInt16(map, 4 + index * sizeof(ushort), dataSectors[index]);
        return map;
    }

    private static void WriteEntry(byte[] directory, int offset, byte flags, int mapSector, int length, string name, string extension)
    {
        directory[offset] = flags;
        WriteUInt16(directory, offset + 1, mapSector);
        WriteLength(directory, offset + 3, length);
        Encoding.ASCII.GetBytes(name.PadRight(8)).CopyTo(directory, offset + 6);
        Encoding.ASCII.GetBytes(extension.PadRight(3)).CopyTo(directory, offset + 14);
        directory[offset + 17] = 1;
        directory[offset + 18] = 1;
        directory[offset + 19] = 94;
    }

    private static void WriteLength(byte[] data, int offset, int value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
    }

    private static void WriteUInt16(byte[] data, int offset, int value) => BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset, sizeof(ushort)), checked((ushort)value));

    private static SectorBlock Block(int sectorNumber, IReadOnlyList<byte> data) => new(sectorNumber - 1, new SectorAddress(0, 0, sectorNumber), data);
}
