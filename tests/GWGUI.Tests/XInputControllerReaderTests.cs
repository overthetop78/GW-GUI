using GWGUI.App.Services;

namespace GWGUI.Tests;

public sealed class XInputControllerReaderTests
{
    [Fact]
    public void Map_ConvertsXInputButtonsAxesAndTriggersToLibretroLayout()
    {
        const ushort dpadUp = 0x0001;
        const ushort start = 0x0010;
        const ushort guide = 0x0400;
        const ushort a = 0x1000;
        const ushort b = 0x2000;
        var state = XInputControllerReader.Map((ushort)(dpadUp | start | guide | a | b), 255, 31,
            123, short.MinValue, -456, 1000);

        Assert.NotEqual(0u, state.Buttons & (1u << 0));
        Assert.NotEqual(0u, state.Buttons & (1u << 3));
        Assert.NotEqual(0u, state.Buttons & (1u << 4));
        Assert.NotEqual(0u, state.Buttons & (1u << 8));
        Assert.NotEqual(0u, state.Buttons & (1u << 12));
        Assert.NotEqual(0u, state.Buttons & (1u << 13));
        Assert.NotEqual(0u, state.Buttons & (1u << 16));
        Assert.Equal((short)123, state.LeftX);
        Assert.Equal(short.MaxValue, state.LeftY);
        Assert.Equal((short)-456, state.RightX);
        Assert.Equal((short)-1000, state.RightY);
        Assert.Equal(short.MaxValue, state.LeftTrigger);
        Assert.True(state.RightTrigger > 0);
    }
}
