
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.TeleDisk;

/// <summary>Réunit les enregistrements TeleDisk réinscriptibles et leur image sectorielle logique.</summary>
public sealed record Td0Image(Td0Header Header, Td0Comment? Comment, IReadOnlyList<Td0Track> Tracks, SectorImage SectorImage);
