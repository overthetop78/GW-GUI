using System.Runtime.InteropServices;
using GWGUI.App.Services.Input.GameInput;

namespace GWGUI.Tests;

// Values audited against Microsoft.GameInput 3.5.268 GameInput.h on 2026-08-23.
// UiNavigation is the single runtime-observed extension absent from that header.
public sealed class GameInputEnumLayoutTests
{
    [Fact]
    public void EveryGameInputEnumNameAndValueMatchesTheAuditedAbi()
    {
        AssertEnum<GameInputKind>(
            ("Unknown", 0x0000000000000000UL),
            ("RawDeviceReport", 0x0000000000000001UL),
            ("ControllerAxis", 0x0000000000000002UL),
            ("ControllerButton", 0x0000000000000004UL),
            ("ControllerSwitch", 0x0000000000000008UL),
            ("Controller", 0x000000000000000EUL),
            ("Keyboard", 0x0000000000000010UL),
            ("Mouse", 0x0000000000000020UL),
            ("Sensors", 0x0000000000000040UL),
            ("ArcadeStick", 0x0000000000010000UL),
            ("FlightStick", 0x0000000000020000UL),
            ("Gamepad", 0x0000000000040000UL),
            ("RacingWheel", 0x0000000000080000UL),
            ("UiNavigation", 0x0000000001000000UL)
        );

        AssertEnum<GameInputEnumerationKind>(
            ("None", 0x0000000000000000UL),
            ("Async", 0x0000000000000001UL),
            ("Blocking", 0x0000000000000002UL)
        );

        AssertEnum<GameInputFocusPolicy>(
            ("Default", 0x0000000000000000UL),
            ("ExclusiveForegroundInput", 0x0000000000000002UL),
            ("ExclusiveForegroundGuideButton", 0x0000000000000008UL),
            ("ExclusiveForegroundShareButton", 0x0000000000000020UL),
            ("EnableBackgroundInput", 0x0000000000000040UL),
            ("EnableBackgroundGuideButton", 0x0000000000000080UL),
            ("EnableBackgroundShareButton", 0x0000000000000100UL)
        );

        AssertEnum<GameInputSwitchKind>(
            ("Unknown", 0xFFFFFFFFFFFFFFFFUL),
            ("TwoWay", 0x0000000000000000UL),
            ("FourWay", 0x0000000000000001UL),
            ("EightWay", 0x0000000000000002UL)
        );

        AssertEnum<GameInputSwitchPosition>(
            ("Center", 0x0000000000000000UL),
            ("Up", 0x0000000000000001UL),
            ("UpRight", 0x0000000000000002UL),
            ("Right", 0x0000000000000003UL),
            ("DownRight", 0x0000000000000004UL),
            ("Down", 0x0000000000000005UL),
            ("DownLeft", 0x0000000000000006UL),
            ("Left", 0x0000000000000007UL),
            ("UpLeft", 0x0000000000000008UL)
        );

        AssertEnum<GameInputKeyboardKind>(
            ("Unknown", 0xFFFFFFFFFFFFFFFFUL),
            ("Ansi", 0x0000000000000000UL),
            ("Iso", 0x0000000000000001UL),
            ("Ks", 0x0000000000000002UL),
            ("Abnt", 0x0000000000000003UL),
            ("Jis", 0x0000000000000004UL)
        );

        AssertEnum<GameInputMouseButtons>(
            ("None", 0x0000000000000000UL),
            ("Left", 0x0000000000000001UL),
            ("Right", 0x0000000000000002UL),
            ("Middle", 0x0000000000000004UL),
            ("Button4", 0x0000000000000008UL),
            ("Button5", 0x0000000000000010UL),
            ("WheelTiltLeft", 0x0000000000000020UL),
            ("WheelTiltRight", 0x0000000000000040UL)
        );

        AssertEnum<GameInputMousePositions>(
            ("None", 0x0000000000000000UL),
            ("Absolute", 0x0000000000000001UL),
            ("Relative", 0x0000000000000002UL)
        );

        AssertEnum<GameInputSensorsKind>(
            ("None", 0x0000000000000000UL),
            ("Accelerometer", 0x0000000000000001UL),
            ("Gyrometer", 0x0000000000000002UL),
            ("Compass", 0x0000000000000004UL),
            ("Orientation", 0x0000000000000008UL)
        );

        AssertEnum<GameInputSensorAccuracy>(
            ("Unknown", 0x0000000000000000UL),
            ("Unreliable", 0x0000000000000001UL),
            ("Approximate", 0x0000000000000002UL),
            ("High", 0x0000000000000003UL)
        );

        AssertEnum<GameInputArcadeStickButtons>(
            ("None", 0x0000000000000000UL),
            ("Menu", 0x0000000000000001UL),
            ("View", 0x0000000000000002UL),
            ("Up", 0x0000000000000004UL),
            ("Down", 0x0000000000000008UL),
            ("Left", 0x0000000000000010UL),
            ("Right", 0x0000000000000020UL),
            ("Action1", 0x0000000000000040UL),
            ("Action2", 0x0000000000000080UL),
            ("Action3", 0x0000000000000100UL),
            ("Action4", 0x0000000000000200UL),
            ("Action5", 0x0000000000000400UL),
            ("Action6", 0x0000000000000800UL),
            ("Special1", 0x0000000000001000UL),
            ("Special2", 0x0000000000002000UL)
        );

        AssertEnum<GameInputFlightStickButtons>(
            ("None", 0x0000000000000000UL),
            ("Menu", 0x0000000000000001UL),
            ("View", 0x0000000000000002UL),
            ("FirePrimary", 0x0000000000000004UL),
            ("FireSecondary", 0x0000000000000008UL),
            ("HatSwitchUp", 0x0000000000000010UL),
            ("HatSwitchDown", 0x0000000000000020UL),
            ("HatSwitchLeft", 0x0000000000000040UL),
            ("HatSwitchRight", 0x0000000000000080UL),
            ("A", 0x0000000000000100UL),
            ("B", 0x0000000000000200UL),
            ("X", 0x0000000000000400UL),
            ("Y", 0x0000000000000800UL),
            ("LeftShoulder", 0x0000000000001000UL),
            ("RightShoulder", 0x0000000000002000UL)
        );

        AssertEnum<GameInputGamepadButtons>(
            ("None", 0x0000000000000000UL),
            ("Menu", 0x0000000000000001UL),
            ("View", 0x0000000000000002UL),
            ("A", 0x0000000000000004UL),
            ("B", 0x0000000000000008UL),
            ("X", 0x0000000000000010UL),
            ("Y", 0x0000000000000020UL),
            ("DPadUp", 0x0000000000000040UL),
            ("DPadDown", 0x0000000000000080UL),
            ("DPadLeft", 0x0000000000000100UL),
            ("DPadRight", 0x0000000000000200UL),
            ("LeftShoulder", 0x0000000000000400UL),
            ("RightShoulder", 0x0000000000000800UL),
            ("LeftThumbstick", 0x0000000000001000UL),
            ("RightThumbstick", 0x0000000000002000UL),
            ("C", 0x0000000000004000UL),
            ("Z", 0x0000000000008000UL),
            ("LeftTriggerButton", 0x0000000000010000UL),
            ("RightTriggerButton", 0x0000000000020000UL),
            ("LeftThumbstickUp", 0x0000000000040000UL),
            ("LeftThumbstickDown", 0x0000000000080000UL),
            ("LeftThumbstickLeft", 0x0000000000100000UL),
            ("LeftThumbstickRight", 0x0000000000200000UL),
            ("RightThumbstickUp", 0x0000000000400000UL),
            ("RightThumbstickDown", 0x0000000000800000UL),
            ("RightThumbstickLeft", 0x0000000001000000UL),
            ("RightThumbstickRight", 0x0000000002000000UL),
            ("PaddleLeft1", 0x0000000004000000UL),
            ("PaddleLeft2", 0x0000000008000000UL),
            ("PaddleRight1", 0x0000000010000000UL),
            ("PaddleRight2", 0x0000000020000000UL)
        );

        AssertEnum<GameInputRawDeviceReportKind>(
            ("Input", 0x0000000000000000UL),
            ("Output", 0x0000000000000001UL)
        );

        AssertEnum<GameInputRacingWheelButtons>(
            ("None", 0x0000000000000000UL),
            ("Menu", 0x0000000000000001UL),
            ("View", 0x0000000000000002UL),
            ("PreviousGear", 0x0000000000000004UL),
            ("NextGear", 0x0000000000000008UL),
            ("DPadUp", 0x0000000000000010UL),
            ("DPadDown", 0x0000000000000020UL),
            ("DPadLeft", 0x0000000000000040UL),
            ("DPadRight", 0x0000000000000080UL),
            ("A", 0x0000000000000100UL),
            ("B", 0x0000000000000200UL),
            ("X", 0x0000000000000400UL),
            ("Y", 0x0000000000000800UL),
            ("LeftThumbstick", 0x0000000000001000UL),
            ("RightThumbstick", 0x0000000000002000UL)
        );

        AssertEnum<GameInputSystemButtons>(
            ("None", 0x0000000000000000UL),
            ("Guide", 0x0000000000000001UL),
            ("Share", 0x0000000000000002UL)
        );

        AssertEnum<GameInputFlightStickAxes>(
            ("None", 0x0000000000000000UL),
            ("Roll", 0x0000000000000010UL),
            ("Pitch", 0x0000000000000020UL),
            ("Yaw", 0x0000000000000040UL),
            ("Throttle", 0x0000000000000080UL)
        );

        AssertEnum<GameInputGamepadAxes>(
            ("None", 0x0000000000000000UL),
            ("LeftTrigger", 0x0000000000000001UL),
            ("RightTrigger", 0x0000000000000002UL),
            ("LeftThumbstickX", 0x0000000000000004UL),
            ("LeftThumbstickY", 0x0000000000000008UL),
            ("RightThumbstickX", 0x0000000000000010UL),
            ("RightThumbstickY", 0x0000000000000020UL)
        );

        AssertEnum<GameInputRacingWheelAxes>(
            ("None", 0x0000000000000000UL),
            ("Steering", 0x0000000000000100UL),
            ("Throttle", 0x0000000000000200UL),
            ("Brake", 0x0000000000000400UL),
            ("Clutch", 0x0000000000000800UL),
            ("Handbrake", 0x0000000000001000UL),
            ("PatternShifter", 0x0000000000002000UL)
        );

        AssertEnum<GameInputDeviceStatus>(
            ("None", 0x0000000000000000UL),
            ("Connected", 0x0000000000000001UL),
            ("HapticInfoReady", 0x0000000000200000UL),
            ("Any", 0x00000000FFFFFFFFUL)
        );

        AssertEnum<GameInputDeviceFamily>(
            ("Virtual", 0xFFFFFFFFFFFFFFFFUL),
            ("Unknown", 0x0000000000000000UL),
            ("XboxOne", 0x0000000000000001UL),
            ("Xbox360", 0x0000000000000002UL),
            ("Hid", 0x0000000000000003UL),
            ("I8042", 0x0000000000000004UL),
            ("Aggregate", 0x0000000000000005UL)
        );

        AssertEnum<GameInputLabel>(
            ("Unknown", 0xFFFFFFFFFFFFFFFFUL),
            ("None", 0x0000000000000000UL),
            ("XboxGuide", 0x0000000000000001UL),
            ("XboxBack", 0x0000000000000002UL),
            ("XboxStart", 0x0000000000000003UL),
            ("XboxMenu", 0x0000000000000004UL),
            ("XboxView", 0x0000000000000005UL),
            ("XboxA", 0x0000000000000007UL),
            ("XboxB", 0x0000000000000008UL),
            ("XboxX", 0x0000000000000009UL),
            ("XboxY", 0x000000000000000AUL),
            ("XboxDPadUp", 0x000000000000000BUL),
            ("XboxDPadDown", 0x000000000000000CUL),
            ("XboxDPadLeft", 0x000000000000000DUL),
            ("XboxDPadRight", 0x000000000000000EUL),
            ("XboxLeftShoulder", 0x000000000000000FUL),
            ("XboxLeftTrigger", 0x0000000000000010UL),
            ("XboxLeftStickButton", 0x0000000000000011UL),
            ("XboxRightShoulder", 0x0000000000000012UL),
            ("XboxRightTrigger", 0x0000000000000013UL),
            ("XboxRightStickButton", 0x0000000000000014UL),
            ("XboxPaddle1", 0x0000000000000015UL),
            ("XboxPaddle2", 0x0000000000000016UL),
            ("XboxPaddle3", 0x0000000000000017UL),
            ("XboxPaddle4", 0x0000000000000018UL),
            ("LetterA", 0x0000000000000019UL),
            ("LetterB", 0x000000000000001AUL),
            ("LetterC", 0x000000000000001BUL),
            ("LetterD", 0x000000000000001CUL),
            ("LetterE", 0x000000000000001DUL),
            ("LetterF", 0x000000000000001EUL),
            ("LetterG", 0x000000000000001FUL),
            ("LetterH", 0x0000000000000020UL),
            ("LetterI", 0x0000000000000021UL),
            ("LetterJ", 0x0000000000000022UL),
            ("LetterK", 0x0000000000000023UL),
            ("LetterL", 0x0000000000000024UL),
            ("LetterM", 0x0000000000000025UL),
            ("LetterN", 0x0000000000000026UL),
            ("LetterO", 0x0000000000000027UL),
            ("LetterP", 0x0000000000000028UL),
            ("LetterQ", 0x0000000000000029UL),
            ("LetterR", 0x000000000000002AUL),
            ("LetterS", 0x000000000000002BUL),
            ("LetterT", 0x000000000000002CUL),
            ("LetterU", 0x000000000000002DUL),
            ("LetterV", 0x000000000000002EUL),
            ("LetterW", 0x000000000000002FUL),
            ("LetterX", 0x0000000000000030UL),
            ("LetterY", 0x0000000000000031UL),
            ("LetterZ", 0x0000000000000032UL),
            ("Number0", 0x0000000000000033UL),
            ("Number1", 0x0000000000000034UL),
            ("Number2", 0x0000000000000035UL),
            ("Number3", 0x0000000000000036UL),
            ("Number4", 0x0000000000000037UL),
            ("Number5", 0x0000000000000038UL),
            ("Number6", 0x0000000000000039UL),
            ("Number7", 0x000000000000003AUL),
            ("Number8", 0x000000000000003BUL),
            ("Number9", 0x000000000000003CUL),
            ("ArrowUp", 0x000000000000003DUL),
            ("ArrowUpRight", 0x000000000000003EUL),
            ("ArrowRight", 0x000000000000003FUL),
            ("ArrowDownRight", 0x0000000000000040UL),
            ("ArrowDown", 0x0000000000000041UL),
            ("ArrowDownLeft", 0x0000000000000042UL),
            ("ArrowLeft", 0x0000000000000043UL),
            ("ArrowUpLeft", 0x0000000000000044UL),
            ("ArrowUpDown", 0x0000000000000045UL),
            ("ArrowLeftRight", 0x0000000000000046UL),
            ("ArrowUpDownLeftRight", 0x0000000000000047UL),
            ("ArrowClockwise", 0x0000000000000048UL),
            ("ArrowCounterClockwise", 0x0000000000000049UL),
            ("ArrowReturn", 0x000000000000004AUL),
            ("IconBranding", 0x000000000000004BUL),
            ("IconHome", 0x000000000000004CUL),
            ("IconMenu", 0x000000000000004DUL),
            ("IconCross", 0x000000000000004EUL),
            ("IconCircle", 0x000000000000004FUL),
            ("IconSquare", 0x0000000000000050UL),
            ("IconTriangle", 0x0000000000000051UL),
            ("IconStar", 0x0000000000000052UL),
            ("IconDPadUp", 0x0000000000000053UL),
            ("IconDPadDown", 0x0000000000000054UL),
            ("IconDPadLeft", 0x0000000000000055UL),
            ("IconDPadRight", 0x0000000000000056UL),
            ("IconDialClockwise", 0x0000000000000057UL),
            ("IconDialCounterClockwise", 0x0000000000000058UL),
            ("IconSliderLeftRight", 0x0000000000000059UL),
            ("IconSliderUpDown", 0x000000000000005AUL),
            ("IconWheelUpDown", 0x000000000000005BUL),
            ("IconPlus", 0x000000000000005CUL),
            ("IconMinus", 0x000000000000005DUL),
            ("IconSuspension", 0x000000000000005EUL),
            ("Home", 0x000000000000005FUL),
            ("Guide", 0x0000000000000060UL),
            ("Mode", 0x0000000000000061UL),
            ("Select", 0x0000000000000062UL),
            ("Menu", 0x0000000000000063UL),
            ("View", 0x0000000000000064UL),
            ("Back", 0x0000000000000065UL),
            ("Start", 0x0000000000000066UL),
            ("Options", 0x0000000000000067UL),
            ("Share", 0x0000000000000068UL),
            ("Up", 0x0000000000000069UL),
            ("Down", 0x000000000000006AUL),
            ("Left", 0x000000000000006BUL),
            ("Right", 0x000000000000006CUL),
            ("LB", 0x000000000000006DUL),
            ("LT", 0x000000000000006EUL),
            ("LSB", 0x000000000000006FUL),
            ("L1", 0x0000000000000070UL),
            ("L2", 0x0000000000000071UL),
            ("L3", 0x0000000000000072UL),
            ("RB", 0x0000000000000073UL),
            ("RT", 0x0000000000000074UL),
            ("RSB", 0x0000000000000075UL),
            ("R1", 0x0000000000000076UL),
            ("R2", 0x0000000000000077UL),
            ("R3", 0x0000000000000078UL),
            ("PaddleLeft1", 0x0000000000000079UL),
            ("PaddleLeft2", 0x000000000000007AUL),
            ("PaddleRight1", 0x000000000000007BUL),
            ("PaddleRight2", 0x000000000000007CUL)
        );

        AssertEnum<GameInputFeedbackAxes>(
            ("None", 0x0000000000000000UL),
            ("LinearX", 0x0000000000000001UL),
            ("LinearY", 0x0000000000000002UL),
            ("LinearZ", 0x0000000000000004UL),
            ("AngularX", 0x0000000000000008UL),
            ("AngularY", 0x0000000000000010UL),
            ("AngularZ", 0x0000000000000020UL),
            ("Normal", 0x0000000000000040UL)
        );

        AssertEnum<GameInputFeedbackEffectState>(
            ("Stopped", 0x0000000000000000UL),
            ("Running", 0x0000000000000001UL),
            ("Paused", 0x0000000000000002UL)
        );

        AssertEnum<GameInputForceFeedbackEffectKind>(
            ("Constant", 0x0000000000000000UL),
            ("Ramp", 0x0000000000000001UL),
            ("SineWave", 0x0000000000000002UL),
            ("SquareWave", 0x0000000000000003UL),
            ("TriangleWave", 0x0000000000000004UL),
            ("SawtoothUpWave", 0x0000000000000005UL),
            ("SawtoothDownWave", 0x0000000000000006UL),
            ("Spring", 0x0000000000000007UL),
            ("Friction", 0x0000000000000008UL),
            ("Damper", 0x0000000000000009UL),
            ("Inertia", 0x000000000000000AUL)
        );

        AssertEnum<GameInputRumbleMotors>(
            ("None", 0x0000000000000000UL),
            ("LowFrequency", 0x0000000000000001UL),
            ("HighFrequency", 0x0000000000000002UL),
            ("LeftTrigger", 0x0000000000000004UL),
            ("RightTrigger", 0x0000000000000008UL)
        );

        AssertEnum<GameInputElementKind>(
            ("None", 0x0000000000000000UL),
            ("Axis", 0x0000000000000001UL),
            ("Button", 0x0000000000000002UL),
            ("Switch", 0x0000000000000003UL)
        );
    }

