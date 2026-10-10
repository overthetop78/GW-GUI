using GWGUI.Emulation;

namespace GWGUI.Emulation.Philips.Emulators.Common.Functions;

internal static class InputFunctions
{
    internal static EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        return new EmulationInputSnapshot(new HashSet<EmulationKey>(snapshot.Keys), snapshot.Pointer with { },
            snapshot.Controllers.ToArray());
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
                DeltaX = InputMappingConstants.ConsumedRelativeValue,
                DeltaY = InputMappingConstants.ConsumedRelativeValue,
                Wheel = InputMappingConstants.ConsumedRelativeValue,
                HorizontalWheel = InputMappingConstants.ConsumedRelativeValue
            }
        };

    private static int SaturatingAdd(int left, int right) =>
        (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);
}
