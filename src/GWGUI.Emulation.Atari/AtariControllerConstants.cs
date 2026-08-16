namespace GWGUI.Emulation.Atari;

public static class AtariControllerConstants
{
    public const int MinimumDeadZonePercent = 0;
    public const int MaximumDeadZonePercent = 100;
    public const int DefaultDeadZonePercent = 15;
    internal const int MaximumAxisMagnitude = short.MaxValue;
    internal const int PercentageDivisor = 100;
    internal const short NeutralAxis = 0;
    internal const uint TriggerAnalogIndex = 2;
    internal const uint LeftTriggerId = 12;
    internal const uint RightTriggerId = 13;
}
