namespace GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;

/// <summary>Conversion des identifiants publics des profils vers leur identité typée.</summary>
public static class ModelCatalog
{
    /// <summary>Recherche un profil par son identifiant technique exact, indépendant de sa traduction.</summary>
    /// <param name="id">Identifiant annoncé dans la liste des machines du module.</param>
    /// <returns>Identité typée du profil correspondant.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Aucun des deux profils ne possède cet identifiant.</exception>
    public static MachineModel Parse(string id) => HardwareModelCatalog.All.FirstOrDefault(item => string.Equals(item.StableModelId, id, StringComparison.Ordinal))?.Model ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
