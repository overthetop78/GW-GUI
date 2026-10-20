using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class NativeInputStateTests
{
    [Theory]
    [InlineData(int.MinValue, int.MaxValue, short.MinValue, short.MaxValue)]
    [InlineData(-32769, 32768, short.MinValue, short.MaxValue)]
    [InlineData(-32768, 32767, short.MinValue, short.MaxValue)]
    [InlineData(-17, 29, -17, 29)]
    public void RelativeMouseAxesSaturateWithoutWrapping(int x, int y, short expectedX, short expectedY)
    {
        var pointer = new EmulationPointerState(x, y, 0, false, false, false);
        Assert.Equal(expectedX, EmulationInputMappingFunctions.NativeMouseState(pointer, 0, 0, 0));
        Assert.Equal(expectedY, EmulationInputMappingFunctions.NativeMouseState(pointer, 0, 0, 1));
    }

    [Theory]
    [InlineData(1u, 0u, 0u)]
    [InlineData(0u, 1u, 0u)]
    [InlineData(uint.MaxValue, 0u, 2u)]
    [InlineData(0u, uint.MaxValue, 2u)]
    [InlineData(0u, 0u, uint.MaxValue)]
    public void UnsupportedMouseRequestsReturnZero(uint port, uint index, uint id)
    {
        var pointer = new EmulationPointerState(21, -32, 1, true, true, true);
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeMouseState(pointer, port, index, id));
    }

    [Theory]
    [InlineData(int.MinValue, 0, 1)]
    [InlineData(0, 0, 0)]
    [InlineData(int.MaxValue, 1, 0)]
    public void WheelDirectionIsReportedAsABoolean(int wheel, short up, short down)
    {
        var pointer = new EmulationPointerState(0, 0, wheel, true, false, true);
        Assert.Equal((short)1, EmulationInputMappingFunctions.NativeMouseState(pointer, 0, 0, 2));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeMouseState(pointer, 0, 0, 3));
        Assert.Equal((short)1, EmulationInputMappingFunctions.NativeMouseState(pointer, 0, 0, 6));
        Assert.Equal(up, EmulationInputMappingFunctions.NativeMouseState(pointer, 0, 0, 4));
        Assert.Equal(down, EmulationInputMappingFunctions.NativeMouseState(pointer, 0, 0, 5));
    }

    [Theory]
    [InlineData(16u)]
    [InlineData(32u)]
    public void JoypadMaskExcludesUpperCarrierBits(uint limit)
    {
        Assert.Equal(unchecked((short)0x8123), EmulationInputMappingFunctions.NativeJoypadState(0xffff8123u, 256, limit));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeJoypadState(0xffff0000u, 256, limit));
    }

    [Fact]
    public void JoypadRangePreventsCarrierBitsAndMaskedShiftAliases()
    {
        Assert.Equal((short)1, EmulationInputMappingFunctions.NativeJoypadState(1u << 15, 15, 16));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeJoypadState(1u << 16, 16, 16));
        Assert.Equal((short)1, EmulationInputMappingFunctions.NativeJoypadState(1u << 31, 31, 32));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeJoypadState(uint.MaxValue, 32, 32));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeJoypadState(uint.MaxValue, uint.MaxValue, 32));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeJoypadState(0, 0, 32));
    }

    [Fact]
    public void StickReadsKeepSignedAxesAndDoNotExposeTriggers()
    {
        var controller = new EmulationControllerState(0, short.MinValue, -42, 71, short.MaxValue, 111, 222);
        Assert.Equal(short.MinValue, EmulationInputMappingFunctions.NativeAnalogState(controller, 0, 0));
        Assert.Equal((short)-42, EmulationInputMappingFunctions.NativeAnalogState(controller, 0, 1));
        Assert.Equal((short)71, EmulationInputMappingFunctions.NativeAnalogState(controller, 1, 0));
        Assert.Equal(short.MaxValue, EmulationInputMappingFunctions.NativeAnalogState(controller, 1, 1));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeAnalogState(controller, 2, 12));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeAnalogState(controller, 2, 13));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeAnalogState(controller, 0, uint.MaxValue));
        Assert.Equal((short)0, EmulationInputMappingFunctions.NativeAnalogState(controller, uint.MaxValue, 0));
    }
}
