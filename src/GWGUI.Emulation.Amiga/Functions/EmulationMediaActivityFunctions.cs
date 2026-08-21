using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal static class EmulationMediaActivityFunctions
{
    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromLedStates(
        IReadOnlyDictionary<int, bool> ledStates)
    {
        var activity = new Dictionary<EmulationMediaSlot, bool>();
        for (var index = 0; index < 4; index++)
            activity[new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, index)] =
                ledStates.GetValueOrDefault(3 + index);

        activity[EmulationMediaSlot.HardDisk0] = ledStates.GetValueOrDefault(7);
        activity[EmulationMediaSlot.Cd0] = ledStates.GetValueOrDefault(8);
        return activity;
    }
}
