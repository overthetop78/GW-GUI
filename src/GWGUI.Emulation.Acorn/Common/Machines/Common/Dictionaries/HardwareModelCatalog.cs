namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Dictionaries;

public static class HardwareModelCatalog
{
    private static readonly IReadOnlyList<HardwareModelDefinition> Definitions = BbcMicroModelCatalog.All;
    private static readonly IReadOnlyDictionary<MachineModel, HardwareModelDefinition> ByModel = HardwareModelFunctions.Index(Definitions);

    public static IReadOnlyList<HardwareModelDefinition> All => Definitions;

    public static HardwareModelDefinition Get(MachineModel model) => ByModel.TryGetValue(model, out var definition) ? definition : throw new ArgumentOutOfRangeException(nameof(model), model, null);
}
