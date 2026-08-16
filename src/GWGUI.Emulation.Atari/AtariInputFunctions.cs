using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariInputFunctions
{
    internal static EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        return new EmulationInputSnapshot(new HashSet<EmulationKey>(snapshot.Keys), snapshot.Pointer with { },
            snapshot.Controllers.ToArray());
    }

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (port >= snapshot.Controllers.Count) return AtariInputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (device == AtariInputConstants.JoypadDevice)
        {
            if (index != AtariInputConstants.LeftAnalogIndex) return AtariInputConstants.InactiveState;
            if (id == AtariInputConstants.JoypadMaskId)
                return unchecked((short)(controller.Buttons & ushort.MaxValue));
            return id < AtariInputConstants.MaximumJoypadButtonCount &&
                   (controller.Buttons & (1u << checked((int)id))) != AtariConstants.NoInputState
                ? AtariInputConstants.ActiveState
                : AtariInputConstants.InactiveState;
        }
        if (device != AtariInputConstants.AnalogDevice) return AtariInputConstants.InactiveState;
        return (index, id) switch
        {
            (AtariInputConstants.LeftAnalogIndex, AtariInputConstants.AnalogXId) => controller.LeftX,
            (AtariInputConstants.LeftAnalogIndex, AtariInputConstants.AnalogYId) => controller.LeftY,
            (AtariInputConstants.RightAnalogIndex, AtariInputConstants.AnalogXId) => controller.RightX,
            (AtariInputConstants.RightAnalogIndex, AtariInputConstants.AnalogYId) => controller.RightY,
            _ => AtariInputConstants.InactiveState
        };
    }
}
