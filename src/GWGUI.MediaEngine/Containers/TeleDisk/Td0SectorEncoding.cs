namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Identifie l'encodage d'une charge utile sectorielle TeleDisk.</summary>
internal enum Td0SectorEncoding : byte
{
    /// <summary>Données brutes.</summary>
    Raw = 0,
    /// <summary>Motif de deux octets répété.</summary>
    RepeatedWord = 1,
    /// <summary>Suites littérales ou motifs de mots répétés.</summary>
    Rle = 2
}
