using ThreeDOModels = GWGUI.Emulation.Panasonic.Common.Machines.ThreeDO.Dictionaries.HardwareModelCatalog;

namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Dictionaries;

internal static class HardwareModelCatalog
{
    private static readonly IReadOnlyDictionary<MachineModel, HardwareModel> Models = ThreeDOModels.All.ToDictionary(model => model.Model);
    internal static HardwareModel Get(MachineModel model) => Models.TryGetValue(model, out var hardware) ? hardware : throw new ArgumentOutOfRangeException(nameof(model));
}
