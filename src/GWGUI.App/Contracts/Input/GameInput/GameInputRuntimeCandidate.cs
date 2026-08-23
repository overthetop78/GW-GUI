namespace GWGUI.App.Services.Input.GameInput;

internal sealed record GameInputRuntimeCandidate(
    string Path,
    Version Version,
    GameInputRuntimeSource Source);
