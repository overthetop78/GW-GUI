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
    private const int SharedParentEntryNumber = 0;
    private const int SharedViewEntryNumber = 1;
    private const int BoundedViewEntryNumber = 2;
    private const int InvalidOwnerEntryNumber = 3;
    private const int UnrelatedOwnerEntryNumber = 7;
    private const int SharedParentFirstSector = 100;
    private const int SharedViewFirstSector = 101;
    private const int BoundedViewFirstSector = 200;
    private const int BoundedViewContinuationSector = 201;
    private const int InvalidOwnerFirstSector = 300;
    private const byte SharedPayloadLength = 1;
    private const string SharedParentName = "PARENT";
    private const string SharedViewName = "ALIAS";
    private const string BoundedViewName = "BOUNDED";
    private const string InvalidOwnerName = "BROKEN";
    private const string DataExtension = "DAT";

    internal static void Run()
    {
        VerifyOpenEntryIsIgnored();
        VerifySharedAndBoundedSectorViews();
    }

    private static void VerifyOpenEntryIsIgnored()
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

    private static void VerifySharedAndBoundedSectorViews()
    {
        var directory = new byte[TestSectorSize];
        WriteDirectoryEntry(directory, SharedParentEntryNumber,
            AtariDosDirectoryFlags.InUse | AtariDosDirectoryFlags.CreatedByDos2,
            sectorCount: 4, SharedParentFirstSector, SharedParentName, DataExtension);
        WriteDirectoryEntry(directory, SharedViewEntryNumber,
            AtariDosDirectoryFlags.InUse | AtariDosDirectoryFlags.CreatedByDos2,
            sectorCount: 2, SharedViewFirstSector, SharedViewName, DataExtension);
        WriteDirectoryEntry(directory, BoundedViewEntryNumber,
            AtariDosDirectoryFlags.InUse | AtariDosDirectoryFlags.CreatedByDos2,
            sectorCount: 1, BoundedViewFirstSector, BoundedViewName, DataExtension);
        WriteDirectoryEntry(directory, InvalidOwnerEntryNumber,
            AtariDosDirectoryFlags.InUse | AtariDosDirectoryFlags.CreatedByDos2,
            sectorCount: 1, InvalidOwnerFirstSector, InvalidOwnerName, DataExtension);

        var image = new SectorImage(
            MediaImageFormatIds.Atari90,
            TestSectorSize,
            TestCylinders,
            TestHeads,
            TestSectorsPerTrack,
            [
                DataBlock(SharedParentFirstSector, SharedParentEntryNumber, SharedViewFirstSector, 0x10),
                DataBlock(SharedViewFirstSector, SharedParentEntryNumber, SharedViewFirstSector + 1, 0x11),
                DataBlock(SharedViewFirstSector + 1, SharedParentEntryNumber, SharedViewFirstSector + 2, 0x12),
                DataBlock(SharedViewFirstSector + 2, SharedParentEntryNumber, 0, 0x13),
                DataBlock(BoundedViewFirstSector, BoundedViewEntryNumber, BoundedViewContinuationSector, 0x20),
                DataBlock(InvalidOwnerFirstSector, UnrelatedOwnerEntryNumber, 0, 0x30),
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

        var parent = entries.Single(entry => entry.Name == $"{SharedParentName}.{DataExtension}");
        var shared = entries.Single(entry => entry.Name == $"{SharedViewName}.{DataExtension}");
        var bounded = entries.Single(entry => entry.Name == $"{BoundedViewName}.{DataExtension}");
        var invalidOwner = entries.Single(entry => entry.Name == $"{InvalidOwnerName}.{DataExtension}");
        if (parent.Size != 4 || parent.DataValid != true || parent.Attributes.Count != 0)
            throw new InvalidOperationException("The complete Atari DOS owner chain must remain an ordinary valid file.");
        if (shared.Size != 2 || shared.DataValid != true
            || !shared.Attributes.Contains(AtariDosFileSystemLayout.SharedSectorViewAttribute)
            || !shared.Attributes.Contains(AtariDosFileSystemLayout.BoundedSectorViewAttribute))
            throw new InvalidOperationException("A contiguous view inside another Atari DOS chain must remain valid and identify its shared sectors.");
        if (bounded.Size != 1 || bounded.DataValid != true
            || !bounded.Attributes.Contains(AtariDosFileSystemLayout.BoundedSectorViewAttribute)
            || bounded.Attributes.Contains(AtariDosFileSystemLayout.SharedSectorViewAttribute))
            throw new InvalidOperationException("A directory-bounded Atari DOS view must stop at its declared extent without becoming a shared view.");
        if (invalidOwner.DataValid != false
            || invalidOwner.Attributes.Contains(AtariDosFileSystemLayout.SharedSectorViewAttribute)
            || invalidOwner.Diagnostics.Count != 1)
            throw new InvalidOperationException("An unexplained Atari DOS sector owner mismatch must remain invalid.");
    }

    private static void WriteDirectoryEntry(
        byte[] directory,
        int entryNumber,
        AtariDosDirectoryFlags flags,
        ushort sectorCount,
        ushort firstSector,
        string fileName = FileName,
        string fileExtension = FileExtension)
    {
        var offset = entryNumber * AtariDosFileSystemLayout.DirectoryEntrySize;
        directory[offset + AtariDosFileSystemLayout.FlagsOffset] = (byte)flags;
        BinaryPrimitives.WriteUInt16LittleEndian(
            directory.AsSpan(offset + AtariDosFileSystemLayout.SectorCountOffset, sizeof(ushort)),
            sectorCount);
        BinaryPrimitives.WriteUInt16LittleEndian(
            directory.AsSpan(offset + AtariDosFileSystemLayout.FirstSectorOffset, sizeof(ushort)),
            firstSector);
        Encoding.ASCII.GetBytes(fileName.PadRight(AtariDosFileSystemLayout.NameLength)).CopyTo(
            directory.AsSpan(offset + AtariDosFileSystemLayout.NameOffset, AtariDosFileSystemLayout.NameLength));
        Encoding.ASCII.GetBytes(fileExtension.PadRight(AtariDosFileSystemLayout.ExtensionLength)).CopyTo(
            directory.AsSpan(offset + AtariDosFileSystemLayout.NameOffset + AtariDosFileSystemLayout.NameLength, AtariDosFileSystemLayout.ExtensionLength));
    }

    private static SectorBlock DataBlock(int sectorNumber, int ownerEntryNumber, int nextSector, byte payload)
    {
        var data = new byte[TestSectorSize];
        data[0] = payload;
        var linkOffset = data.Length - AtariDosFileSystemLayout.LinkByteCount;
        data[linkOffset] = checked((byte)((ownerEntryNumber << AtariDosFileSystemLayout.FileOwnerShift)
            | (nextSector >> BitConstants.BitsPerByte & AtariDosFileSystemLayout.NextSectorHighMask)));
        data[linkOffset + 1] = checked((byte)nextSector);
        data[linkOffset + 2] = SharedPayloadLength;
        return Block(sectorNumber, data);
    }

    private static SectorBlock Block(int sectorNumber, IReadOnlyList<byte> data) =>
        new(sectorNumber - 1, new SectorAddress(0, 0, sectorNumber), data);
}
