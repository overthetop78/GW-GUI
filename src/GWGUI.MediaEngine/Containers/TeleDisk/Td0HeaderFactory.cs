using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Construit les métadonnées TeleDisk absentes d'une image sectorielle générique.</summary>
internal static class Td0HeaderFactory
{
    /// <summary>Crée un en-tête cohérent avec la géométrie de l'image.</summary>
    public static Td0Header Create(SectorImage image) => new(0, 0, Td0Format.DefaultVersion, Td0Format.DefaultDataRate, ResolveDriveType(image), 0, 0, checked((byte)image.Heads));

    private static byte ResolveDriveType(SectorImage image)
    {
        if (image.Cylinders <= 40 && image.Heads <= 2) return 1;
        if (image.Capacity <= 737_280) return 3;
        if (image.Capacity <= 1_228_800) return 2;
        return 4;
    }
}
