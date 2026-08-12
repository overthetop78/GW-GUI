using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization;

namespace GWGUI.MediaEngine.Visualization.Policies;

/// <summary>Sélectionne un encodeur pour une liste de préfixes d'identifiants de formats.</summary>
internal sealed class PrefixVisualizationPolicy : SectorImageVisualizationPolicy
{
    private readonly string _encoderId;
    private readonly IReadOnlyList<string> _prefixes;

    /// <summary>Crée une politique de correspondance par préfixe.</summary>
    /// <param name="encoderId">Identifiant de l'encodeur.</param>
    /// <param name="prefixes">Préfixes acceptés.</param>
    /// <exception cref="ArgumentException">Un identifiant est vide ou aucun préfixe n'est fourni.</exception>
    public PrefixVisualizationPolicy(string encoderId, params string[] prefixes)
    {
        if (string.IsNullOrWhiteSpace(encoderId)) throw new ArgumentException("L'identifiant de l'encodeur est obligatoire.", nameof(encoderId));
        ArgumentNullException.ThrowIfNull(prefixes);
        if (prefixes.Length == 0 || prefixes.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("Au moins un préfixe non vide est obligatoire.", nameof(prefixes));
        _encoderId = encoderId;
        _prefixes = Array.AsReadOnly(prefixes.ToArray());
    }

    /// <inheritdoc />
    public override bool CanHandle(SectorImage image) => _prefixes.Any(prefix => image.FormatId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public override string EncoderId(SectorImage image) => _encoderId;
}
