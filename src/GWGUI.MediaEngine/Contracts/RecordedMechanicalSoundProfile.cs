namespace GWGUI.MediaEngine.Contracts;

/// <summary>Modèle de lecteur et fichiers audio utilisés pour ses actions mécaniques.</summary>
internal sealed record RecordedMechanicalSoundProfile(string Id, string Device, string DisplayName, IReadOnlyList<RecordedMechanicalSoundSample> Samples);
