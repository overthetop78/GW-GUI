namespace GWGUI.Emulation.Atari.Dictionaries;

public static class AtariCompatibilityCatalog
{
    private static readonly IReadOnlyList<AtariCompatibilityDefinition> Definitions =
        AtariCompatibilityFunctions.Values(
            AtariCompatibilityFunctions.Create(AtariMachineModel.St),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Stf),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Stfm),
            AtariCompatibilityFunctions.Create(AtariMachineModel.MegaSt),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Ste),
            AtariCompatibilityFunctions.Create(AtariMachineModel.MegaSte),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Tt),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Falcon),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Atari400),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Atari800),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Atari800Xl),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Atari130Xe),
            AtariCompatibilityFunctions.Create(AtariMachineModel.XlXe),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Xegs),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Atari5200),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Atari2600),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Atari7800),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Lynx),
            AtariCompatibilityFunctions.Create(AtariMachineModel.Jaguar),
            AtariCompatibilityFunctions.Create(AtariMachineModel.JaguarCd));

    private static readonly IReadOnlyDictionary<AtariMachineModel, AtariCompatibilityDefinition> ByModel =
        AtariCompatibilityFunctions.Index(Definitions);

    public static IReadOnlyList<AtariCompatibilityDefinition> All => Definitions;

    public static AtariCompatibilityDefinition Get(AtariMachineModel model) =>
        ByModel.TryGetValue(model, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(model), model,
                AtariErrorMessages.UnknownCompatibilityModel);
}
