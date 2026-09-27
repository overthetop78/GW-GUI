using ModelConstants = GWGUI.Emulation.Amstrad.Common.Machines.CpcClassic.Constants.ModelConstants;
using CommonModelConstants = GWGUI.Emulation.Amstrad.Common.Machines.Common.Constants.ModelConstants;

namespace GWGUI.Emulation.Amstrad.Common.Machines.CpcClassic.Dictionaries;

internal static class ModelCatalog
{
    internal static IReadOnlyList<Model> All { get; } =
    [
        new(ModelConstants.Cpc464, ModelConstants.DisplayCpc464, CommonModelConstants.BackendCpc464,
            CommonModelConstants.Ram64Kib, true, 0, 2, true, true, false, false),
        new(ModelConstants.Cpc664, ModelConstants.DisplayCpc664, CommonModelConstants.BackendCpc664,
            CommonModelConstants.Ram64Kib, true, 1, 2, false, true, false, false),
        new(ModelConstants.Cpc6128, ModelConstants.DisplayCpc6128, CommonModelConstants.BackendCpc6128,
            CommonModelConstants.Ram128Kib, true, 1, 2, false, true, false, false)
    ];
}
