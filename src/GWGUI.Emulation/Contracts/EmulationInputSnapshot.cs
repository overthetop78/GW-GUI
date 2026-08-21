namespace GWGUI.Emulation;

public sealed record EmulationInputSnapshot(
    IReadOnlySet<EmulationKey> Keys,
    EmulationPointerState Pointer,
    IReadOnlyList<EmulationControllerState> Controllers)
{
    public static EmulationInputSnapshot Empty { get; } = new(
        new HashSet<EmulationKey>(),
        new EmulationPointerState(0, 0, 0, false, false, false, false, false, 0),
        [
            EmulationControllerState.Empty,
            EmulationControllerState.Empty,
            EmulationControllerState.Empty,
            EmulationControllerState.Empty
        ]);
}
