using System.IO;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

internal static class SpartaDosFileSystemExceptions
{
    public static InvalidDataException Unsupported(string formatId) => new($"L'image '{formatId}' ne contient pas un volume SpartaDOS valide.");
    public static InvalidDataException MissingSector(int sectorNumber) => new($"Le secteur SpartaDOS {sectorNumber} est absent.");
    public static InvalidDataException InvalidSectorReference(int sectorNumber, int totalSectors) => new($"Le secteur SpartaDOS {sectorNumber} est hors de la plage 1..{totalSectors}.");
    public static InvalidDataException SectorMapLoop(int sectorNumber) => new($"La chaîne des cartes de secteurs SpartaDOS boucle sur le secteur {sectorNumber}.");
    public static InvalidDataException SectorMapBackLink(int sectorNumber, int expected, int observed) => new($"La carte de secteurs SpartaDOS {sectorNumber} référence le prédécesseur {observed} au lieu de {expected}.");
    public static InvalidDataException DirectoryLoop(int sectorNumber) => new($"L'arborescence SpartaDOS boucle sur le répertoire dont la carte commence au secteur {sectorNumber}.");
    public static InvalidDataException InvalidDirectoryLength(int sectorNumber, int length) => new($"Le répertoire SpartaDOS du secteur {sectorNumber} annonce une taille invalide de {length} octets.");
}
