using GWGUI.Emulation.HardDisks;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class HardDiskFormats
{
    private static readonly DiskFormatRegistry Formats = DiskFormatRegistry.CreateDefault();

    /// <summary>Consulte les formats du cœur choisi, ou ceux disponibles pour le modèle avant sa sélection.</summary>
    /// <remarks>La consultation ne sélectionne aucun émulateur et ne crée aucun contenu. Les contraintes
    /// de format appartiennent aux adaptateurs ; Common conserve le raccordement au registre partagé.</remarks>
    internal static IReadOnlyList<HardDiskImageFormat> For(MachineConfiguration configuration)
    {
        if (!ConfigurationFunctions.SupportsRuntime(configuration.Model)) return [];
        var engine = new Engine();
        var selected = engine.SelectedAdapter(configuration);
        return selected is not null ? selected.GetHardDiskFormats(configuration, EmulationMediaSlot.HardDisk0)
            : engine.Adapters(configuration.MachineId).SelectMany(adapter => adapter.GetHardDiskFormats(configuration, EmulationMediaSlot.HardDisk0)).ToArray();
    }

    /// <summary>Complète le format annoncé par un adaptateur avec les contraintes du conteneur du SDK.</summary>
    /// <remarks>Le registre conserve les limites, tailles de secteur et opérations existantes sans ajouter de callback.
    /// La description ne possède aucun fichier ; les fonctions déjà présentes empruntent le flux de leur appelant.</remarks>
    internal static HardDiskImageFormat Describe(HardDiskImageFormat format) => Formats.Describe(format);
}
