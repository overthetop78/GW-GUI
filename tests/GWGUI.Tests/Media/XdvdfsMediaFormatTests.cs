using System.Buffers.Binary;
using System.IO;
using System.Text;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats.Optical.Xdvdfs;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Exploration;
using GWGUI.MediaFileSystems.FileSystems.Xbox.Xdvdfs;

namespace GWGUI.Tests.Media;

public sealed class XdvdfsMediaFormatTests
{
    [Fact]
    public async Task ReadsExploresAndWritesXdvdfsFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-xdvdfs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var sourcePath = Path.Combine(directory, "game.iso");
            var outputPath = Path.Combine(directory, "roundtrip.iso");
            var source = CreateImage();
            await File.WriteAllBytesAsync(sourcePath, source);

            IMediaImageReader reader = new XdvdfsReader();
            var context = new MediaRecognitionContext(new MediaSourceDescriptor(sourcePath, []));
            Assert.True(await reader.CanReadAsync(context, CancellationToken.None));
            var document = await reader.ReadAsync(context, CancellationToken.None);
            Assert.Equal(OpticalImageFormatIds.XboxXdvdfs, document.FormatId);

            var explorer = new GWGUI.MediaFileSystems.Exploration.MediaExplorer(
                [new XdvdfsFileSystemReader()],
                new MediaVolumeDetectorRegistry([]));
            var result = await explorer.ExploreAsync(document);
            var file = Assert.Single(Assert.Single(result.ExploredVolumes).FileSystem!.Entries);
            Assert.Equal("default.xbe", file.Name);
            Assert.Equal(4, file.Size);
            Assert.Equal("XBEH", Encoding.ASCII.GetString(file.Content!.ToArray()));

            await new XdvdfsWriter().WriteAsync(document, outputPath, OpticalImageFormatIds.XboxXdvdfs);
            Assert.Equal(source, await File.ReadAllBytesAsync(outputPath));
            var convertedPath = Path.Combine(directory, "converted.iso");
            await MediaEngineComposition.CreateDefault().OpticalConversionService.ConvertAsync(
                sourcePath, convertedPath, OpticalImageFormatIds.XboxXdvdfs);
            Assert.Equal(source, await File.ReadAllBytesAsync(convertedPath));
            Assert.Contains(MediaImageWriterIds.OpticalXboxXdvdfs,
                MediaWritingComposition.CreateDefault().Writers.Select(writer => writer.Id));
            Assert.Contains(DiskImageFileExtensions.Iso,
                MediaRecognitionComposition.CreateDefault().SupportedExtensions);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private static byte[] CreateImage()
    {
        const int sectorSize = 2048;
        const int sectorCount = 36;
        var bytes = new byte[sectorCount * sectorSize];
        var descriptor = bytes.AsSpan(32 * sectorSize, sectorSize);
        Encoding.ASCII.GetBytes("MICROSOFT*XBOX*MEDIA", descriptor);
        BinaryPrimitives.WriteUInt32LittleEndian(descriptor[0x14..], 34);
        BinaryPrimitives.WriteUInt32LittleEndian(descriptor[0x18..], sectorSize);
        Encoding.ASCII.GetBytes("MICROSOFT*XBOX*MEDIA", descriptor[0x7EC..]);

        var entry = bytes.AsSpan(34 * sectorSize, sectorSize);
        BinaryPrimitives.WriteUInt32LittleEndian(entry, 0);
        BinaryPrimitives.WriteUInt16LittleEndian(entry, 0);
        BinaryPrimitives.WriteUInt16LittleEndian(entry[2..], 0);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[4..], 35);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[8..], 4);
        entry[0x0C] = 0x20;
        entry[0x0D] = 11;
        Encoding.ASCII.GetBytes("default.xbe", entry[0x0E..]);
        Encoding.ASCII.GetBytes("XBEH", bytes.AsSpan(35 * sectorSize));
        return bytes;
    }
}
