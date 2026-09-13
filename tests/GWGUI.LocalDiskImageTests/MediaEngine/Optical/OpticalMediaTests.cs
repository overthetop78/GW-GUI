using System.Buffers.Binary;
using System.Text;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Exploration.Partitioning;
using GWGUI.MediaEngine.FileSystems.Iso9660;
using GWGUI.MediaEngine.Formats.Optical.BinCue;
using GWGUI.MediaEngine.Reading.Optical;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Representations.Optical;

namespace GWGUI.Tests.MediaEngine.Optical;

public sealed class OpticalMediaTests
{
    private const int SectorSize = 2_048;

    [Fact]
    public void CueSheetKeepsAssociatedFilesAndTrackDeclarations()
    {
        var sheet = new CueSheetReader().Parse([
            "FILE \"disc one.bin\" BINARY",
            "  TRACK 01 MODE1/2352",
            "    INDEX 01 00:00:00",
            "FILE audio.bin BINARY",
            "  TRACK 02 AUDIO",
            "    INDEX 00 00:00:00",
            "    INDEX 01 00:02:00"
        ]);

        Assert.Equal(new[] { "disc one.bin", "audio.bin" }, sheet.Files.Select(file => file.DeclaredPath));
        Assert.Equal(new[] { 1, 2 }, sheet.Files.SelectMany(file => file.Tracks).Select(track => track.Number));
        Assert.Equal(OpticalTrackMode.Mode1Raw2352, sheet.Files[0].Tracks[0].Mode);
        Assert.Equal(OpticalTrackMode.Audio, sheet.Files[1].Tracks[0].Mode);
        Assert.Equal(150, sheet.Files[1].Tracks[0].Indexes.Single(index => index.Number == 1).RelativeSector);
    }

    [Fact]
    public async Task RawOpticalSectorExposesOnlyItsUserDataWindow()
    {
        var stored = new byte[2_352];
        stored.AsSpan(16, SectorSize).Fill(0x5a);
        var track = new OpticalTrackDescriptor(
            1, 1, OpticalTrackMode.Mode1Raw2352, 0, 1, 2_352, 16, SectorSize,
            new MemoryRandomAccessData(stored), 0);

        var userData = await new OpticalSectorReader().ReadUserDataAsync(track, 0);

        Assert.Equal(SectorSize, userData.Length);
        Assert.All(userData, value => Assert.Equal(0x5a, value));
        Assert.False(track.HasSubchannels);
    }

    [Fact]
    public async Task SessionsBecomeDistinctDataVolumesWithoutInventingPhysicalInformation()
    {
        var source = new MemoryRandomAccessData(new byte[4 * SectorSize]);
        var tracks = new[]
        {
            new OpticalTrackDescriptor(1, 1, OpticalTrackMode.Mode1Data2048, 0, 2, SectorSize, 0, SectorSize, source, 0),
            new OpticalTrackDescriptor(2, 2, OpticalTrackMode.Mode1Data2048, 2, 2, SectorSize, 0, SectorSize, source, 2 * SectorSize)
        };
        var representation = new OpticalMediaImageRepresentation(4 * SectorSize, tracks, associatedFiles: ["disc.cue", "disc.bin"]);
        var document = CreateDocument(representation);

        var detected = await new OpticalTrackVolumeDetector().DetectAsync(document);

        Assert.Equal(new[] { 1, 2 }, representation.Sessions);
        Assert.Equal(new int?[] { 1, 2 }, detected!.Volumes.Select(volume => volume.SessionNumber));
        Assert.Equal(new int?[] { 1, 2 }, detected.Volumes.Select(volume => volume.TrackNumber));
        Assert.Null(representation.LayerCount);
        Assert.Null(representation.FaceCount);
        Assert.Equal(new[] { "disc.cue", "disc.bin" }, representation.AssociatedFiles);
    }

    [Fact]
    public void MinimalIso9660VolumeExposesItsFile()
    {
        var bytes = CreateIso9660Image();
        var source = new MemoryRandomAccessData(bytes);
        var track = new OpticalTrackDescriptor(
            1, 1, OpticalTrackMode.Mode1Data2048, 0, bytes.Length / SectorSize,
            SectorSize, 0, SectorSize, source, 0);
        var representation = new OpticalMediaImageRepresentation(bytes.Length, [track]);
        var document = CreateDocument(representation);
        var volume = new MediaVolumeDescriptor(0, bytes.Length, "test", sessionNumber: 1, trackNumber: 1);
        var reader = new Iso9660FileSystemReader();

        Assert.True(reader.CanRead(document, volume));
        var fileSystem = reader.Read(document, volume);
        var file = Assert.Single(fileSystem.Entries);
        Assert.Equal("HELLO.TXT", file.Name);
        Assert.Equal(5, file.Size);
        Assert.Equal("hello", Encoding.ASCII.GetString(file.Content!.ToArray()));
    }

    private static MediaImageDocument CreateDocument(OpticalMediaImageRepresentation representation) => new(
        new MediaSourceDescriptor("memory.optical", []),
        "test.optical",
        MediaKind.Optical,
        representation,
        [],
        [],
        new Dictionary<string, string>());

    private static byte[] CreateIso9660Image()
    {
        const int sectorCount = 24;
        const int rootSector = 20;
        const int fileSector = 21;
        var image = new byte[sectorCount * SectorSize];
        var descriptor = image.AsSpan(16 * SectorSize, SectorSize);
        descriptor[0] = 1;
        Encoding.ASCII.GetBytes("CD001").CopyTo(descriptor[1..]);
        descriptor[6] = 1;
        Encoding.ASCII.GetBytes("GWGUI TEST").CopyTo(descriptor[40..]);
        WriteBothEndianUInt32(descriptor[80..], sectorCount);
        WriteBothEndianUInt16(descriptor[128..], SectorSize);
        WriteDirectoryRecord(descriptor[156..], rootSector, SectorSize, true, [0]);

        var terminator = image.AsSpan(17 * SectorSize, SectorSize);
        terminator[0] = 255;
        Encoding.ASCII.GetBytes("CD001").CopyTo(terminator[1..]);
        terminator[6] = 1;

        WriteDirectoryRecord(
            image.AsSpan(rootSector * SectorSize, SectorSize),
            fileSector,
            5,
            false,
            Encoding.ASCII.GetBytes("HELLO.TXT;1"));
        Encoding.ASCII.GetBytes("hello").CopyTo(image.AsSpan(fileSector * SectorSize));
        return image;
    }

    private static void WriteDirectoryRecord(Span<byte> destination, uint extent, uint length, bool directory, ReadOnlySpan<byte> name)
    {
        var recordLength = 33 + name.Length + (name.Length % 2 == 0 ? 1 : 0);
        destination[0] = checked((byte)recordLength);
        destination[1] = 0;
        WriteBothEndianUInt32(destination[2..], extent);
        WriteBothEndianUInt32(destination[10..], length);
        destination[25] = directory ? (byte)2 : (byte)0;
        destination[28] = 1;
        destination[31] = 1;
        destination[32] = checked((byte)name.Length);
        name.CopyTo(destination[33..]);
    }

    private static void WriteBothEndianUInt16(Span<byte> destination, int value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(destination, checked((ushort)value));
        BinaryPrimitives.WriteUInt16BigEndian(destination[2..], checked((ushort)value));
    }

    private static void WriteBothEndianUInt32(Span<byte> destination, uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(destination, value);
        BinaryPrimitives.WriteUInt32BigEndian(destination[4..], value);
    }
}
