namespace GWGUI.MediaEngine.Decoding.Scp.Sectors;

using GWGUI.MediaEngine.Recognition.Scp;
using GWGUI.MediaEngine.Representations.Sectors;

/// <summary>Décrit un reconstructeur SCP nommé, sa famille et sa fonction de lecture réutilisable.</summary>
internal sealed record ScpSectorImageCandidate(string Id, ScpFormatFamily Family, Func<string, string?, CancellationToken, Task<SectorImage>> ReadAsync);
