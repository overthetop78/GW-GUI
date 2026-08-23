namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputDeviceClassifier
{
    private const GameInputKind StandardGamingKinds =
        GameInputKind.Controller |
        GameInputKind.ArcadeStick |
        GameInputKind.FlightStick |
        GameInputKind.Gamepad |
        GameInputKind.RacingWheel;

    internal static bool IsGamingController(GameInputDeviceDescriptor descriptor)
    {
        if ((descriptor.SupportedInput & StandardGamingKinds) != 0) return true;
        if ((descriptor.SupportedInput & GameInputKind.RawDeviceReport) == 0) return false;
        return descriptor.Usage.Page == 0x01 &&
            descriptor.Usage.Id is 0x04 or 0x05 or 0x08;
    }
}
