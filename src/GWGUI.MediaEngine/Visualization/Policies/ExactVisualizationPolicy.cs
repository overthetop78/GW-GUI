using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization;

namespace GWGUI.MediaEngine.Visualization.Policies;

/// <summary>Sélectionne un encodeur pour une liste fermée d'identifiants de formats.</summary>
internal sealed class ExactVisualizationPolicy : SectorImageVisualizationPolicy
{
    private readonly string _encoderId;
    private readonly IReadOnlyList<string> _formatIds;

    /// <summary>Crée une politique de correspondance exacte.</summary>
    /// <param name="encoderId">Identifiant de l'encodeur.</param>
    /// <param name="formatIds">Identifiants de formats acceptés.</param>
    /// <exception cref="ArgumentException">Un identifiant est vide ou aucun format n'est fourni.</exception>
    public ExactVisualizationPolicy(string encoderId, params string[] formatIds)
    {
        if (string.IsNullOrWhiteSpace(encoderId)) throw new ArgumentException("L'identifiant de l'encodeur est obligatoire.", nameof(encoderId));
        ArgumentNullException.ThrowIfNull(formatIds);
        if (formatIds.Length == 0 || formatIds.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("Au moins un identifiant de format non vide est obligatoire.", nameof(formatIds));
        _encoderId = encoderId;
        _formatIds = Array.AsReadOnly(formatIds.ToArray());
    }

    /// <inheritdoc />
    public override bool CanHandle(SectorImage image) => _formatIds.Contains(image.FormatId, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string EncoderId(SectorImage image) => _encoderId;
}
