using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var compatibility = CompatibilityCatalog.Get(configuration.Model);
        var blocks = (compatibility.Core == Emulator.Hatari
            ? CreateSt(configuration)
            : CreateHardware(configuration)).ToList();
        AddGeneralFolders(configuration, compatibility, blocks);
        AddMouseSettings(configuration, compatibility, blocks);
        return blocks;
    }
}
