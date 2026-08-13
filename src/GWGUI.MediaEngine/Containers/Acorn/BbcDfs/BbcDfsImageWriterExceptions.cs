namespace GWGUI.MediaEngine.Containers.Acorn.BbcDfs;

/// <summary>Construit les erreurs propres au Writer BBC DFS.</summary>
public static class BbcDfsImageWriterExceptions
{
    /// <summary>Signale un format ou une extension incompatibles.</summary>
    public static InvalidDataException UnsupportedTarget(string formatId, string extension) => new($"BBC DFS target '{formatId}' cannot be written with extension '{extension}'.");
}
