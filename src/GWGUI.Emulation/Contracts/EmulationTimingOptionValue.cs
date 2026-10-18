using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationTimingOptionValue(string Value, string Label, string? ResourceKey = null, EmulationVideoStandard? VideoStandard = null, double? FrequencyHz = null, string? InvariantDisplayValue = null);
