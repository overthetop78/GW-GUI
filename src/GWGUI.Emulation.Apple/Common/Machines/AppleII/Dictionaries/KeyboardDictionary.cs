namespace GWGUI.Emulation.Apple.Common.Machines.AppleII.Dictionaries;

internal static class AppleIIKeyboardDictionary
{
    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultHostKeys =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.AppleOpenApple] = EmulationKey.LeftAlt,
            [EmulationKey.AppleSolidApple] = EmulationKey.RightAlt
        };
}
