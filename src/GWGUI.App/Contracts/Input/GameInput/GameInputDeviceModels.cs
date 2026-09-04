namespace GWGUI.App.Services.Input.GameInput;

internal enum ControllerVisualModel
{
    GenericGamepad,
    XboxSeries,
    XboxOne,
    Xbox360,
    Xbox360White,
    XboxRematchCore,
    PlayStation4,
    PlayStation5,
    MasterSystem,
    NintendoEntertainmentSystem,
    Nintendo64,
    SuperNintendo,
    MegaDrive3,
    MegaDrive6,
    PlayStation1,
    PlayStation2,
    Saturn,
    Dreamcast,
    RacingWheel,
    FlightStick,
    ArcadeStick
}

internal enum GameInputControlType { Axis, Button, Switch, RawByte }

internal sealed record GameInputControlDescriptor(
    GameInputControlType Type,
    int Index,
    GameInputLabel Label,
    GameInputSwitchKind SwitchKind = GameInputSwitchKind.Unknown,
    IReadOnlyList<GameInputLabel>? SwitchLabels = null);

internal sealed record GameInputRawReportDescriptor(
    GameInputRawDeviceReportKind Kind,
    uint Id,
    uint Size);

internal sealed record GameInputForceFeedbackMotorDescriptor(
    int Index,
    GameInputFeedbackAxes SupportedAxes,
    IReadOnlyList<GameInputForceFeedbackEffectKind> SupportedEffects,
    bool PoweredOn);

internal sealed record GameInputStandardCapabilities(
    GameInputGamepadButtons GamepadLayout,
    uint GamepadExtraButtonCount,
    uint GamepadExtraAxisCount,
    bool HasArcadeStick,
    bool HasFlightStick,
    bool HasGamepad,
    bool HasRacingWheel,
    bool RacingWheelHasClutch,
    bool RacingWheelHasHandbrake,
    bool RacingWheelHasPatternShifter,
    float RacingWheelMaxAngle,
    IReadOnlyDictionary<GameInputKind, IReadOnlyList<byte>> ExtraAxisIndexes,
    IReadOnlyDictionary<GameInputKind, IReadOnlyList<byte>> ExtraButtonIndexes);

internal sealed record GameInputDeviceDescriptor(
    string Id,
    string ProductName,
    string GameInputDisplayName,
    string PnpPath,
    ushort VendorId,
    ushort ProductId,
    ushort RevisionNumber,
    GameInputVersion HardwareVersion,
    GameInputVersion FirmwareVersion,
    string DeviceRootId,
    Guid ContainerId,
    GameInputDeviceFamily Family,
    GameInputUsage Usage,
    GameInputKind SupportedInput,
    GameInputRumbleMotors RumbleMotors,
    GameInputSystemButtons SystemButtons,
    string Manufacturer,
    IReadOnlyList<string> WindowsIdentityChain,
    IReadOnlyList<GameInputControlDescriptor> Controls,
    GameInputStandardCapabilities StandardCapabilities,
    IReadOnlyList<GameInputForceFeedbackMotorDescriptor> ForceFeedbackMotors,
    IReadOnlyList<GameInputRawReportDescriptor> InputReports,
    IReadOnlyList<GameInputRawReportDescriptor> OutputReports,
    bool HasHaptics,
    string HapticAudioEndpointId,
    IReadOnlyList<Guid> HapticLocations,
    ControllerVisualModel SuggestedVisualModel,
    bool IsExactVisualModelMatch)
{
    internal string VidPid => $"{VendorId:X4}:{ProductId:X4}";
    internal GameInputDeviceStatus Status { get; init; }
}

internal sealed record GameInputControlValue(
    GameInputControlType Type,
    int Index,
    GameInputLabel Label,
    float Value,
    GameInputSwitchPosition SwitchPosition = GameInputSwitchPosition.Center)
{
    internal bool IsPressed => Type == GameInputControlType.Button && Value >= .5f;
}

internal sealed record GameInputLiveState(
    string DeviceId,
    ulong Timestamp,
    GameInputKind InputKind,
    IReadOnlyList<GameInputControlValue> Controls,
    IReadOnlyList<byte> RawReport,
    GameInputSystemButtons SystemButtons,
    GameInputArcadeStickState? ArcadeStick,
    GameInputFlightStickState? FlightStick,
    GameInputGamepadState? Gamepad,
    GameInputRacingWheelState? RacingWheel,
    bool ControlsUseNormalizedAxes = false)
{
    internal static GameInputLiveState Empty(string deviceId) =>
        new(deviceId, 0, GameInputKind.Unknown, [], [], GameInputSystemButtons.None, null, null, null, null);
}
