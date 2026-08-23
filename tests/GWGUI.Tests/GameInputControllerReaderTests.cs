using GWGUI.App.Services.Input.GameInput;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class GameInputControllerReaderTests
{
    [Fact]
    public void DeviceEnumerationKeepsRawControllersKeyboardAndMouseOnSeparateCallbacks()
    {
        var filters = GameInputControllerReader.RegisteredDeviceCallbackFilters;

        Assert.Equal(4, filters.Count);
        Assert.Contains(GameInputKind.RawDeviceReport, filters);
        Assert.Contains(GameInputKind.Keyboard, filters);
        Assert.Contains(GameInputKind.Mouse, filters);
        var controllers = Assert.Single(filters, filter =>
            (filter & GameInputKind.Gamepad) != 0);
        Assert.True((controllers & GameInputKind.Controller) != 0);
        Assert.DoesNotContain(filters, filter =>
            (filter & GameInputKind.RawDeviceReport) != 0 &&
            (filter & GameInputKind.Gamepad) != 0);
    }

    [Fact]
    public void ManualControllerRefreshOnlyEnumeratesControllerFamilies()
    {
        var filters = GameInputControllerReader.RegisteredControllerRefreshFilters;

        Assert.Equal(2, filters.Count);
        Assert.Contains(GameInputKind.RawDeviceReport, filters);
        Assert.Single(filters, filter => (filter & GameInputKind.Gamepad) != 0);
        Assert.DoesNotContain(GameInputKind.Keyboard, filters);
        Assert.DoesNotContain(GameInputKind.Mouse, filters);
    }

    [Fact]
    public void MapGamepad_PreservesExistingButtonAxisAndTriggerLayout()
    {
        var native = new GameInputGamepadState
        {
            Buttons = GameInputGamepadButtons.DPadUp | GameInputGamepadButtons.Menu |
                GameInputGamepadButtons.A | GameInputGamepadButtons.B,
            LeftTrigger = 1f,
            RightTrigger = .13f,
            LeftThumbstickX = 123f / short.MaxValue,
            LeftThumbstickY = -1f,
            RightThumbstickX = -456f / short.MaxValue,
            RightThumbstickY = -1000f / short.MaxValue
        };
        var state = GameInputControllerReader.MapGamepad("gameinput:test", native);

        Assert.NotEqual(0u, state.Buttons & (1u << 0));
        Assert.NotEqual(0u, state.Buttons & (1u << 3));
        Assert.NotEqual(0u, state.Buttons & (1u << 4));
        Assert.NotEqual(0u, state.Buttons & (1u << 8));
        Assert.NotEqual(0u, state.Buttons & (1u << 12));
        Assert.NotEqual(0u, state.Buttons & (1u << 13));
        Assert.Equal((short)123, state.LeftX);
        Assert.Equal(short.MaxValue, state.LeftY);
        Assert.Equal((short)-456, state.RightX);
        Assert.Equal((short)1000, state.RightY);
        Assert.Equal(short.MaxValue, state.LeftTrigger);
        Assert.True(state.RightTrigger > 0);
        Assert.Equal("gameinput:test", state.DeviceId);
    }
}
