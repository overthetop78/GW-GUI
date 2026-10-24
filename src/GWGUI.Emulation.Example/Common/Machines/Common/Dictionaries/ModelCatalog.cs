namespace GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;

/// <summary>Conversion des identifiants publics des profils vers leur identité typée.</summary>
public static class ModelCatalog
{
    /// <summary>Recherche un profil par son identifiant technique exact, indépendant de sa traduction.</summary>
    /// <param name="id">Identifiant annoncé dans la liste des machines du module.</param>
    /// <returns>Identité typée du profil correspondant.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Aucun des deux profils ne possède cet identifiant.</exception>
    public static MachineModel Parse(string id) => HardwareModelCatalog.All.FirstOrDefault(item => string.Equals(item.StableModelId, id, StringComparison.Ordinal))?.Model ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);
    /// <summary>Projette les contraintes du profil et du backend à partir des descriptions déjà déclarées.</summary>
    /// <param name="configuration">Configuration empruntée donnant les identités de machine et de cœur.</param>
    /// <returns>Instantané de compatibilité ; ses listes sont protégées contre la modification.</returns>
    /// <remarks>HardwareModelCatalog reste propriétaire des caractéristiques matérielles.
    /// StorageSettingsFunctions fournit les mêmes lecteurs que l’éditeur de stockage, et Engine fournit
    /// la visibilité commune des onglets. La projection ne crée aucune session ni aucun fichier.</remarks>
    internal static CompatibilityDefinition GetCompatibility(MachineConfiguration configuration)
    {
        var model = HardwareModelCatalog.Get(configuration.Model);
        var devices = StorageSettingsFunctions.DescribeDevices(configuration.Model);
        var tabs = new Engine().DefaultVisibility.Tabs.Where(item => item.Value).Select(item => item.Key).ToArray();
        var media = model.Media.Select(category => new MediaCompatibilityRule(category,
            Array.AsReadOnly(devices.Where(device => device.MediaType == EmulationMediaConversionFunctions.ToType(category)).Select(device => device.Slot).ToArray()))).ToArray();
        return new(model.Model, configuration.Core, Array.AsReadOnly(tabs), model.Firmware, Array.AsReadOnly(media), ConfigurationFunctions.ControllerPortCount(configuration));
    }
}
