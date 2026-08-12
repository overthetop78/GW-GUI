namespace GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Associe une famille SCP à l'identifiant technique du décodeur qui la sonde.</summary>
internal sealed record ScpFamilyProbeDefinition(ScpFormatFamily Family, string DecoderId);
