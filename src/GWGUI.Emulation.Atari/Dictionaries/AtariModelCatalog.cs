using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public static class AtariModelCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } =
    [
        Model(AtariMachineModel.St, "Emulation.Atari.Model.St"),
        Model(AtariMachineModel.Stf, "Emulation.Atari.Model.Stf"),
        Model(AtariMachineModel.Stfm, "Emulation.Atari.Model.Stfm"),
        Model(AtariMachineModel.MegaSt, "Emulation.Atari.Model.MegaSt"),
        Model(AtariMachineModel.Ste, "Emulation.Atari.Model.Ste"),
        Model(AtariMachineModel.MegaSte, "Emulation.Atari.Model.MegaSte"),
        Model(AtariMachineModel.Tt, "Emulation.Atari.Model.Tt"),
        Model(AtariMachineModel.Falcon, "Emulation.Atari.Model.Falcon"),
        Model(AtariMachineModel.Atari400, "Emulation.Atari.Model.400"),
        Model(AtariMachineModel.Atari800, "Emulation.Atari.Model.800"),
        Model(AtariMachineModel.Atari800Xl, "Emulation.Atari.Model.800Xl"),
        Model(AtariMachineModel.Atari130Xe, "Emulation.Atari.Model.130Xe"),
        Model(AtariMachineModel.XlXe, "Emulation.Atari.Model.XlXe"),
        Model(AtariMachineModel.Xegs, "Emulation.Atari.Model.Xegs"),
        Model(AtariMachineModel.Atari2600, "Emulation.Atari.Model.2600"),
        Model(AtariMachineModel.Atari5200, "Emulation.Atari.Model.5200"),
        Model(AtariMachineModel.Atari7800, "Emulation.Atari.Model.7800"),
        Model(AtariMachineModel.Lynx, "Emulation.Atari.Model.Lynx"),
        Model(AtariMachineModel.Jaguar, "Emulation.Atari.Model.Jaguar"),
        Model(AtariMachineModel.JaguarCd, "Emulation.Atari.Model.JaguarCd")
    ];

    public static AtariMachineModel Parse(string id) =>
        Enum.TryParse<AtariMachineModel>(id, out var model) && Enum.IsDefined(model)
            ? model
            : throw new ArgumentOutOfRangeException(nameof(id), id, null);

    private static EmulationMachineDefinition Model(AtariMachineModel model, string resourceKey) =>
        new(model.ToString(), resourceKey);
}
