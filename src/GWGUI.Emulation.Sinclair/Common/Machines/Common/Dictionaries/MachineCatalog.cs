namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(ModelCatalog.All.Select(model => new EmulationMachineDefinition(model.Id, model.DisplayResourceKey, $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{ImageName(ModelCatalog.Parse(model.Id))}.png")).ToArray());

    private static string ImageName(MachineModel model) => model switch { MachineModel.Spectrum16K or MachineModel.Spectrum48KNtsc => nameof(MachineModel.Spectrum48K), MachineModel.SpectrumPlus3E => nameof(MachineModel.SpectrumPlus3), MachineModel.Pentagon512K => nameof(MachineModel.Pentagon128K), _ => model.ToString() };
}
