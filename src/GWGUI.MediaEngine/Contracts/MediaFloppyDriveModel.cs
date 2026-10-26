namespace GWGUI.MediaEngine.Contracts;

/// <summary>Description publique d'un lecteur ; géométries et règles restent possédées par MediaEngine.</summary>
public sealed record MediaFloppyDriveModel(string Id, string DisplayName, long Capacity, IReadOnlyList<string> AcceptedExtensions, IReadOnlyDictionary<string, string> BlankFormats);
