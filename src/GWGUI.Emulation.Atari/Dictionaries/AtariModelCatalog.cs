using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Dictionaries;

public static class AtariModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } =
    [
        Model(AtariMachineModel.St, AtariModelCatalogConstants.ResourceAtariModelSt),
        Model(AtariMachineModel.Stf, AtariModelCatalogConstants.ResourceAtariModelStf),
        Model(AtariMachineModel.Stfm, AtariModelCatalogConstants.ResourceAtariModelStfm),
        Model(AtariMachineModel.MegaSt, AtariModelCatalogConstants.ResourceAtariModelMegaSt),
        Model(AtariMachineModel.Ste, AtariModelCatalogConstants.ResourceAtariModelSte),
        Model(AtariMachineModel.MegaSte, AtariModelCatalogConstants.ResourceAtariModelMegaSte),
        Model(AtariMachineModel.Tt, AtariModelCatalogConstants.ResourceAtariModelTt),
        Model(AtariMachineModel.Falcon, AtariModelCatalogConstants.ResourceAtariModelFalcon),
        Model(AtariMachineModel.Atari400, AtariModelCatalogConstants.ResourceAtariModel400),
        Model(AtariMachineModel.Atari800, AtariModelCatalogConstants.ResourceAtariModel800),
        Model(AtariMachineModel.Atari800Xl, AtariModelCatalogConstants.ResourceAtariModel800Xl),
        Model(AtariMachineModel.Atari130Xe, AtariModelCatalogConstants.ResourceAtariModel130Xe),
        Model(AtariMachineModel.XlXe, AtariModelCatalogConstants.ResourceAtariModelXlXe),
        Model(AtariMachineModel.Xegs, AtariModelCatalogConstants.ResourceAtariModelXegs),
        Model(AtariMachineModel.Atari2600, AtariModelCatalogConstants.ResourceAtariModel2600),
        Model(AtariMachineModel.Atari5200, AtariModelCatalogConstants.ResourceAtariModel5200),
        Model(AtariMachineModel.Atari7800, AtariModelCatalogConstants.ResourceAtariModel7800),
        Model(AtariMachineModel.Lynx, AtariModelCatalogConstants.ResourceAtariModelLynx),
        Model(AtariMachineModel.Jaguar, AtariModelCatalogConstants.ResourceAtariModelJaguar),
        Model(AtariMachineModel.JaguarCd, AtariModelCatalogConstants.ResourceAtariModelJaguarCd)
    ];

    public static AtariMachineModel Parse(string id) =>
        Enum.TryParse<AtariMachineModel>(id, out var model) && Enum.IsDefined(model)
            ? model
            : throw new ArgumentOutOfRangeException(nameof(id), id, null);

    private static EmulationMachineDefinition Model(AtariMachineModel model, string resourceKey) =>
        new(model.ToString(), resourceKey);
}
