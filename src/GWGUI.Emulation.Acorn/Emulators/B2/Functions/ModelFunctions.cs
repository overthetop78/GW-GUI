using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Emulators.B2.Contracts;
using GWGUI.Emulation.Acorn.Emulators.B2.Dictionaries;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Functions;

internal static class B2ModelFunctions
{
    internal static IReadOnlyList<B2ModelDefinition> GetModels(MachineModel model) =>
        B2ModelCatalog.All.Where(definition => definition.MachineModel == model).ToArray();

    internal static string DefaultValue(MachineModel model) => model switch
    {
        MachineModel.BbcMicroModelB => OptionConstants.ModelBAcorn1770,
        MachineModel.BbcMaster128 => OptionConstants.Master128,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    internal static bool IsCompatible(MachineModel model, string nativeValue) =>
        B2ModelCatalog.All.Any(definition => definition.MachineModel == model
            && string.Equals(definition.NativeValue, nativeValue, StringComparison.Ordinal));
}
