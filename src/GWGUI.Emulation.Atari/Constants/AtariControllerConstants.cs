using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Atari.Constants;

public static class AtariControllerConstants
{
    internal static readonly IReadOnlyList<string> DirectionActions = [EmulationControllerCommandIds.Up, EmulationControllerCommandIds.Down, EmulationControllerCommandIds.Left, EmulationControllerCommandIds.Right];
    internal static readonly IReadOnlyList<string> SingleFireActions = [EmulationControllerCommandIds.Fire1];
    internal static readonly IReadOnlyList<string> DualFireActions = [EmulationControllerCommandIds.Fire1, EmulationControllerCommandIds.Fire2];
    internal static readonly IReadOnlyList<string> HatariFireActions = [EmulationControllerCommandIds.Fire1, EmulationControllerCommandIds.Turbo];
    internal static readonly IReadOnlyList<string> LynxActions =
        [EmulationControllerCommandIds.Fire1, EmulationControllerCommandIds.Fire2, EmulationControllerCommandIds.Option1, EmulationControllerCommandIds.Option2, EmulationControllerCommandIds.Pause];
    internal static readonly IReadOnlyList<string> KeypadActions =
        [EmulationControllerCommandIds.Start, EmulationControllerCommandIds.Pause, EmulationControllerCommandIds.Reset, EmulationControllerCommandIds.Key0, EmulationControllerCommandIds.Key1, EmulationControllerCommandIds.Key2, EmulationControllerCommandIds.Key3, EmulationControllerCommandIds.Key4, EmulationControllerCommandIds.Key5, EmulationControllerCommandIds.Key6, EmulationControllerCommandIds.Key7, EmulationControllerCommandIds.Key8, EmulationControllerCommandIds.Key9, EmulationControllerCommandIds.Star, EmulationControllerCommandIds.Hash];
    internal static readonly IReadOnlyList<string> JaguarActions =
        [EmulationControllerCommandIds.A, EmulationControllerCommandIds.B, EmulationControllerCommandIds.C, EmulationControllerCommandIds.Option, EmulationControllerCommandIds.Pause, EmulationControllerCommandIds.Key0, EmulationControllerCommandIds.Key1, EmulationControllerCommandIds.Key2, EmulationControllerCommandIds.Key3, EmulationControllerCommandIds.Key4, EmulationControllerCommandIds.Key5, EmulationControllerCommandIds.Key6, EmulationControllerCommandIds.Key7, EmulationControllerCommandIds.Key8, EmulationControllerCommandIds.Key9, EmulationControllerCommandIds.Star, EmulationControllerCommandIds.Hash];
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
