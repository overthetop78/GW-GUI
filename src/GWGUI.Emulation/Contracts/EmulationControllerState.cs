namespace GWGUI.Emulation.Contracts;

public sealed record EmulationControllerState(
    uint Buttons,
    short LeftX,
    short LeftY,
    short RightX,
    short RightY,
    short LeftTrigger,
    short RightTrigger)
{
    public string DeviceId { get; init; } = string.Empty;
    public EmulationControllerControls Controls { get; init; } = EmulationControllerControls.Empty;

    public static EmulationControllerState Empty { get; } = new(0, 0, 0, 0, 0, 0, 0);
}
