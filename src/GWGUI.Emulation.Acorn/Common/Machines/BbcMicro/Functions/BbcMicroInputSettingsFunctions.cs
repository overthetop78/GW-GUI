using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Dictionaries;

namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;

internal static class BbcMicroInputSettingsFunctions
{
    internal static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(MachineConfiguration configuration)
    {
        var keys = configuration.Model == MachineModel.BbcMaster128 ? BbcMicroKeyboardConstants.MasterSpecialKeys : BbcMicroKeyboardConstants.SpecialKeys;
        return keys.Select(key => new InputBindingDefinition(key.ToString(), ResourceKey(key), BbcMicroKeyboardDictionary.DefaultHostKeys.TryGetValue(key, out var physicalKey) ? physicalKey.ToString() : string.Empty)).ToArray();
    }

    private static string ResourceKey(EmulationKey key) => key switch
    {
        EmulationKey.AcornF0 => BbcMicroKeyboardConstants.F0Resource,
        EmulationKey.AcornCopy => BbcMicroKeyboardConstants.CopyResource,
        EmulationKey.AcornShiftLock => BbcMicroKeyboardConstants.ShiftLockResource,
        EmulationKey.AcornKeypadHash => BbcMicroKeyboardConstants.KeypadHashResource,
        EmulationKey.AcornKeypadDelete => BbcMicroKeyboardConstants.KeypadDeleteResource,
        EmulationKey.AcornKeypadComma => BbcMicroKeyboardConstants.KeypadCommaResource,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
    };
}
