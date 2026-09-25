namespace GWGUI.Emulation.Atari.Common.Dictionaries;

public static class CompatibilityCatalog
{
    private static readonly IReadOnlyList<CompatibilityDefinition> Definitions =
        CompatibilityFunctions.Values(
            CompatibilityFunctions.Create(MachineModel.St),
            CompatibilityFunctions.Create(MachineModel.Stf),
            CompatibilityFunctions.Create(MachineModel.Stfm),
            CompatibilityFunctions.Create(MachineModel.MegaSt),
            CompatibilityFunctions.Create(MachineModel.Ste),
            CompatibilityFunctions.Create(MachineModel.MegaSte),
            CompatibilityFunctions.Create(MachineModel.Tt),
            CompatibilityFunctions.Create(MachineModel.Falcon),
            CompatibilityFunctions.Create(MachineModel.Atari400),
            CompatibilityFunctions.Create(MachineModel.Atari800),
            CompatibilityFunctions.Create(MachineModel.Atari800Xl),
            CompatibilityFunctions.Create(MachineModel.Atari130Xe),
            CompatibilityFunctions.Create(MachineModel.XlXe),
            CompatibilityFunctions.Create(MachineModel.Xegs),
            CompatibilityFunctions.Create(MachineModel.Atari5200),
            CompatibilityFunctions.Create(MachineModel.Atari2600),
            CompatibilityFunctions.Create(MachineModel.Atari7800),
            CompatibilityFunctions.Create(MachineModel.Lynx),
            CompatibilityFunctions.Create(MachineModel.Jaguar),
            CompatibilityFunctions.Create(MachineModel.JaguarCd));

    private static readonly IReadOnlyDictionary<MachineModel, CompatibilityDefinition> ByModel =
        CompatibilityFunctions.Index(Definitions);

    public static IReadOnlyList<CompatibilityDefinition> All => Definitions;

    public static CompatibilityDefinition Get(MachineModel model) =>
        ByModel.TryGetValue(model, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(model), model,
                ErrorMessages.UnknownCompatibilityModel);
}
