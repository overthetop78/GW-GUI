namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Contient l'identifiant de fichier et le numéro de page décodés depuis un tag Lisa valide.</summary>
internal readonly record struct LisaPageTag
{
    /// <summary>Crée un tag Lisa décodé.</summary>
    public LisaPageTag(ushort fileId, int pageNumber)
    {
        FileId = fileId;
        PageNumber = pageNumber;
    }

    /// <summary>Identifiant du fichier auquel appartient la page.</summary>
    public ushort FileId { get; }

    /// <summary>Numéro de la page dans le fichier.</summary>
    public int PageNumber { get; }
}
