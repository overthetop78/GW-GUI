using GWGUI.Emulation;
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
        Assert.Equal(expectedX, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 0));
        Assert.Equal(expectedY, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 1));
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
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeMouseState(pointer, port, index, id));
    }

    [Theory]
    [InlineData(int.MinValue, 0, 1)]
    [InlineData(0, 0, 0)]
    [InlineData(int.MaxValue, 1, 0)]
    public void WheelDirectionIsReportedAsABoolean(int wheel, short up, short down)
    {
        var pointer = new EmulationPointerState(0, 0, wheel, true, false, true);
        Assert.Equal((short)1, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 2));
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 3));
        Assert.Equal((short)1, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 6));
        Assert.Equal(up, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 4));
        Assert.Equal(down, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 5));
    }

    [Theory]
    [InlineData(int.MinValue, 0, 1)]
    [InlineData(0, 0, 0)]
    [InlineData(int.MaxValue, 1, 0)]
    public void HorizontalWheelDirectionIsIndependentFromVerticalWheel(int wheel, short right, short left)
    {
        var pointer = new EmulationPointerState(0, 0, 17, false, false, false, HorizontalWheel: wheel);
        Assert.Equal(right, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 7));
        Assert.Equal(left, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 8));
        Assert.Equal((short)1, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 4));
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 5));
    }

    [Fact]
    public void ExtraMouseButtonsRemainIndependentAndRespectNativeRequestBounds()
    {
        foreach (var button4 in new[] { false, true })
        foreach (var button5 in new[] { false, true })
        {
            var pointer = new EmulationPointerState(0, 0, 0, false, false, false, button4, button5, 1);
            Assert.Equal(button4 ? (short)1 : (short)0, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 9));
            Assert.Equal(button5 ? (short)1 : (short)0, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 10));
            foreach (var id in new uint[] { 7, 8, 9, 10 })
            {
                Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeMouseState(pointer, 1, 0, id));
                Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 1, id));
            }
            Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeMouseState(pointer, 0, 0, 11));
        }
    }

    [Theory]
    [InlineData(16u)]
    [InlineData(32u)]
    public void JoypadMaskExcludesUpperCarrierBits(uint limit)
    {
        Assert.Equal(unchecked((short)0x8123), EmulationSdk.InputProcessing.NativeJoypadState(0xffff8123u, 256, limit));
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeJoypadState(0xffff0000u, 256, limit));
    }

    [Fact]
    public void JoypadRangePreventsCarrierBitsAndMaskedShiftAliases()
    {
        Assert.Equal((short)1, EmulationSdk.InputProcessing.NativeJoypadState(1u << 15, 15, 16));
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeJoypadState(1u << 16, 16, 16));
        Assert.Equal((short)1, EmulationSdk.InputProcessing.NativeJoypadState(1u << 31, 31, 32));
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeJoypadState(uint.MaxValue, 32, 32));
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeJoypadState(uint.MaxValue, uint.MaxValue, 32));
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeJoypadState(0, 0, 32));
    }

    [Fact]
    public void StickReadsKeepSignedAxesAndPressureReadsExposeTriggers()
    {
        var controller = new EmulationControllerState(0, short.MinValue, -42, 71, short.MaxValue, 111, 222);
        Assert.Equal(short.MinValue, EmulationSdk.InputProcessing.NativeAnalogState(controller, 0, 0));
        Assert.Equal((short)-42, EmulationSdk.InputProcessing.NativeAnalogState(controller, 0, 1));
        Assert.Equal((short)71, EmulationSdk.InputProcessing.NativeAnalogState(controller, 1, 0));
        Assert.Equal(short.MaxValue, EmulationSdk.InputProcessing.NativeAnalogState(controller, 1, 1));
        Assert.Equal((short)111, EmulationSdk.InputProcessing.NativeAnalogState(controller, 2, 12));
        Assert.Equal((short)222, EmulationSdk.InputProcessing.NativeAnalogState(controller, 2, 13));
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeAnalogState(controller, 0, uint.MaxValue));
        Assert.Equal((short)0, EmulationSdk.InputProcessing.NativeAnalogState(controller, uint.MaxValue, 0));
    }

    [Fact]
    public void AnalogPressuresPreserveTheirCourseAndOverrideDigitalFallbacks()
    {
        var processing = EmulationSdk.InputProcessing;
        var controller = EmulationControllerState.Empty with { Buttons = ushort.MaxValue, LeftTrigger = 1234, RightTrigger = 2345 };
        for (uint id = 0; id < 16; id++)
            Assert.Equal(id == 12 ? (short)1234 : id == 13 ? (short)2345 : short.MaxValue, processing.NativeAnalogState(controller, 2, id));
        var values = new Dictionary<string, float> { ["A"] = .25f, ["L2"] = 0, ["R2"] = .75f, ["B"] = float.NaN, ["X"] = float.PositiveInfinity, ["Y"] = -1, ["Start"] = 2 };
        controller = controller with { Controls = new EmulationControllerControls(values) };
        Assert.Equal((short)8192, processing.NativeAnalogState(controller, 2, 8));
        Assert.Equal((short)0, processing.NativeAnalogState(controller, 2, 12));
        Assert.Equal((short)24575, processing.NativeAnalogState(controller, 2, 13));
        foreach (var id in new uint[] { 0, 1, 9, 16, 32, 256, uint.MaxValue })
            Assert.Equal((short)0, processing.NativeAnalogState(controller, 2, id));
        Assert.Equal(short.MaxValue, processing.NativeAnalogState(controller, 2, 3));
        Assert.Equal((short)0, processing.NativeAnalogState(EmulationControllerState.Empty, 2, 8));
        Assert.Equal(1234, controller.LeftTrigger);
    }

    [Fact]
    public void SonyAnalogButtonsUseTheSamePressureReader()
    {
        var assembly = typeof(GWGUI.Emulation.Sony.Modules.SonyEmulationModuleFactory).Assembly;
        var functions = assembly.GetType("GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.ControllerInputFunctions", throwOnError: true)!;
        var buttonType = assembly.GetType("GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums.JoypadInput", throwOnError: true)!;
        var read = functions.GetMethod("AnalogButton", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        var controllers = new[]
        {
            EmulationControllerState.Empty,
            EmulationControllerState.Empty with { Buttons = ushort.MaxValue, LeftTrigger = 8192, RightTrigger = 24575 },
            EmulationControllerState.Empty with { Buttons = ushort.MaxValue, Controls = new EmulationControllerControls(new Dictionary<string, float> { ["A"] = .25f, ["B"] = 0, ["L2"] = .75f }) }
        };
        foreach (var controller in controllers)
        for (uint id = 0; id < 16; id++)
            Assert.Equal(EmulationSdk.InputProcessing.NativeAnalogState(controller, 2, id), (short)read.Invoke(null, [controller, Enum.ToObject(buttonType, id)])!);
        Assert.Equal((short)0, (short)read.Invoke(null, [controllers[1], Enum.ToObject(buttonType, uint.MaxValue)])!);
    }

    [Fact]
    public void NativePointingReadsTheSelectedPortAndItsAbsoluteCoordinates()
    {
        var processing = EmulationSdk.InputProcessing;
        var commands = new[] { "B", "A", "X", "Y", "Start", "Select", "Up", "Down", "Left", "Right", "Reload" };
        var controller = EmulationControllerState.Empty with { LeftX = -1234, LeftY = 5678, Controls = new EmulationControllerControls(commands.Select(command => KeyValuePair.Create(command, 1f))) };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [EmulationControllerState.Empty, controller] };
        foreach (var device in new uint[] { 4, 260 })
        {
            Assert.Equal((short)-1234, processing.ReadNativeInput(snapshot, 1, device, 0, 13));
            Assert.Equal((short)5678, processing.ReadNativeInput(snapshot, 1, device, 0, 14));
            Assert.Equal((short)0, processing.ReadNativeInput(snapshot, 1, device, 0, 15));
            foreach (var id in new uint[] { 2, 3, 4, 6, 7, 8, 9, 10, 11, 12, 16 })
            {
                Assert.Equal((short)1, processing.ReadNativeInput(snapshot, 1, device, 0, id));
                Assert.Equal((short)0, processing.ReadNativeInput(snapshot, 0, device, 0, id));
            }
        }
        foreach (var device in new uint[] { 6, 262 })
        {
            Assert.Equal((short)-1234, processing.ReadNativeInput(snapshot, 1, device, 0, 0));
            Assert.Equal((short)5678, processing.ReadNativeInput(snapshot, 1, device, 0, 1));
            Assert.Equal((short)1, processing.ReadNativeInput(snapshot, 1, device, 0, 2));
            Assert.Equal((short)1, processing.ReadNativeInput(snapshot, 1, device, 0, 3));
            Assert.Equal((short)0, processing.ReadNativeInput(snapshot, 0, device, 0, 3));
        }
        foreach (var device in new uint[] { 4, 6 })
        {
            Assert.Equal((short)0, processing.ReadNativeInput(snapshot, 2, device, 0, 2));
            Assert.Equal((short)0, processing.ReadNativeInput(snapshot, 1, device, 1, 2));
            Assert.Equal((short)0, processing.ReadNativeInput(snapshot, 1, device, 0, uint.MaxValue));
            foreach (var edge in new[] { short.MinValue, short.MaxValue })
            {
                var horizontal = snapshot with { Controllers = [controller with { LeftX = edge }] };
                var vertical = snapshot with { Controllers = [controller with { LeftY = edge }] };
                Assert.Equal((short)1, processing.ReadNativeInput(horizontal, 0, device, 0, 15));
                Assert.Equal((short)1, processing.ReadNativeInput(vertical, 0, device, 0, 15));
            }
        }
    }
}
