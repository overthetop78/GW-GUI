namespace GWGUI.Emulation.Enums;

/// <summary>Résultat de lecture et de reconnaissance d'un candidat au firmware.</summary>
public enum EmulationFirmwareDetectionStatus
{
    /// <summary>Le candidat correspond à une identité connue du catalogue ou de l'inspecteur.</summary>
    Known,
    /// <summary>Le candidat est lisible mais aucune règle ne reconnaît son identité.</summary>
    Unknown,
    /// <summary>Le candidat n'a pas pu être lu ; son diagnostic reste dans les observations.</summary>
    Unreadable
}
