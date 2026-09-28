using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Formats.Optical.WiiU;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.Tests.Media;

public sealed class WiiUMediaFormatTests
{
    [Fact]
    public async Task WudReadAndWriteRoundTripAsOpticalBlocks()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-wiiu-wud-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var sourcePath = Path.Combine(directory, "source.wud");
            var outputPath = Path.Combine(directory, "roundtrip.wud");
            var source = Enumerable.Range(0, 2 * 2048)
                .Select(value => (byte)(value % byte.MaxValue))
                .ToArray();
            await File.WriteAllBytesAsync(sourcePath, source);

            var reader = new WiiUReader();
            var document = await ReadAsync(reader, sourcePath);
            var blocks = Assert.IsType<BlockMediaImageRepresentation>(document.Representation);
            Assert.Equal(MediaKind.Optical, document.MediaKind);
            Assert.Equal(DiskImageFormatIds.NintendoWiiU, document.FormatId);
            Assert.Equal(2048, blocks.LogicalBlockSize);
            Assert.Equal(2, blocks.LogicalBlockCount);
            Assert.Equal(DiskImageFileExtensions.Wud, document.Metadata["nintendo.wiiu.variant"]);

            var readBack = new byte[source.Length];
            await ((IMediaBlockRepresentation)blocks).ReadExactlyAsync(0, readBack);
            Assert.Equal(source, readBack);

            await new WiiUWriter().WriteAsync(document, outputPath, DiskImageFormatIds.NintendoWiiU);
            Assert.Equal(source, await File.ReadAllBytesAsync(outputPath));

            var catalog = new BuiltInImageFormatCatalog();
            var format = Assert.Single(catalog.Formats, item => item.Id == DiskImageFormatIds.NintendoWiiU);
            Assert.Equal([DiskImageFileExtensions.Wud, DiskImageFileExtensions.Wux], format.Extensions.Select(item => item.Extension));
            Assert.Contains(MediaImageWriterIds.NintendoWiiU,
                MediaWritingComposition.CreateDefault().Writers.Select(writer => writer.Id));
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task WuxReadMapsIndexedSectorsAndExposesWiiUMetadata()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-wiiu-wux-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var sourcePath = Path.Combine(directory, "source.wux");
            var outputPath = Path.Combine(directory, "roundtrip.wux");
            var source = CreateWux();
            await File.WriteAllBytesAsync(sourcePath, source);

            var reader = new WiiUReader();
            var document = await ReadAsync(reader, sourcePath);
            var blocks = Assert.IsType<BlockMediaImageRepresentation>(document.Representation);
            var readBack = new byte[checked((int)blocks.Capacity)];
            await ((IMediaBlockRepresentation)blocks).ReadExactlyAsync(0, readBack);

            Assert.Equal(Enumerable.Repeat((byte)0x11, 4096)
                .Concat(Enumerable.Repeat((byte)0x22, 4096)).ToArray(), readBack);
            Assert.Equal(DiskImageFileExtensions.Wux, document.Metadata["nintendo.wiiu.variant"]);
            Assert.Equal("2", document.Metadata["nintendo.wiiu.indexEntryCount"]);
            Assert.Equal("2", document.Metadata["nintendo.wiiu.storedSectorCount"]);

            await new WiiUWriter().WriteAsync(document, outputPath, DiskImageFormatIds.NintendoWiiU);
            var roundtrip = await ReadAsync(reader, outputPath);
            var roundtripBlocks = Assert.IsType<BlockMediaImageRepresentation>(roundtrip.Representation);
            var roundtripBytes = new byte[checked((int)roundtripBlocks.Capacity)];
            await ((IMediaBlockRepresentation)roundtripBlocks).ReadExactlyAsync(0, roundtripBytes);
            Assert.Equal(readBack, roundtripBytes);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    private static async Task<MediaImageDocument> ReadAsync(WiiUReader reader, string path)
    {
        var context = new MediaRecognitionContext(new MediaSourceDescriptor(path, []));
        var imageReader = (IMediaImageReader)reader;
        Assert.True(await imageReader.CanReadAsync(context, CancellationToken.None));
        return await imageReader.ReadAsync(context, CancellationToken.None);
    }

    private static byte[] CreateWux()
    {
        const int sectorSize = 4096;
        const int headerSize = 28;
        const int indexCount = 2;
        const int sectorArrayOffset = sectorSize;
        var bytes = new byte[sectorArrayOffset + indexCount * sectorSize];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(0, 4), WiiUFormat.WuxMagic0);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4, 4), WiiUFormat.WuxMagic1);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8, 4), sectorSize);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(12, 8), 2 * (ulong)sectorSize);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(20, 4), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(headerSize, 4), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(headerSize + 4, 4), 1);
        bytes.AsSpan(sectorArrayOffset, sectorSize).Fill(0x11);
        bytes.AsSpan(sectorArrayOffset + sectorSize, sectorSize).Fill(0x22);
        return bytes;
    }
}
