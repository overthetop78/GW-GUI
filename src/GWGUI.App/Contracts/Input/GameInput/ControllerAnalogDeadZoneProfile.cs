namespace GWGUI.App.Services.Input.GameInput;

internal sealed record ControllerAnalogDeadZoneProfile(
    int StickPercent = 0,
    int TriggerPercent = 0,
    int OuterPercent = 0)
{
    internal static ControllerAnalogDeadZoneProfile Default { get; } = new();

    internal ControllerAnalogDeadZoneProfile Normalize() => this with
    {
        StickPercent = Math.Clamp(StickPercent, 0, 50),
        TriggerPercent = Math.Clamp(TriggerPercent, 0, 50),
        OuterPercent = Math.Clamp(OuterPercent, 0, 30)
    };
}
