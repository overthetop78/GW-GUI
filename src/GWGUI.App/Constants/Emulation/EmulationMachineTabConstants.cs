using GWGUI.App.Contracts.Emulation.Machine;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.Emulation;

namespace GWGUI.App.Constants.Emulation;

internal static class EmulationMachineTabConstants
{
    internal const double HorizontalPadding = 14;
    internal const double VerticalPadding = 9;
    internal const double OuterMargin = 8;

    internal static readonly IReadOnlyList<EmulationMachineTabDefinition> Definitions =
    [
        new(EmulationMachineTab.General, IconGlyphs.Settings, "Emulation.Tab.General"),
        new(EmulationMachineTab.Cpu, IconGlyphs.Processor, "Emulation.Tab.Cpu"),
        new(EmulationMachineTab.Ram, IconGlyphs.Memory, "Emulation.Tab.Ram"),
        new(EmulationMachineTab.Rom, IconGlyphs.File, "Emulation.Tab.Rom"),
        new(EmulationMachineTab.Video, IconGlyphs.Display, "Emulation.Tab.Video"),
        new(EmulationMachineTab.Audio, IconGlyphs.Audio, "Emulation.Tab.Audio"),
        new(EmulationMachineTab.Storage, IconGlyphs.HardDisk, "Emulation.Tab.Storage"),
        new(EmulationMachineTab.Keyboard, EmulationInputSettingsConstants.KeyboardIcon, "Emulation.Tab.Keyboard"),
        new(EmulationMachineTab.Mouse, IconGlyphs.Mouse, "Emulation.Tab.Mouse"),
        new(EmulationMachineTab.Controllers, IconGlyphs.Controller, "Emulation.Tab.Controller")
    ];
}
