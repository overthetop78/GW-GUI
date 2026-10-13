using GWGUI.Emulation.Acorn.Emulators.B2.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Functions;

internal static class MediaDeviceFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Describe(MachineConfiguration configuration) =>
        [new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, MediaRuntimeConstants.FloppyExtensions,
            RequiresMachineRecreation: true, DisplayLabel: MediaRuntimeConstants.DriveLabel,
            ImageDirectory: configuration.Folders.Floppies, IsPermanent: true)];
}
