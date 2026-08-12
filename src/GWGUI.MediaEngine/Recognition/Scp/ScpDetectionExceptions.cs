namespace GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Construit les erreurs produites pendant la détection d'une image sectorielle SCP.</summary>
internal static class ScpDetectionExceptions
{
    /// <summary>Crée l'erreur signalant qu'aucun secteur d'une famille ou d'un format n'a pu être décodé.</summary>
    public static InvalidDataException NoDecodedSector(string formatOrFamily) => new($"Aucun secteur du format ou de la famille '{formatOrFamily}' n'a pu être décodé depuis l'image SCP.");
}
