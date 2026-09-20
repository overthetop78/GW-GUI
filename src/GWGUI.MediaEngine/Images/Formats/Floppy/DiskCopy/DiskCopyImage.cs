
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.DiskCopy;

/// <summary>Associe une image sectorielle aux métadonnées conservées de son en-tête DiskCopy 4.2.</summary>
public sealed record DiskCopyImage(
    SectorImage Image,
    IReadOnlyList<byte> NameBytes,
    byte DiskFormat,
    byte FormatByte,
    bool? DataChecksumValid = null,
    bool? TagChecksumValid = null);
