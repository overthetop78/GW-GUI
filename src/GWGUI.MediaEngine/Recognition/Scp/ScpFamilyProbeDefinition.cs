namespace GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Associe une famille SCP aux décodeurs capables d'en reconnaître les marqueurs physiques.</summary>
internal sealed record ScpFamilyProbeDefinition(
    ScpFormatFamily Family,
    string DisplayName,
    IReadOnlyList<string> DecoderIds);