    [Fact]
    public void RumbleParameterLayoutMatchesGameInputHeader()
    {
        Assert.Equal(16, Marshal.SizeOf<GameInputRumbleParams>());
        Assert.Equal(0, Marshal.OffsetOf<GameInputRumbleParams>(nameof(GameInputRumbleParams.LowFrequency)).ToInt32());
        Assert.Equal(4, Marshal.OffsetOf<GameInputRumbleParams>(nameof(GameInputRumbleParams.HighFrequency)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<GameInputRumbleParams>(nameof(GameInputRumbleParams.LeftTrigger)).ToInt32());
        Assert.Equal(12, Marshal.OffsetOf<GameInputRumbleParams>(nameof(GameInputRumbleParams.RightTrigger)).ToInt32());
    }

    private static void AssertEnum<T>(params (string Name, ulong Value)[] expected)
        where T : struct, Enum
    {
        var actual = Enum.GetValues<T>().ToDictionary(
            value => value.ToString(),
            Bits,
            StringComparer.Ordinal);
        Assert.Equal(expected.Length, actual.Count);
        foreach (var (name, value) in expected)
        {
            Assert.True(actual.TryGetValue(name, out var actualValue), $"Missing {typeof(T).Name}.{name}.");
            Assert.Equal(value, actualValue);
        }
    }

    private static ulong Bits<T>(T value)
        where T : struct, Enum
    {
        var type = Enum.GetUnderlyingType(typeof(T));
        return Type.GetTypeCode(type) switch
        {
            TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 =>
                unchecked((ulong)Convert.ToInt64(value)),
            _ => Convert.ToUInt64(value)
        };
    }
}
