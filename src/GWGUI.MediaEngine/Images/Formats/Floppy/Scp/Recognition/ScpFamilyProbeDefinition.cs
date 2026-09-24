namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Recognition;

/// <summary>Associe une famille SCP aux décodeurs capables d'en reconnaître les marqueurs physiques.</summary>
internal sealed record ScpFamilyProbeDefinition(
    ScpFormatFamily Family,
    string DisplayName,
    IReadOnlyList<string> DecoderIds);
