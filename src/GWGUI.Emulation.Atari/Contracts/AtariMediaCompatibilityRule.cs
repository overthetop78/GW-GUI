using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public sealed record AtariMediaCompatibilityRule(
    AtariMediaCategory Category,
    IReadOnlyList<EmulationMediaSlot> Slots,
    AtariMediaAvailability Availability = AtariMediaAvailability.Available,
    string? ExplanationResourceKey = null);
