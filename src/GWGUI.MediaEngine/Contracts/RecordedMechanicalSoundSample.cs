namespace GWGUI.MediaEngine.Contracts;

/// <summary>Un fichier source inchangé et les opérations qu’il enregistre.</summary>
internal sealed record RecordedMechanicalSoundSample(string Id, string FilePath, string FileName, int SampleRate, long Frames, int Channels, double? PeakDbfs, IReadOnlyList<RecordedMechanicalSoundOperation> Operations);
