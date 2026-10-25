using MoModels = GWGUI.Emulation.Thomson.Common.Machines.Mo.Dictionaries.HardwareModelCatalog;
using ToModels = GWGUI.Emulation.Thomson.Common.Machines.To.Dictionaries.HardwareModelCatalog;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Dictionaries;

internal static class HardwareModelCatalog
{
    private static readonly IReadOnlyDictionary<MachineModel, HardwareModel> Models = MoModels.All.Concat(ToModels.All).ToDictionary(model => model.Model);
    internal static HardwareModel Get(MachineModel model) => Models.TryGetValue(model, out var hardware) ? hardware : throw new ArgumentOutOfRangeException(nameof(model));
}
