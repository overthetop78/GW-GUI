namespace GWGUI.MediaEngine.Containers.Acorn.BbcDfs;

/// <summary>Construit les erreurs de lecture SSD et DSD.</summary>
internal static class BbcDfsExceptions
{
    /// <summary>Crée l'erreur signalant une extension autre que SSD ou DSD.</summary>
    public static NotSupportedException UnknownExtension(string extension) => new($"BBC DFS extension '{extension}' is neither SSD nor DSD.");
    /// <summary>Crée l'erreur signalant une piste incomplète.</summary>
    public static InvalidDataException IncompleteTrack(int length, int heads, int trackSize) => new($"BBC DFS image contains {length} bytes; it is not a whole number of {trackSize}-byte tracks across {heads} heads.");
    /// <summary>Crée l'erreur signalant une capacité autre que 40 ou 80 cylindres.</summary>
    public static InvalidDataException UnsupportedCylinderCount(int length, int cylinders, int heads) => new($"BBC DFS image contains {length} bytes and resolves to {cylinders} cylinders across {heads} heads; expected 40 or 80 cylinders.");
}
