namespace GWGUI.MediaEngine.Contracts;

/// <summary>Profil identifié par sa provenance ; les opérations absentes ne sont pas inventées.</summary>
internal sealed record RecordedMechanicalSoundProfile(string Id, string Device, string DisplayName, IReadOnlyList<RecordedMechanicalSoundSample> Samples, IReadOnlyList<string> MissingOperations);
