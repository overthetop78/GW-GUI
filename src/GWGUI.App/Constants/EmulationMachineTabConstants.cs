using GWGUI.App.Contracts;
using GWGUI.Emulation;

namespace GWGUI.App.Constants;

internal static class EmulationMachineTabConstants
{
    internal const double HorizontalPadding = 14;
    internal const double VerticalPadding = 9;
    internal const double OuterMargin = 8;

    internal static readonly IReadOnlyList<EmulationMachineTabDefinition> Definitions =
    [
        new(EmulationMachineTab.General, "\uE713", "Emulation.Tab.General"),
        new(EmulationMachineTab.Cpu, "\uE950", "Emulation.Tab.Cpu"),
        new(EmulationMachineTab.Ram, "\uE964", "Emulation.Tab.Ram"),
        new(EmulationMachineTab.Rom, "\uE8B7", "Emulation.Tab.Rom"),
        new(EmulationMachineTab.Video, "\uE7F4", "Emulation.Tab.Video"),
        new(EmulationMachineTab.Audio, "\uE767", "Emulation.Audio"),
        new(EmulationMachineTab.Storage, "\uEDA2", "Emulation.Tab.Storage"),
        new(EmulationMachineTab.Keyboard, "\uE765", "Emulation.Tab.Keyboard"),
        new(EmulationMachineTab.Mouse, "\uE962", "Emulation.Tab.Mouse"),
        new(EmulationMachineTab.Controllers, "\uE7FC", "Emulation.Controller.Tab")
    ];
}
