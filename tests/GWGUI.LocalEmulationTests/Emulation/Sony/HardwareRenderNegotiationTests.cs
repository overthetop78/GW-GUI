using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class HardwareRenderNegotiationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void UnsupportedContextsAreDeclinedWithoutInstallingCallbacks(int contextType)
    {
        using var session = new HardwareRenderSession();
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.HardwareRenderCallback>());
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.HardwareRenderCallback { ContextType = contextType }, pointer, false);
            Assert.False(session.Configure(pointer));
            var callback = Marshal.PtrToStructure<ExternalCoreApi.HardwareRenderCallback>(pointer);
            Assert.Equal(nint.Zero, callback.GetCurrentFramebuffer);
            Assert.Equal(nint.Zero, callback.GetProcAddress);
        }
        finally { Marshal.FreeHGlobal(pointer); }
    }

    [Fact]
    public void MissingContextCannotProduceAFrameAndCanBeReleasedRepeatedly()
    {
        using var session = new HardwareRenderSession();
        Assert.False(session.Configure(nint.Zero));
        session.Reset();
        Assert.False(session.TryReadFramebuffer(640, 448, out var pixels));
        Assert.Empty(pixels);
        session.NotifyDestroy();
        session.Dispose();
    }
}
