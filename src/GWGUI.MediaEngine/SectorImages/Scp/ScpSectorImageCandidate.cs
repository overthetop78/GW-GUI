namespace GWGUI.MediaEngine.SectorImages.Scp;

using GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Décrit un reconstructeur SCP nommé, sa famille et sa fonction de lecture réutilisable.</summary>
internal sealed record ScpSectorImageCandidate(string Id, ScpFormatFamily Family, Func<string, string?, CancellationToken, Task<SectorImage>> ReadAsync);
