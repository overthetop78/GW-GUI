namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Décrit l'état d'un secteur enregistré dans un conteneur TeleDisk.</summary>
[Flags]
public enum Td0SectorFlags : byte
{
    /// <summary>Aucun drapeau.</summary>
    None = 0,
    /// <summary>Les données ont été lues avec une erreur de CRC.</summary>
    DataCrcError = 0x02,
    /// <summary>Le secteur porte une marque de données supprimées.</summary>
    DeletedData = 0x04,
    /// <summary>Les données ont été omises selon l'allocation DOS.</summary>
    DataUnavailable = 0x10,
    /// <summary>Le secteur possède un identifiant mais aucune donnée.</summary>
    DataSkipped = 0x20,
    /// <summary>Masque des drapeaux indiquant l'absence de charge utile.</summary>
    UnavailableMask = DataUnavailable | DataSkipped
}
