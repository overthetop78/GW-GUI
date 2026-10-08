using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.ParallelN64.Constants;

internal static class FirmwareConstants
{
    internal static IReadOnlyList<FirmwareSlot> All { get; } =
    [
        new("configuration.firmware.64DD_IPL.bin", "64DD_IPL.bin", "Emulation.Firmware.parallel_n64.Slot0", false, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nintendo64 })
    ];
}
