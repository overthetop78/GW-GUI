using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Constants;
namespace GWGUI.Emulation.Mattel.Emulators.Common.Interop.Functions;

internal static class KeyboardFunctions
{
    // Console keypad commands are transported through the associated controller.
    // Publishing host keyboard characters here would bypass those associations.
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap() => new Dictionary<EmulationKey, uint>();
    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => KeyboardConstants.NoModifiers;
    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys) => KeyboardConstants.NoCharacter;
    internal static bool IsModifier(EmulationKey key) => false;
}
