using System.Buffers.Binary;
using System.IO;
using System.Text;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaFileSystems;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Exploration;
using GWGUI.MediaFileSystems.FileSystems.Nintendo.GameCube;

namespace GWGUI.Tests.Media;

public sealed class GameCubeFstFileSystemReaderTests
{
    [Fact]
    public async Task ReadsGameCubeFstDirectoriesAndFiles()
    {
        var bytes = new byte[32 * 2048];
        WriteFst(bytes);
        var track = new OpticalTrackDescriptor(
            1, 1, OpticalTrackMode.Mode1Data2048, 0, 32,
            2048, 0, 2048, new MemoryRandomAccessData(bytes), 0);
        var volume = new MediaVolumeDescriptor(0, bytes.Length, "optical-track", sessionNumber: 1, trackNumber: 1);
        var document = new MediaImageDocument(
            new MediaSourceDescriptor("game.gcm", []),
            MediaImageFormatIds.OpticalIso,
            MediaKind.Optical,
            new OpticalMediaImageRepresentation(bytes.Length, [track]),
            [volume],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceExtension"] = ".gcm"
            });

        var reader = new GameCubeFstFileSystemReader();
        Assert.True(reader.CanRead(document, volume));
        var result = reader.Read(document, volume);
        var directory = Assert.Single(result.Entries);
        Assert.Equal("files", directory.Name);
        var file = Assert.Single(directory.Children);
        Assert.Equal("hello.txt", file.Name);
        Assert.Equal("HELLO", Encoding.ASCII.GetString(file.Content!.ToArray()));

        var explorer = new MediaExplorer([reader], new MediaVolumeDetectorRegistry([]));
        var explored = await explorer.ExploreAsync(document);
        var exploredDirectory = Assert.Single(Assert.Single(explored.ExploredVolumes).FileSystem!.Entries);
        Assert.Equal("hello.txt", Assert.Single(exploredDirectory.Children).Name);
    }

    private static void WriteFst(byte[] image)
    {
        const int fstOffset = 4 * 2048 + 16;
        const int fstSize = 61;
        BinaryPrimitives.WriteUInt32BigEndian(image.AsSpan(0x424, sizeof(uint)), fstOffset);
        BinaryPrimitives.WriteUInt32BigEndian(image.AsSpan(0x428, sizeof(uint)), fstSize);
        var fst = image.AsSpan(fstOffset, fstSize);
        BinaryPrimitives.WriteUInt32BigEndian(fst, 0x8000_0000u);
        BinaryPrimitives.WriteUInt32BigEndian(fst[8..], 3);
        BinaryPrimitives.WriteUInt32BigEndian(fst[12..], 0x8000_0000u);
        BinaryPrimitives.WriteUInt32BigEndian(fst[20..], 3);
        BinaryPrimitives.WriteUInt32BigEndian(fst[24..], 6);
        BinaryPrimitives.WriteUInt32BigEndian(fst[28..], 8 * 2048);
        BinaryPrimitives.WriteUInt32BigEndian(fst[32..], 5);
        Encoding.ASCII.GetBytes("files\0hello.txt\0").CopyTo(fst[36..]);
        Encoding.ASCII.GetBytes("HELLO").CopyTo(image.AsSpan(8 * 2048));
    }
}
