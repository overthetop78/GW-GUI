using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems;

/// <summary>Associe une image déjà décodée au système de fichiers qui y a été lu.</summary>
public sealed record SectorFileSystemCandidate(IMediaSectorImage Image, FileSystemMatch Match);
