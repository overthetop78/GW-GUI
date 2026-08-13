namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Décrit les identifiants techniques de systèmes et de protection associés à une image.</summary>
public sealed record DiskImageMetadata
{
    /// <summary>Initialise des métadonnées en copiant les identifiants de systèmes dans leur ordre de résolution.</summary>
    /// <param name="systemIds">Identifiants techniques ordonnés des systèmes, sans doublon.</param>
    /// <param name="protectionId">Identifiant technique de protection, ou <see langword="null"/>.</param>
    public DiskImageMetadata(IEnumerable<string> systemIds, string? protectionId, DiskContentMetadata? content = null)
    {
        SystemIds = systemIds.ToArray();
        ProtectionId = protectionId;
        Content = content ?? new(false, null, []);
    }

    /// <summary>Obtient la copie immuable et ordonnée des identifiants techniques de systèmes.</summary>
    public IReadOnlyList<string> SystemIds { get; }
    /// <summary>Obtient l'identifiant technique de protection, ou <see langword="null"/>.</summary>
    public string? ProtectionId { get; }
    /// <summary>Obtient les caractéristiques détectées directement dans le contenu.</summary>
    public DiskContentMetadata Content { get; }
}
