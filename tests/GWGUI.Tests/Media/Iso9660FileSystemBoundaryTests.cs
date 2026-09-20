using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaFileSystems;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.FileSystems.Iso9660;

namespace GWGUI.Tests.Media;

public sealed class Iso9660FileSystemBoundaryTests
{
    [Fact]
    public void ReadsCatalogAndFileThroughDecodedOpticalTrack()
    {
        const int sectorSize = 2_048;
        var bytes = new byte[32 * sectorSize];
        var descriptor = bytes.AsSpan(16 * sectorSize, sectorSize);
        descriptor[0] = 1;
        "CD001"u8.CopyTo(descriptor[1..]);
        descriptor[6] = 1;
        "TEST"u8.CopyTo(descriptor[40..]);
        WriteBothEndian32(descriptor[80..], 32);
        WriteBothEndian16(descriptor[128..], sectorSize);
        WriteRecord(descriptor[156..], 20, sectorSize, 2, [0]);

        var directory = bytes.AsSpan(20 * sectorSize, sectorSize);
        WriteRecord(directory, 20, sectorSize, 2, [0]);
        var fileName = Encoding.ASCII.GetBytes("HELLO.TXT;1");
        WriteRecord(directory[34..], 21, 5, 0, fileName);
        "HELLO"u8.CopyTo(bytes.AsSpan(21 * sectorSize));

        var track = new OpticalTrackDescriptor(
            1, 1, OpticalTrackMode.Mode1Data2048, 0, 32,
            sectorSize, 0, sectorSize, new MemoryRandomAccessData(bytes), 0);
        var representation = new OpticalMediaImageRepresentation(bytes.Length, [track]);
        var volume = new MediaVolumeDescriptor(0, bytes.Length, "optical-track", sessionNumber: 1, trackNumber: 1);
        var document = new MediaImageDocument(
            new MediaSourceDescriptor("memory.iso", []), "optical.iso", MediaKind.Optical,
            representation, [volume], [], new Dictionary<string, string>());
        var reader = new Iso9660FileSystemReader();

        Assert.True(reader.CanRead(document, volume));
        var result = reader.Read(document, volume);
        Assert.Equal("TEST", result.Name);
        var entry = Assert.Single(result.Entries);
        Assert.Equal("HELLO.TXT", entry.Name);
        Assert.Equal(FileSystemEntryKind.File, entry.Kind);
        Assert.Equal("HELLO", Encoding.ASCII.GetString(entry.Content!.ToArray()));

        var fileSystemsExplorer = new GWGUI.MediaFileSystems.Exploration.MediaExplorer(
            [reader], new GWGUI.MediaFileSystems.Exploration.MediaVolumeDetectorRegistry([]));
        var engineResult = new GWGUI.MediaEngine.Images.Reading.MediaExplorer(fileSystemsExplorer).Explore(document);
        var engineVolume = Assert.Single(engineResult.Volumes);
        var engineEntry = Assert.Single(engineVolume.FileSystem!.Entries);
        Assert.Equal("HELLO.TXT", engineEntry.Name);
        Assert.Equal("HELLO", Encoding.ASCII.GetString(engineEntry.Content!.ToArray()));
    }

    private static void WriteRecord(Span<byte> destination, uint extent, uint length, byte flags, ReadOnlySpan<byte> name)
    {
        destination[0] = checked((byte)(33 + name.Length + (name.Length % 2 == 0 ? 1 : 0)));
        WriteBothEndian32(destination[2..], extent);
        WriteBothEndian32(destination[10..], length);
        destination[25] = flags;
        destination[32] = checked((byte)name.Length);
        name.CopyTo(destination[33..]);
    }

    private static void WriteBothEndian16(Span<byte> destination, ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(destination, value);
        BinaryPrimitives.WriteUInt16BigEndian(destination[2..], value);
    }

    private static void WriteBothEndian32(Span<byte> destination, uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(destination, value);
        BinaryPrimitives.WriteUInt32BigEndian(destination[4..], value);
    }
}
