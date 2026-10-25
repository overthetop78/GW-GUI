namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Functions;

internal static class MediaRuntimeFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices { get; } = [new(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc, [".iso", ".bin", ".chd", ".cue"], IsPermanent: true, DisplayLabel: "CD-ROM")];
    internal static IReadOnlyList<EmulationMedia> Resolve(MachineConfiguration configuration) => configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray();
}
