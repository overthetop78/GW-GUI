namespace GWGUI.MediaEngine.Containers.Ibm.Raw;

/// <summary>Construit les erreurs propres aux images sectorielles brutes IBM.</summary>
internal static class IbmRawImageExceptions
{
    /// <summary>Crée l'erreur signalant une longueur non sectorielle.</summary>
    public static InvalidDataException InvalidLength(int observedLength, int sectorSize) => new($"L'image IBM contient {observedLength} octet(s), une taille non divisible par les secteurs de {sectorSize} octets.");
    /// <summary>Crée l'erreur signalant une géométrie indéterminable.</summary>
    public static InvalidDataException UnknownGeometry(int capacity) => new($"Aucun BPB valide ni aucune capacité IBM répertoriée ne permet de déterminer la géométrie des {capacity} octets.");
}
