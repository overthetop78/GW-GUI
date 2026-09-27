using ModelConstants = GWGUI.Emulation.Amstrad.Common.Machines.Gx4000.Constants.ModelConstants;
using CommonModelConstants = GWGUI.Emulation.Amstrad.Common.Machines.Common.Constants.ModelConstants;

namespace GWGUI.Emulation.Amstrad.Common.Machines.Gx4000.Dictionaries;

internal static class ModelCatalog
{
    internal static IReadOnlyList<Model> All { get; } =
    [
        new(ModelConstants.Gx4000, ModelConstants.DisplayGx4000,
            CommonModelConstants.BackendCpcPlus, CommonModelConstants.Ram64Kib,
            false, 0, 0, false, false, true, true, MouseButtonCount: 0)
    ];
}
