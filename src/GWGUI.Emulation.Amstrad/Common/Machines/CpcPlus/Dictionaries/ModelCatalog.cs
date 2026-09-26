using ModelConstants = GWGUI.Emulation.Amstrad.Common.Machines.CpcPlus.Constants.ModelConstants;
using CommonModelConstants = GWGUI.Emulation.Amstrad.Common.Machines.Common.Constants.ModelConstants;

namespace GWGUI.Emulation.Amstrad.Common.Machines.CpcPlus.Dictionaries;

internal static class ModelCatalog
{
    internal static IReadOnlyList<Model> All { get; } =
    [
        new(ModelConstants.Cpc464Plus, ModelConstants.DisplayCpc464Plus,
            CommonModelConstants.BackendCpcPlus, CommonModelConstants.Ram64Kib,
            true, 0, 2, true, true, true, true),
        new(ModelConstants.Cpc6128Plus, ModelConstants.DisplayCpc6128Plus,
            CommonModelConstants.BackendCpcPlus, CommonModelConstants.Ram128Kib,
            true, 1, 2, false, true, true, true)
    ];
}
