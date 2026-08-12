using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Résout les identifiants techniques des protections reconnues.</summary>
internal sealed class DiskProtectionResolver
{
    /// <summary>Recherche une protection dans les identifiants de formats détectés.</summary>
    /// <param name="formatIds">Identifiants techniques des formats détectés.</param>
    /// <returns>L'identifiant technique de la protection, ou <see langword="null"/> lorsqu'aucune protection n'est reconnue.</returns>
    public string? ResolveId(IEnumerable<string> formatIds) => formatIds.Any(id => id.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase)) ? DiskImageFormatIds.AppleIIRwts18 : null;
}
