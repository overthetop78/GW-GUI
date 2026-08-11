namespace GWGUI.MediaEngine.Decoding;

internal enum FluxIntegrityDescriptionState
{
    Valid,
    Invalid,
    Unavailable
}

internal static class FluxStructureDescriptions
{
    public static FluxIntegrityDescriptionState IntegrityState(bool? valid) => valid is null ? FluxIntegrityDescriptionState.Unavailable : valid.Value ? FluxIntegrityDescriptionState.Valid : FluxIntegrityDescriptionState.Invalid;

    public static string Identity(string codec, FluxStructureKind kind, int cylinder, int head, int sector, int size, byte? mark, string? variant)
    {
        var markText = mark is null ? string.Empty : $", mark {mark.Value:X2}";
        var variantText = string.IsNullOrWhiteSpace(variant) ? string.Empty : $", {variant}";
        return $"{codec} {kind}, C{cylinder} H{head} R{sector}, {size} bytes{markText}{variantText}";
    }

    public static string Integrity(string label, bool? valid) => $"{label} {IntegrityState(valid).ToString().ToLowerInvariant()}";

    public static string Integrity(bool? headerValid, bool? dataValid) => $"{Integrity("header CRC", headerValid)}, {Integrity("data CRC", dataValid)}";

    public static string Complete(string codec, FluxStructureKind kind, int cylinder, int head, int sector, int size, byte? mark, string? variant, bool? headerValid, bool? dataValid) => $"{Identity(codec, kind, cylinder, head, sector, size, mark, variant)}, {Integrity(headerValid, dataValid)}";

    public static string Truncated(string codec, FluxStructureKind kind, byte? mark, string? variant) => $"{codec} {kind}{(mark is null ? string.Empty : $", mark {mark.Value:X2}")}{(string.IsNullOrWhiteSpace(variant) ? string.Empty : $", {variant}")}, truncated";

    public static string UnpairedData(string codec, byte? mark, string? variant) => $"Unpaired {codec} data{(mark is null ? string.Empty : $", mark {mark.Value:X2}")}{(string.IsNullOrWhiteSpace(variant) ? string.Empty : $", {variant}")}";

    public static string UnclassifiedMark(string codec, FluxStructureKind kind, byte? mark, string? variant) => $"Unclassified {codec} {kind}{(mark is null ? string.Empty : $", mark {mark.Value:X2}")}{(string.IsNullOrWhiteSpace(variant) ? string.Empty : $", {variant}")}";
}
