namespace GWGUI.MediaEngine.TrackImages;

/// <summary>Décrit une caractéristique sur une plage de cellules binaires.</summary>
public sealed record TrackFeature(TrackFeatureKind Kind, int BitOffset, int BitLength, string? Description = null);
