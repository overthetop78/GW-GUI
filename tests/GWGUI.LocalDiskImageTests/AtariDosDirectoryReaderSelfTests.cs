using System.Buffers.Binary;
using System.Text;

using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

namespace GWGUI.MediaAudit;

internal static class AtariDosDirectoryReaderSelfTests
{
    private const int TestBlockCount = 720;
    private const int TestSectorSize = AtariDosFileSystemLayout.MinimumSectorSize;
    private const int TestCylinders = 40;
    private const int TestHeads = 1;
    private const int TestSectorsPerTrack = 18;
    private const int OpenEntryNumber = 0;
    private const int FinalizedEntryNumber = 1;
    private const int DataSectorNumber = 136;
    private const int PayloadLength = 3;
    private const string FileName = "COMPOSE";
    private const string FileExtension = "OBJ";

    internal static void Run()
    {
        var directory = new byte[TestSectorSize];
        WriteDirectoryEntry(directory, OpenEntryNumber,
            AtariDosDirectoryFlags.InUse | AtariDosDirectoryFlags.CreatedByDos2 | AtariDosDirectoryFlags.OpenForOutput,
            sectorCount: 0, DataSectorNumber);
        WriteDirectoryEntry(directory, FinalizedEntryNumber,
            AtariDosDirectoryFlags.InUse | AtariDosDirectoryFlags.CreatedByDos2,
            sectorCount: 1, DataSectorNumber);

        var data = new byte[TestSectorSize];
        data[0] = 0x41;
        data[1] = 0x42;
        data[2] = 0x43;
        var linkOffset = data.Length - AtariDosFileSystemLayout.LinkByteCount;
        data[linkOffset] = checked((byte)(FinalizedEntryNumber << AtariDosFileSystemLayout.FileOwnerShift));
        data[linkOffset + 1] = 0;
        data[linkOffset + 2] = PayloadLength;

        var image = new SectorImage(
            MediaImageFormatIds.Atari90,
            TestSectorSize,
            TestCylinders,
            TestHeads,
            TestSectorsPerTrack,
            [
                Block(DataSectorNumber, data),
                Block(AtariDosFileSystemLayout.FirstDirectorySector, directory)
            ],
            logicalBlockCount: TestBlockCount);
        var warnings = new List<string>();
        var entries = AtariDosDirectoryReader.Read(
            image,
            new AtariDosDirectoryLocation(
                AtariDosFileSystemLayout.FirstDirectorySector,
                SectorCount: 1,
                AtariDosFileSystemLayout.DirectoryEntriesPerSector),
            warnings);

        if (entries.Count != 1)
            throw new InvalidOperationException("An unfinished Atari DOS directory entry must not duplicate its finalized file.");
        var entry = entries[0];
        if (entry.Name != $"{FileName}.{FileExtension}" || entry.Size != PayloadLength || entry.Content is null || !entry.Content.SequenceEqual(data.Take(PayloadLength)))
            throw new InvalidOperationException("The finalized Atari DOS directory entry must remain readable after its unfinished duplicate is ignored.");
        var expectedWarning = AtariDosWarnings.OpenForOutputEntryIgnored(
            $"{FileName}.{FileExtension}",
            AtariDosFileSystemLayout.FirstDirectorySector,
            OpenEntryNumber);
        if (warnings.Count != 1 || warnings[0] != expectedWarning)
            throw new InvalidOperationException("An unfinished Atari DOS directory entry must produce one warning without traversing its data chain.");
    }

    private static void WriteDirectoryEntry(
        byte[] directory,
        int entryNumber,
        AtariDosDirectoryFlags flags,
        ushort sectorCount,
        ushort firstSector)
    {
        var offset = entryNumber * AtariDosFileSystemLayout.DirectoryEntrySize;
        directory[offset + AtariDosFileSystemLayout.FlagsOffset] = (byte)flags;
        BinaryPrimitives.WriteUInt16LittleEndian(
            directory.AsSpan(offset + AtariDosFileSystemLayout.SectorCountOffset, sizeof(ushort)),
            sectorCount);
        BinaryPrimitives.WriteUInt16LittleEndian(
            directory.AsSpan(offset + AtariDosFileSystemLayout.FirstSectorOffset, sizeof(ushort)),
            firstSector);
        Encoding.ASCII.GetBytes(FileName.PadRight(AtariDosFileSystemLayout.NameLength)).CopyTo(
            directory.AsSpan(offset + AtariDosFileSystemLayout.NameOffset, AtariDosFileSystemLayout.NameLength));
        Encoding.ASCII.GetBytes(FileExtension.PadRight(AtariDosFileSystemLayout.ExtensionLength)).CopyTo(
            directory.AsSpan(offset + AtariDosFileSystemLayout.NameOffset + AtariDosFileSystemLayout.NameLength, AtariDosFileSystemLayout.ExtensionLength));
    }

    private static SectorBlock Block(int sectorNumber, IReadOnlyList<byte> data) =>
        new(sectorNumber - 1, new SectorAddress(0, 0, sectorNumber), data);
}
