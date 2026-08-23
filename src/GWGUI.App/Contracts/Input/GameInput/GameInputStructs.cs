using System.Runtime.InteropServices;

namespace GWGUI.App.Services.Input.GameInput;

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputKeyState
{
    internal uint ScanCode;
    internal uint CodePoint;
    internal byte VirtualKey;
    [MarshalAs(UnmanagedType.I1)] internal bool IsDeadKey;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputMouseState
{
    internal GameInputMouseButtons Buttons;
    internal GameInputMousePositions Positions;
    internal long PositionX;
    internal long PositionY;
    internal long AbsolutePositionX;
    internal long AbsolutePositionY;
    internal long WheelX;
    internal long WheelY;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputVersion
{
    internal ushort Major;
    internal ushort Minor;
    internal ushort Build;
    internal ushort Revision;

    public override readonly string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputSensorsState
{
    internal float AccelerationInGX;
    internal float AccelerationInGY;
    internal float AccelerationInGZ;
    internal float AngularVelocityInRadPerSecX;
    internal float AngularVelocityInRadPerSecY;
    internal float AngularVelocityInRadPerSecZ;
    internal float HeadingInDegreesFromMagneticNorth;
    internal GameInputSensorAccuracy HeadingAccuracy;
    internal float OrientationW;
    internal float OrientationX;
    internal float OrientationY;
    internal float OrientationZ;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputArcadeStickState { internal GameInputArcadeStickButtons Buttons; }

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputFlightStickState
{
    internal GameInputFlightStickButtons Buttons;
    internal GameInputSwitchPosition HatSwitch;
    internal float Roll;
    internal float Pitch;
    internal float Yaw;
    internal float Throttle;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputGamepadState
{
    internal GameInputGamepadButtons Buttons;
    internal float LeftTrigger;
    internal float RightTrigger;
    internal float LeftThumbstickX;
    internal float LeftThumbstickY;
    internal float RightThumbstickX;
    internal float RightThumbstickY;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputRacingWheelState
{
    internal GameInputRacingWheelButtons Buttons;
    internal int PatternShifterGear;
    internal float Wheel;
    internal float Throttle;
    internal float Brake;
    internal float Clutch;
    internal float Handbrake;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputUsage { internal ushort Page; internal ushort Id; }

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct AppLocalDeviceId
{
    internal fixed byte Value[32];

    internal readonly string ToHex()
    {
        fixed (byte* value = Value)
            return Convert.ToHexString(new ReadOnlySpan<byte>(value, 32));
    }
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct GameInputControllerSwitchInfo
{
    internal fixed int Labels[8];
    internal GameInputSwitchKind Kind;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputControllerInfo
{
    internal uint AxisCount;
    internal IntPtr AxisLabels;
    internal uint ButtonCount;
    internal IntPtr ButtonLabels;
    internal uint SwitchCount;
    internal IntPtr SwitchInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputKeyboardInfo
{
    internal GameInputKeyboardKind Kind;
    internal uint Layout;
    internal uint KeyCount;
    internal uint FunctionKeyCount;
    internal uint MaxSimultaneousKeys;
    internal uint PlatformType;
    internal uint PlatformSubtype;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputMouseInfo
{
    internal GameInputMouseButtons SupportedButtons;
    internal uint SampleRate;
    [MarshalAs(UnmanagedType.I1)] internal bool HasWheelX;
    [MarshalAs(UnmanagedType.I1)] internal bool HasWheelY;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputSensorsInfo { internal GameInputSensorsKind SupportedSensors; }

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputArcadeStickInfo
{
    internal GameInputLabel MenuButtonLabel;
    internal GameInputLabel ViewButtonLabel;
    internal GameInputLabel StickUpLabel;
    internal GameInputLabel StickDownLabel;
    internal GameInputLabel StickLeftLabel;
    internal GameInputLabel StickRightLabel;
    internal GameInputLabel ActionButton1Label;
    internal GameInputLabel ActionButton2Label;
    internal GameInputLabel ActionButton3Label;
    internal GameInputLabel ActionButton4Label;
    internal GameInputLabel ActionButton5Label;
    internal GameInputLabel ActionButton6Label;
    internal GameInputLabel SpecialButton1Label;
    internal GameInputLabel SpecialButton2Label;
    internal uint ExtraButtonCount;
    internal uint ExtraAxisCount;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputFlightStickInfo
{
    internal GameInputLabel MenuButtonLabel;
    internal GameInputLabel ViewButtonLabel;
    internal GameInputLabel FirePrimaryButtonLabel;
    internal GameInputLabel FireSecondaryButtonLabel;
    internal GameInputLabel HatSwitchUpLabel;
    internal GameInputLabel HatSwitchDownLabel;
    internal GameInputLabel HatSwitchLeftLabel;
    internal GameInputLabel HatSwitchRightLabel;
    internal GameInputLabel AButtonLabel;
    internal GameInputLabel BButtonLabel;
    internal GameInputLabel XButtonLabel;
    internal GameInputLabel YButtonLabel;
    internal GameInputLabel LeftShoulderButtonLabel;
    internal GameInputLabel RightShoulderButtonLabel;
    internal uint ExtraButtonCount;
    internal uint ExtraAxisCount;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputGamepadInfo
{
    internal GameInputGamepadButtons SupportedLayout;
    internal GameInputLabel MenuButtonLabel;
    internal GameInputLabel ViewButtonLabel;
    internal GameInputLabel AButtonLabel;
    internal GameInputLabel BButtonLabel;
    internal GameInputLabel CButtonLabel;
    internal GameInputLabel XButtonLabel;
    internal GameInputLabel YButtonLabel;
    internal GameInputLabel ZButtonLabel;
    internal GameInputLabel DPadUpLabel;
    internal GameInputLabel DPadDownLabel;
    internal GameInputLabel DPadLeftLabel;
    internal GameInputLabel DPadRightLabel;
    internal GameInputLabel LeftShoulderButtonLabel;
    internal GameInputLabel RightShoulderButtonLabel;
    internal GameInputLabel LeftThumbstickButtonLabel;
    internal GameInputLabel RightThumbstickButtonLabel;
    internal uint ExtraButtonCount;
    internal uint ExtraAxisCount;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputRacingWheelInfo
{
    internal GameInputLabel MenuButtonLabel;
    internal GameInputLabel ViewButtonLabel;
    internal GameInputLabel PreviousGearButtonLabel;
    internal GameInputLabel NextGearButtonLabel;
    internal GameInputLabel DPadUpLabel;
    internal GameInputLabel DPadDownLabel;
    internal GameInputLabel DPadLeftLabel;
    internal GameInputLabel DPadRightLabel;
    internal GameInputLabel AButtonLabel;
    internal GameInputLabel BButtonLabel;
    internal GameInputLabel XButtonLabel;
    internal GameInputLabel YButtonLabel;
    internal GameInputLabel LeftThumbstickButtonLabel;
    internal GameInputLabel RightThumbstickButtonLabel;
    [MarshalAs(UnmanagedType.I1)] internal bool HasClutch;
    [MarshalAs(UnmanagedType.I1)] internal bool HasHandbrake;
    [MarshalAs(UnmanagedType.I1)] internal bool HasPatternShifter;
    internal int MinPatternShifterGear;
    internal int MaxPatternShifterGear;
    internal float MaxWheelAngle;
    internal uint ExtraButtonCount;
    internal uint ExtraAxisCount;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputForceFeedbackMotorInfo
{
    internal GameInputFeedbackAxes SupportedAxes;
    [MarshalAs(UnmanagedType.I1)] internal bool IsConstantEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsRampEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsSineWaveEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsSquareWaveEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsTriangleWaveEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsSawtoothUpWaveEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsSawtoothDownWaveEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsSpringEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsFrictionEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsDamperEffectSupported;
    [MarshalAs(UnmanagedType.I1)] internal bool IsInertiaEffectSupported;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputRawDeviceReportInfo
{
    internal GameInputRawDeviceReportKind Kind;
    internal uint Id;
    internal uint Size;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct GameInputDeviceInfo
{
    internal ushort VendorId;
    internal ushort ProductId;
    internal ushort RevisionNumber;
    internal GameInputUsage Usage;
    internal GameInputVersion HardwareVersion;
    internal GameInputVersion FirmwareVersion;
    internal AppLocalDeviceId DeviceId;
    internal AppLocalDeviceId DeviceRootId;
    internal GameInputDeviceFamily DeviceFamily;
    internal GameInputKind SupportedInput;
    internal GameInputRumbleMotors SupportedRumbleMotors;
    internal GameInputSystemButtons SupportedSystemButtons;
    internal Guid ContainerId;
    internal IntPtr DisplayName;
    internal IntPtr PnpPath;
    internal IntPtr KeyboardInfo;
    internal IntPtr MouseInfo;
    internal IntPtr SensorsInfo;
    internal IntPtr ControllerInfo;
    internal IntPtr ArcadeStickInfo;
    internal IntPtr FlightStickInfo;
    internal IntPtr GamepadInfo;
    internal IntPtr RacingWheelInfo;
    internal uint ForceFeedbackMotorCount;
    internal IntPtr ForceFeedbackMotorInfo;
    internal uint InputReportCount;
    internal IntPtr InputReportInfo;
    internal uint OutputReportCount;
    internal IntPtr OutputReportInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct GameInputHapticInfo
{
    internal fixed char AudioEndpointId[256];
    internal uint LocationCount;
    internal fixed byte Locations[16 * 8];
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputForceFeedbackEnvelope
{
    internal ulong AttackDuration;
    internal ulong SustainDuration;
    internal ulong ReleaseDuration;
    internal float AttackGain;
    internal float SustainGain;
    internal float ReleaseGain;
    internal uint PlayCount;
    internal ulong RepeatDelay;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputForceFeedbackMagnitude
{
    internal float LinearX;
    internal float LinearY;
    internal float LinearZ;
    internal float AngularX;
    internal float AngularY;
    internal float AngularZ;
    internal float Normal;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputForceFeedbackConditionParams
{
    internal GameInputForceFeedbackMagnitude Magnitude;
    internal float PositiveCoefficient;
    internal float NegativeCoefficient;
    internal float MaxPositiveMagnitude;
    internal float MaxNegativeMagnitude;
    internal float DeadZone;
    internal float Bias;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputForceFeedbackConstantParams
{
    internal GameInputForceFeedbackEnvelope Envelope;
    internal GameInputForceFeedbackMagnitude Magnitude;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputForceFeedbackPeriodicParams
{
    internal GameInputForceFeedbackEnvelope Envelope;
    internal GameInputForceFeedbackMagnitude Magnitude;
    internal float Frequency;
    internal float Phase;
    internal float Bias;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputForceFeedbackRampParams
{
    internal GameInputForceFeedbackEnvelope Envelope;
    internal GameInputForceFeedbackMagnitude StartMagnitude;
    internal GameInputForceFeedbackMagnitude EndMagnitude;
}

[StructLayout(LayoutKind.Explicit)]
internal struct GameInputForceFeedbackData
{
    [FieldOffset(0)] internal GameInputForceFeedbackConstantParams Constant;
    [FieldOffset(0)] internal GameInputForceFeedbackRampParams Ramp;
    [FieldOffset(0)] internal GameInputForceFeedbackPeriodicParams Periodic;
    [FieldOffset(0)] internal GameInputForceFeedbackConditionParams Condition;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputForceFeedbackParams
{
    internal GameInputForceFeedbackEffectKind Kind;
    internal GameInputForceFeedbackData Data;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputRumbleParams
{
    internal float LowFrequency;
    internal float HighFrequency;
    internal float LeftTrigger;
    internal float RightTrigger;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputAxisMapping
{
    internal GameInputElementKind ControllerElementKind;
    internal uint ControllerIndex;
    [MarshalAs(UnmanagedType.I1)] internal bool IsInverted;
    [MarshalAs(UnmanagedType.I1)] internal bool FromTwoButtons;
    internal uint ButtonMinIndexValue;
    internal GameInputSwitchPosition ReferenceDirection;
}

[StructLayout(LayoutKind.Sequential)]
internal struct GameInputButtonMapping
{
    internal GameInputElementKind ControllerElementKind;
    internal uint ControllerIndex;
    [MarshalAs(UnmanagedType.I1)] internal bool IsInverted;
    internal GameInputSwitchPosition SwitchPosition;
}
