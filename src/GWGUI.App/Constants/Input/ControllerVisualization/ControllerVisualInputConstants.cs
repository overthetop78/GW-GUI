namespace GWGUI.App.Constants.Input.ControllerVisualization;

internal static class ControllerVisualInputConstants
{
    internal const float TriggerPressedThreshold = 0.04f;
    internal const float DirectionPressedThreshold = 0.5f;
    internal const float AnalogInputThreshold = 0.0001f;
    internal const float MinimumSignedAxisValue = -1f;
    internal const float NeutralAxisValue = 0f;
    internal const float MaximumAxisValue = 1f;
    internal const float NormalizedAxisScale = 2f;

    internal const int GamepadLeftHorizontalAxisIndex = 0;
    internal const int GamepadLeftVerticalAxisIndex = 1;
    internal const int GamepadRightHorizontalAxisIndex = 2;
    internal const int GamepadRightVerticalAxisIndex = 3;
    internal const int RacingWheelAxisIndex = 0;
    internal const int RacingThrottleAxisIndex = 1;
    internal const int RacingBrakeAxisIndex = 2;
    internal const int RacingClutchAxisIndex = 3;
    internal const int RacingHandbrakeAxisIndex = 4;
    internal const int FlightRollAxisIndex = 0;
    internal const int FlightPitchAxisIndex = 1;
    internal const int FlightYawAxisIndex = 2;
    internal const int FlightThrottleAxisIndex = 3;
    internal const int FlightHatHorizontalAxisIndex = 4;
    internal const int FlightHatVerticalAxisIndex = 5;
    internal const int LeftTriggerButtonIndex = 12;
    internal const int RightTriggerButtonIndex = 13;
}
