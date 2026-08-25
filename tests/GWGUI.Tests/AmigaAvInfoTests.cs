using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class AmigaAvInfoTests
{
    [Fact]
    public void SystemAvInfoCallback_UpdatesRuntimeTiming()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Av", Guid.NewGuid().ToString("N"));
        using var callbacks = new AmigaExternalHostCallbacks(
            Path.Combine(root, "System"), Path.Combine(root, "Content"), Path.Combine(root, "Saves"), null);
        var info = new ExternalCoreApi.SystemAvInfo
        {
            Geometry = new ExternalCoreApi.Geometry
            {
                BaseWidth = 320,
                BaseHeight = 256,
                MaximumWidth = 1440,
                MaximumHeight = 576,
                AspectRatio = 4f / 3f
            },
            Timing = new ExternalCoreApi.Timing { FramesPerSecond = 59.94, SampleRate = 48000 }
        };
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.SystemAvInfo>());
        try
        {
            Marshal.StructureToPtr(info, pointer, false);

            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetSystemAvInfo, pointer));
            Assert.Equal(59.94, callbacks.FramesPerSecond, 2);
            Assert.Equal(48000, callbacks.SampleRate);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void AvCallbacks_RejectNullPointersWithoutChangingTiming()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Av", Guid.NewGuid().ToString("N"));
        using var callbacks = new AmigaExternalHostCallbacks(
            Path.Combine(root, "System"), Path.Combine(root, "Content"), Path.Combine(root, "Saves"), null);
        try
        {
            Assert.False(callbacks.Environment(ExternalCoreApiConstants.SetGeometry, 0));
            Assert.False(callbacks.Environment(ExternalCoreApiConstants.SetSystemAvInfo, 0));
            Assert.Equal(50, callbacks.FramesPerSecond);
            Assert.Equal(44100, callbacks.SampleRate);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
