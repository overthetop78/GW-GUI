namespace GWGUI.Emulation.Coleco.Emulators.Common.Functions;

internal static class ControllerFunctions
{
    internal static EmulationInputSnapshot ApplyDeadZones(EmulationInputSnapshot input, IReadOnlyList<ControllerBinding>? bindings)
    {
        var snapshot = InputFunctions.Freeze(input);
        if (bindings is null) return snapshot;
        var controllers = snapshot.Controllers.ToArray();
        foreach (var binding in bindings)
        {
            if (binding.Port < ControllerConstants.FirstPort || binding.Port >= controllers.Length) continue;
            if (binding.Peripheral == PeripheralCategory.None)
                controllers[binding.Port] = EmulationControllerState.Empty;
        }
        // Native analogue values encode Coleco keypad/buttons; preserve them as produced by the mappings.
        return snapshot with { Controllers = controllers };
    }
}
