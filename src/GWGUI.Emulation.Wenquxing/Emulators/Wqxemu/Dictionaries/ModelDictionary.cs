namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Dictionaries;

internal static class ModelDictionary
{
    internal static IReadOnlyDictionary<MachineModel, string> NativeValues { get; } = new Dictionary<MachineModel, string> { [MachineModel.NC1020] = "nc1020", [MachineModel.PC1000] = "pc1000", [MachineModel.CC800] = "cc800", [MachineModel.NC2000] = "nc2000", [MachineModel.NC3000] = "nc3000" };
}