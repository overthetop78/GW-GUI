namespace GWGUI.Emulation.Contracts;

/// <summary>Paramètres d’une distribution de cœur : archive officielle, bibliothèque dans l’archive, nom installé et nom affiché.</summary>
/// <remarks>Le module fournit ces métadonnées ; le SDK ne déduit pas de fournisseur à partir du nom de l’assembly.</remarks>
public sealed record EmulationCoreReleaseSettings(
    Uri OfficialArchiveUri,
    string ArchiveLibraryName,
    string InstalledLibraryName,
    string DisplayName)
{
    public override string ToString()
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("CoreReleaseSettings { ");
        if (PrintMembers(builder)) builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
