namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class InputFunctions
{
    internal static EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        return new EmulationInputSnapshot(new HashSet<EmulationKey>(snapshot.Keys), snapshot.Pointer with { }, snapshot.Controllers.ToArray());
    }

    internal static EmulationInputSnapshot Accumulate(EmulationInputSnapshot current, EmulationInputSnapshot update) => new(new HashSet<EmulationKey>(update.Keys), update.Pointer with
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
                DeltaX = RuntimeConstants.ConsumedRelativeValue,
                DeltaY = RuntimeConstants.ConsumedRelativeValue,
                Wheel = RuntimeConstants.ConsumedRelativeValue,
                HorizontalWheel = RuntimeConstants.ConsumedRelativeValue
            }
        };

    private static int SaturatingAdd(int left, int right) => (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);
}
