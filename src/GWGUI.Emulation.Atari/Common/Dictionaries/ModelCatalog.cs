using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } =
    [
        Model(MachineModel.St, ModelCatalogConstants.ResourceAtariModelSt),
        Model(MachineModel.Stf, ModelCatalogConstants.ResourceAtariModelStf),
        Model(MachineModel.Stfm, ModelCatalogConstants.ResourceAtariModelStfm),
        Model(MachineModel.MegaSt, ModelCatalogConstants.ResourceAtariModelMegaSt),
        Model(MachineModel.Ste, ModelCatalogConstants.ResourceAtariModelSte),
        Model(MachineModel.MegaSte, ModelCatalogConstants.ResourceAtariModelMegaSte),
        Model(MachineModel.Tt, ModelCatalogConstants.ResourceAtariModelTt),
        Model(MachineModel.Falcon, ModelCatalogConstants.ResourceAtariModelFalcon),
        Model(MachineModel.Atari400, ModelCatalogConstants.ResourceAtariModel400),
        Model(MachineModel.Atari800, ModelCatalogConstants.ResourceAtariModel800),
        Model(MachineModel.Atari800Xl, ModelCatalogConstants.ResourceAtariModel800Xl),
        Model(MachineModel.Atari130Xe, ModelCatalogConstants.ResourceAtariModel130Xe),
        Model(MachineModel.XlXe, ModelCatalogConstants.ResourceAtariModelXlXe),
        Model(MachineModel.Xegs, ModelCatalogConstants.ResourceAtariModelXegs),
        Model(MachineModel.Atari2600, ModelCatalogConstants.ResourceAtariModel2600),
        Model(MachineModel.Atari5200, ModelCatalogConstants.ResourceAtariModel5200),
        Model(MachineModel.Atari7800, ModelCatalogConstants.ResourceAtariModel7800),
        Model(MachineModel.Lynx, ModelCatalogConstants.ResourceAtariModelLynx),
        Model(MachineModel.Jaguar, ModelCatalogConstants.ResourceAtariModelJaguar),
        Model(MachineModel.JaguarCd, ModelCatalogConstants.ResourceAtariModelJaguarCd)
    ];

    public static MachineModel Parse(string id) =>
        Enum.TryParse<MachineModel>(id, out var model) && Enum.IsDefined(model)
            ? model
            : throw new ArgumentOutOfRangeException(nameof(id), id, null);

    private static EmulationMachineDefinition Model(MachineModel model, string resourceKey) =>
        new(model.ToString(), resourceKey);
}
