using GWGUI.Emulation;

namespace GWGUI.App.Services.Input.GameInput;

internal sealed record GameInputPhysicalState(IReadOnlySet<EmulationKey> Keys, EmulationPointerState Pointer, IReadOnlyList<EmulationControllerState> Controllers);
