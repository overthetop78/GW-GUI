namespace GWGUI.Emulation.Apple.Common.Machines.Macintosh.Dictionaries;

internal static class MacintoshKeyboardDictionary
{
    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultHostKeys =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.MacintoshCommand] = EmulationKey.LeftAlt,
            [EmulationKey.MacintoshKeypadClear] = EmulationKey.Delete,
            [EmulationKey.MacintoshKeypadEquals] = EmulationKey.Insert
        };
}
