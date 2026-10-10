using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Machines.AppleII.Dictionaries;
using GWGUI.Emulation.Apple.Common.Machines.Macintosh.Dictionaries;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Dictionaries;

public static class HardwareModelCatalog
{
    public static IReadOnlyList<HardwareModelDefinition> All { get; } = Array.AsReadOnly(
        AppleIIModelCatalog.All.Concat(MacintoshModelCatalog.All).ToArray());
    private static readonly IReadOnlyDictionary<MachineModel, HardwareModelDefinition> ByModel =
        All.ToDictionary(definition => definition.Model);

    public static HardwareModelDefinition Get(MachineModel model) => ByModel.TryGetValue(model, out var definition)
        ? definition : throw new ArgumentOutOfRangeException(nameof(model), model, null);
}
