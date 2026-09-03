using System.Windows.Media;
using System.Windows.Media.Imaging;
using GWGUI.App.Rendering.Emulation.Processing;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Rendering.Emulation;

internal static class EmulationVideoSnapshotFunctions
{
    internal static Task<BitmapSource?> CreateAsync(VideoFrame? frame,
        EmulationVideoProcessingConfiguration configuration,
        EmulationVideoProcessingSize outputSize)
    {
        if (frame is null) return Task.FromResult<BitmapSource?>(null);
        return Task.Run<BitmapSource?>(() =>
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            var processed = pipeline.Process(configuration, frame,
                new EmulationVideoProcessingSize(frame.Width, frame.Height), outputSize);
            var pixels = EmulationVideoPixelFunctions.ToBgra32(processed);
            var bitmap = BitmapSource.Create(processed.Width, processed.Height,
                96, 96, PixelFormats.Bgra32, null, pixels, processed.Width * 4);
            bitmap.Freeze();
            return bitmap;
        });
    }
}