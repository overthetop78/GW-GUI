using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GWGUI.App.Rendering.Emulation.Processing;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Rendering.Emulation;

internal static class EmulationVideoSnapshotFunctions
{
    internal static Task<BitmapSource?> CreateAsync(VideoFrame? frame, EmulationVideoProcessingConfiguration configuration, EmulationVideoProcessingSize outputSize)
    {
        if (frame is null) return Task.FromResult<BitmapSource?>(null);
        return Task.Run<BitmapSource?>(() =>
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            var surfaceFrame = EmulationVideoSurfaceFrameFunctions.Process(pipeline, configuration, frame, outputSize);
            var processed = surfaceFrame.Frame;
            var pixels = surfaceFrame.Bgra32Pixels;
            var bitmap = BitmapSource.Create(processed.Width, processed.Height, 96, 96, PixelFormats.Bgra32, null, pixels, processed.Width * 4);
            bitmap.Freeze();
            return bitmap;
        });
    }

    internal static BitmapSource? Ensure(ref WriteableBitmap? snapshot, VideoFrame? frame,
        IEmulationVideoProcessingPipeline pipeline, EmulationVideoProcessingConfiguration configuration,
        EmulationVideoProcessingSize outputSize)
    {
        if (snapshot is not null) return snapshot;
        if (frame is null) return null;
        var processed = EmulationVideoSurfaceFrameFunctions.Process(pipeline, configuration, frame, outputSize);
        Update(ref snapshot, processed.Bgra32Pixels, processed.Frame.Width, processed.Frame.Height);
        return snapshot;
    }

    private static void Update(ref WriteableBitmap? snapshot, byte[] pixels, int width, int height)
    {
        if (snapshot is null || snapshot.PixelWidth != width || snapshot.PixelHeight != height)
            snapshot = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        snapshot.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
    }
}