using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options.ControllerVisualization;

namespace GWGUI.Tests.Interface;

public sealed class ControllerVisualMappingTests
{
    [Theory]
    [InlineData("gamepad", 22)]
    [InlineData("racing", 14)]
    [InlineData("flight", 14)]
    [InlineData("arcade", 10)]
    public void EveryOriginalPairDrivesTheSameIndividualAndLiveControl(string family, int expectedCount)
    {
        var rows = Rows(family).ToArray();
        Assert.Equal(expectedCount, rows.Length);
        foreach (var row in rows)
        {
            Assert.Equal(row.Control, Map(family, row.Button));
            var state = ControllerVisualStateConverter.Convert(WithButtons(family, row.Button));
            Assert.Equal(row.Control, Assert.Single(state.StandardControls));
        }
    }

    [Theory]
    [InlineData("gamepad")]
    [InlineData("racing")]
    [InlineData("flight")]
    [InlineData("arcade")]
    public void LiveMasksCombineControlsWhileIndividualLookupRequiresOneExactButton(string family)
    {
        var rows = Rows(family).ToArray();
        var combined = rows[0].Button | rows[1].Button;
        var state = ControllerVisualStateConverter.Convert(WithButtons(family, combined));
        Assert.True(state.StandardControls.SetEquals([rows[0].Control, rows[1].Control]));
        AssertUnknownMapping(family, combined);
        var all = rows.Aggregate(0u, (mask, row) => mask | row.Button);
        state = ControllerVisualStateConverter.Convert(WithButtons(family, all));
        Assert.True(state.StandardControls.SetEquals(rows.Select(row => row.Control)));
    }

    [Theory]
    [InlineData("gamepad")]
    [InlineData("racing")]
    [InlineData("flight")]
    [InlineData("arcade")]
    public void ZeroAndUnknownBitsKeepTheirOriginalPolicies(string family)
    {
        const uint unknown = 0x80000000;
        Assert.Empty(ControllerVisualStateConverter.Convert(WithButtons(family, 0)).StandardControls);
        Assert.Empty(ControllerVisualStateConverter.Convert(WithButtons(family, unknown)).StandardControls);
        AssertUnknownMapping(family, 0);
        AssertUnknownMapping(family, unknown);
        var known = Rows(family).First();
        var state = ControllerVisualStateConverter.Convert(WithButtons(family, known.Button | unknown));
        Assert.Equal(known.Control, Assert.Single(state.StandardControls));
        AssertUnknownMapping(family, known.Button | unknown);
    }

    [Fact]
    public void SharedRowsCannotBeChangedThroughMutableListInterfaces()
    {
        CannotWrite(ControllerVisualControlMappings.GamepadMappings);
        CannotWrite(ControllerVisualControlMappings.RacingMappings);
        CannotWrite(ControllerVisualControlMappings.FlightMappings);
        CannotWrite(ControllerVisualControlMappings.ArcadeMappings);
    }

    [Fact]
    public void MissingLiveStateStillHasNoControls()
    {
        var state = ControllerVisualStateConverter.Convert(null);
        Assert.Empty(state.StandardControls);
        Assert.Empty(state.LabeledControls);
        Assert.Empty(state.RawAxisValues);
    }

    private static void CannotWrite<T>(IReadOnlyList<(T Button, ControllerVisualControl Control)> rows)
    {
        if (rows is IList<(T Button, ControllerVisualControl Control)> list)
            Assert.Throws<NotSupportedException>(() => list[0] = list[0]);
    }

    private static void AssertUnknownMapping(string family, uint button)
    {
        if (family == "gamepad") { Assert.Null(Map(family, button)); return; }
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => Map(family, button));
        Assert.Equal("button", error.ParamName);
        Assert.Equal(button, Convert.ToUInt32(error.ActualValue));
    }

    private static IEnumerable<(uint Button, ControllerVisualControl Control)> Rows(string family) => family switch
    {
        "gamepad" => ControllerVisualControlMappings.GamepadMappings.Select(row => ((uint)row.Button, row.Control)),
        "racing" => ControllerVisualControlMappings.RacingMappings.Select(row => ((uint)row.Button, row.Control)),
        "flight" => ControllerVisualControlMappings.FlightMappings.Select(row => ((uint)row.Button, row.Control)),
        "arcade" => ControllerVisualControlMappings.ArcadeMappings.Select(row => ((uint)row.Button, row.Control)),
        _ => throw new ArgumentOutOfRangeException(nameof(family))
    };

    private static ControllerVisualControl? Map(string family, uint button) => family switch
    {
        "gamepad" => ControllerVisualControlMappings.GamepadControl((GameInputGamepadButtons)button),
        "racing" => ControllerVisualControlMappings.RacingControl((GameInputRacingWheelButtons)button),
        "flight" => ControllerVisualControlMappings.FlightControl((GameInputFlightStickButtons)button),
        "arcade" => ControllerVisualControlMappings.ArcadeControl((GameInputArcadeStickButtons)button),
        _ => throw new ArgumentOutOfRangeException(nameof(family))
    };

    private static GameInputLiveState WithButtons(string family, uint button)
    {
        var state = GameInputLiveState.Empty("managed-test");
        return family switch
        {
            "gamepad" => state with { Gamepad = new GameInputGamepadState { Buttons = (GameInputGamepadButtons)button } },
            "racing" => state with { RacingWheel = new GameInputRacingWheelState { Buttons = (GameInputRacingWheelButtons)button } },
            "flight" => state with { FlightStick = new GameInputFlightStickState { Buttons = (GameInputFlightStickButtons)button, HatSwitch = GameInputSwitchPosition.Center } },
            "arcade" => state with { ArcadeStick = new GameInputArcadeStickState { Buttons = (GameInputArcadeStickButtons)button } },
            _ => throw new ArgumentOutOfRangeException(nameof(family))
        };
    }
}
