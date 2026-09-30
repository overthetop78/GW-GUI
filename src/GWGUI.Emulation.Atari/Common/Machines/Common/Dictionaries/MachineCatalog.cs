using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Common.Constants;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Dictionaries;

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id, model.DisplayResourceKey,
            ImageFileName(model.Id) is { } fileName
                ? $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{fileName}"
                : null))
        .ToArray();

    private static string? ImageFileName(string machineId) => machineId switch
    {
        "St" => "ST.png",
        "Stf" => "STf.png",
        "Stfm" => "STfm.png",
        "MegaSt" => "MegaST.png",
        "Ste" => "STe.png",
        "MegaSte" => "MegaSTE.png",
        "Tt" => "TT.png",
        "Falcon" => "Falcon.png",
        "Atari400" => "Atari400.png",
        "Atari800" => "Atari800.png",
        "Atari800Xl" => "Atari800XL.png",
        "Atari130Xe" => "Atari130XE.png",
        "Atari5200" => "Atari5200.png",
        "Atari2600" => "AtariVCS2600.png",
        "Atari7800" => "Atari7800.png",
        "Lynx" => "AtariLynxII.png",
        "Jaguar" => "AtariJaguar.png",
        "JaguarCd" => "AtariJaguarCD.png",
        "XlXe" => "AtariXLXE.png",
        _ => null
    };
}
