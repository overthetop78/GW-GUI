using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nec.Emulators.Quasi88.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOptionDefinition> All { get; } =
    [
        new("q88_basic_mode", "Emulation.Nec.CoreOptions.q88_basic_mode", "N88 V2", EmulationMachineTab.Cpu,
            [new("N88 V2", "N88-BASIC V2", IsInvariant: true), new("N88 V1H", "N88-BASIC V1H", IsInvariant: true), new("N88 V1S", "N88-BASIC V1S", IsInvariant: true), new("N", "N-BASIC", IsInvariant: true)]),
        new("q88_sub_cpu_mode", "Emulation.Nec.CoreOptions.q88_sub_cpu_mode", "0", EmulationMachineTab.Cpu,
            [new("0", "Emulation.Nec.CoreOptions.q88_sub_cpu_mode.Choice0"), new("1", "Emulation.Nec.CoreOptions.q88_sub_cpu_mode.Choice1"), new("2", "Emulation.Nec.CoreOptions.q88_sub_cpu_mode.Choice2")]),
        new("q88_cpu_clock", "Emulation.Nec.CoreOptions.q88_cpu_clock", "4", EmulationMachineTab.Cpu,
            [new("4", "Emulation.Nec.CoreOptions.q88_cpu_clock.Choice0"), new("2.53", "Emulation.Nec.CoreOptions.q88_cpu_clock.Choice1"), new("8", "Emulation.Nec.CoreOptions.q88_cpu_clock.Choice2"), new("16", "Emulation.Nec.CoreOptions.q88_cpu_clock.Choice3"), new("32", "Emulation.Nec.CoreOptions.q88_cpu_clock.Choice4"), new("64", "Emulation.Nec.CoreOptions.q88_cpu_clock.Choice5"), new("1", "Emulation.Nec.CoreOptions.q88_cpu_clock.Choice6"), new("2", "Emulation.Nec.CoreOptions.q88_cpu_clock.Choice7")]),
        new("q88_sound_board", "Emulation.Nec.CoreOptions.q88_sound_board", "OPNA", EmulationMachineTab.Audio,
            [new("OPN", "OPN (Yamaha YM2203)", IsInvariant: true), new("OPNA", "OPNA (Yamaha YM2608)", IsInvariant: true)]),
        new("q88_use_fdc_wait", "Emulation.Nec.CoreOptions.q88_use_fdc_wait", "enabled", EmulationMachineTab.Storage,
            [new("enabled", "Emulation.Nec.CoreOptions.q88_use_fdc_wait.Choice0"), new("disabled", "Emulation.Nec.CoreOptions.q88_use_fdc_wait.Choice1")]),
        new("q88_pcg-8100", "Emulation.Nec.CoreOptions.q88_pcg-8100", "disabled", EmulationMachineTab.General,
            [new("enabled", "Emulation.Nec.CoreOptions.q88_pcg-8100.Choice0"), new("disabled", "Emulation.Nec.CoreOptions.q88_pcg-8100.Choice1")]),
        new("q88_save_to_disk_image", "Emulation.Nec.CoreOptions.q88_save_to_disk_image", "disabled", EmulationMachineTab.Storage,
            [new("enabled", "Emulation.Nec.CoreOptions.q88_save_to_disk_image.Choice0"), new("disabled", "Emulation.Nec.CoreOptions.q88_save_to_disk_image.Choice1")]),
        new("q88_rumble", "Emulation.Nec.CoreOptions.q88_rumble", "enabled", EmulationMachineTab.Storage,
            [new("enabled", "Emulation.Nec.CoreOptions.q88_rumble.Choice0"), new("disabled", "Emulation.Nec.CoreOptions.q88_rumble.Choice1")]),
        new("q88_screen_size", "Emulation.Nec.CoreOptions.q88_screen_size", "full", EmulationMachineTab.Video,
            [new("full", "Emulation.Nec.CoreOptions.q88_screen_size.Choice0"), new("half", "Emulation.Nec.CoreOptions.q88_screen_size.Choice1")]),
    ];
}
