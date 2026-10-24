using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Décrit les images de disque dur et de carte mémoire utilisables par les dialogues communs.</summary>
/// <remarks>Le mécanisme est celui des autres modules : le profil choisit ses extensions et limites,
/// puis DiskFormatRegistry.Describe complète chaque description avec les capacités du conteneur.
/// Cette description est transmise à l'application par EmulationMediaDevice.HardDiskFormats.</remarks>
internal static class HardDiskFormats
{
    /// <summary>Descriptions stables et protégées contre le remplacement d'éléments par l'appelant.</summary>
    private static readonly IReadOnlyList<HardDiskImageFormat> Formats = CreateFormats();
    /// <summary>Images brutes de carte mémoire ; minimum, maximum et valeur initiale fixent la même capacité.</summary>
    /// <remarks>La simulation emploie des octets nuls pour une carte neuve. Un backend réel peut fournir
    /// InitializeBlankImage pour écrire son en-tête natif dans le flux qui reste possédé par le service de création.</remarks>
    private static readonly IReadOnlyList<HardDiskImageFormat> MemoryCardFormats = Array.AsReadOnly(MediaConstants.MemoryCardExtensions.Select(extension => new HardDiskImageFormat(
        StorageSettingsFunctionsConstants.FormatIdPrefix + StorageSettingsFunctionsConstants.MemoryCardModel + extension, extension,
        StorageSettingsFunctionsConstants.MemoryCardModel, StorageSettingsFunctionsConstants.MemoryCardSize, StorageSettingsFunctionsConstants.MemoryCardSize)
        { MinimumBytes = StorageSettingsFunctionsConstants.MemoryCardSize }).ToArray());

    /// <summary>Retourne les formats du profil lorsqu'il expose un disque dur.</summary>
    /// <param name="model">Identité du profil matériel sélectionné.</param>
    /// <returns>Formats du profil A ; collection vide pour un profil sans disque dur.</returns>
    /// <remarks>Le choix de l'émulateur n'intervient pas : les trois simulations utilisent les mêmes lecteurs.</remarks>
    internal static IReadOnlyList<HardDiskImageFormat> For(MachineModel model) => HardwareModelCatalog.Get(model).Storage.Contains(HardwareStorageCapability.HardDisk) ? Formats : [];

    /// <summary>Retourne les descriptions d’images nécessaires au dialogue commun de carte mémoire.</summary>
    /// <param name="model">Profil dont la capacité de carte mémoire est consultée.</param>
    /// <returns>Formats bruts à capacité fixe pour le profil B ; liste vide sans lecteur de carte mémoire.</returns>
    internal static IReadOnlyList<HardDiskImageFormat> ForMemoryCard(MachineModel model) => HardwareModelCatalog.Get(model).Storage.Contains(HardwareStorageCapability.MemoryCard) ? MemoryCardFormats : [];

    /// <summary>Construit les descriptions correspondant aux extensions déjà acceptées par le module.</summary>
    /// <returns>Une description par extension, enrichie par les limites et opérations du registre.</returns>
    /// <remarks>Les extensions VHD et VHDX choisissent ici le conteneur à créer. Les autres choix produisent
    /// une image brute. Une image vide illustre la préparation commune ; les partitions et systèmes de fichiers
    /// peuvent être proposés par un profil en renseignant ses préparations prises en charge.</remarks>
    private static IReadOnlyList<HardDiskImageFormat> CreateFormats()
    {
        var registry = DiskFormatRegistry.CreateDefault();
        return Array.AsReadOnly(MediaConstants.HardDiskExtensions.Select(extension => registry.Describe(new HardDiskImageFormat(
            StorageSettingsFunctionsConstants.FormatIdPrefix + extension.TrimStart(MediaConstants.ExtensionPrefix), extension,
            StorageSettingsFunctionsConstants.InterfaceName, StorageSettingsFunctionsConstants.MaximumBytes, StorageSettingsFunctionsConstants.DefaultBytes)
        {
            Container = extension switch { MediaConstants.VhdExtension => DiskContainerKind.Vhd, MediaConstants.VhdxExtension => DiskContainerKind.Vhdx, _ => DiskContainerKind.Raw }
        })).ToArray());
    }
}
