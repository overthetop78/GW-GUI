namespace GWGUI.Emulation.Fairchild.Emulators.Common.Functions;

internal static class InputFunctions
{
    internal static EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        return new(new HashSet<EmulationKey>(snapshot.Keys), snapshot.Pointer with { }, snapshot.Controllers.ToArray());
    }

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device != InputConstants.JoypadDevice || index != InputConstants.PrimaryIndex || port >= snapshot.Controllers.Count)
            return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (id == InputConstants.JoypadMaskId) return unchecked((short)(controller.Buttons & ushort.MaxValue));
        return id < InputConstants.MaximumJoypadButtonCount && (controller.Buttons & (1u << checked((int)id))) != 0 ? InputConstants.ActiveState : InputConstants.InactiveState;
    }
    internal static EmulationInputSnapshot Accumulate(EmulationInputSnapshot current,
        EmulationInputSnapshot update) => new(new HashSet<EmulationKey>(update.Keys), update.Pointer with
    {
        DeltaX = SaturatingAdd(current.Pointer.DeltaX, update.Pointer.DeltaX),
        DeltaY = SaturatingAdd(current.Pointer.DeltaY, update.Pointer.DeltaY),
        Wheel = SaturatingAdd(current.Pointer.Wheel, update.Pointer.Wheel),
        HorizontalWheel = SaturatingAdd(current.Pointer.HorizontalWheel, update.Pointer.HorizontalWheel)
    }, update.Controllers.ToArray());

    internal static EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) =>
        snapshot with
        {
            Pointer = snapshot.Pointer with
            {
                DeltaX = InputConstants.ConsumedRelativeValue,
                DeltaY = InputConstants.ConsumedRelativeValue,
                Wheel = InputConstants.ConsumedRelativeValue,
                HorizontalWheel = InputConstants.ConsumedRelativeValue
            }
        };


    private static int SaturatingAdd(int left, int right) => (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);
}
