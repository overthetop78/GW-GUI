using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;
using VideoConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.VideoConstants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries;

/// <summary>Catalogue des implémentations matérielles disponibles dans l'émulateur multimodèle AcornEmu.</summary>
/// <remarks>Le Model A est la première implémentation. Les futurs modèles auront leur propre entrée et assemblage.
/// La fabrique et le cœur consultent ce catalogue : ils ne transforment pas automatiquement un autre modèle en Model A.</remarks>
internal static class ModelCatalog
{
    internal static IReadOnlyList<AcornEmuModelDefinition> All { get; } =
    [
        new(MachineModel.BbcMicroModelA, (double)BbcMicroModelConstants.CpuFrequencyHz / BbcMicroModelAConstants.CyclesPerFrame,
            VideoConstants.Width, VideoConstants.Height, 4, EmulationPixelFormat.Xrgb8888, 4f / 3, EmulationVideoStandard.Pal,
            CreateModelA)
    ];

    private static ModelAMachine CreateModelA(MachineConfiguration configuration)
    {
        var network = NetworkFunctions.Read(configuration);
        var chips = SidewaysExpansionFunctions.ChipBanks(configuration);
        var basic = chips.TryGetValue(SidewaysExpansionFunctions.NativeBank(configuration, Machines.BbcMicro.Common.Constants.DiskConstants.BasicBank), out var programmedBasic) ? programmedBasic : FirmwareFunctions.Read(configuration, EmulatorConstants.BasicFirmware);
        var mos = FirmwareFunctions.Read(configuration, EmulatorConstants.SystemFirmware);
        MemoryExtensionFunctions.ValidateMos(configuration, mos);
        return new ModelAMachine(mos, basic, BbcMicroMemoryFunctions.ModelABytes(configuration), FirmwareFunctions.Read(configuration, EmulatorConstants.CharacterFirmware), StorageFunctions.HasInterface(configuration), FirmwareFunctions.OptionRoms(configuration, chips), StorageFunctions.UsesWd1770(configuration), StorageFunctions.HasWinchester(configuration), network.Enabled, network.Station, network.CyclesPerByte, MemoryExtensionFunctions.Read(configuration), SidewaysExpansionFunctions.Read(configuration))
        {
            Video = { Monochrome = configuration.Options.GetValueOrDefault(EmulatorConstants.VideoOutput, EmulatorConstants.UhfOutput) == EmulatorConstants.MonochromeOutput }
        };
    }

    internal static AcornEmuModelDefinition Get(MachineModel model) => All.SingleOrDefault(item => item.Model == model) ?? throw new NotSupportedException(model.ToString());
}
