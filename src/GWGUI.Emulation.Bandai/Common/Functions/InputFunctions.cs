namespace GWGUI.Emulation.Bandai.Common.Functions;

internal static class InputFunctions
{
    internal static EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        return new(new HashSet<EmulationKey>(snapshot.Keys), snapshot.Pointer with { }, snapshot.Controllers.ToArray());
    }

    internal static EmulationInputSnapshot Accumulate(EmulationInputSnapshot current, EmulationInputSnapshot update) => Freeze(update);

    internal static EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) => snapshot;
}
