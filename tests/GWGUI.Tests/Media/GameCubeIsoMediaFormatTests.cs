using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;

namespace GWGUI.Tests.Media;

public sealed class GameCubeIsoMediaFormatTests
{
    [Fact]
    public async Task GcmUsesTheExistingIsoReaderAndWriter()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-gcm-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var sourcePath = Path.Combine(directory, "game.gcm");
            var outputPath = Path.Combine(directory, "roundtrip.gcm");
            var source = new byte[4096];
            source[0] = 0x47;
            await File.WriteAllBytesAsync(sourcePath, source);

            var engine = MediaEngineComposition.CreateDefault();
            var document = await engine.ReadingService.ReadAsync(
                new MediaSourceDescriptor(sourcePath, []));

            Assert.Equal(OpticalImageFormatIds.Iso, document.FormatId);
            await engine.OpticalConversionService.ConvertAsync(
                sourcePath, outputPath, OpticalImageFormatIds.Iso);
            Assert.Equal(source, await File.ReadAllBytesAsync(outputPath));
            Assert.Contains(DiskImageFileExtensions.Gcm,
                MediaRecognitionComposition.CreateDefault().SupportedExtensions);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
