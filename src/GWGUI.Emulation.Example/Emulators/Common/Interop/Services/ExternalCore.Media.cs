using System.IO;

namespace GWGUI.Emulation.Example.Emulators.Common.Interop.Services;

/// <summary>Montages des cinq catégories de médias de démonstration ; les contenus utilisateur restent empruntés.</summary>
internal sealed partial class ExternalCore
{
    /// <summary>Montages actifs indexés par lecteur logique ; l’éjection retire effectivement leur entrée.</summary>
    private readonly Dictionary<EmulationMediaSlot, MediaConfiguration> _mounted = [];
    /// <summary>Décrit les LED des emplacements actuellement montés dans la simulation.</summary>
    /// <remarks><para>Chaque identifiant est celui du protocole de l&apos;emplacement. Le mock annonce une activité inactive ; le nouveau dictionnaire ne commande aucun périphérique physique.</para></remarks>
    public IReadOnlyDictionary<int, bool> LedStates => _mounted.Keys.ToDictionary(slot => slot.ProtocolValue, _ => false);

    /// <summary>Valide le lecteur et l’existence du contenu puis enregistre son montage sans ouvrir de flux persistant.</summary>
    /// <param name="media">Description du montage à insérer ; le fichier de média reste possédé par l&apos;utilisateur.</param>
    public void InsertMedia(MediaConfiguration media)
    {
        EnsureReady();
        var device = StorageSettingsFunctions.DescribeDevices(_configuration!.Model).SingleOrDefault(item => item.Slot == media.Slot);
        if (device is null || device.MediaType != EmulationMediaConversionFunctions.ToType(media.Category)) throw new ArgumentException(nameof(media));
        if (!File.Exists(media.Path) && !Directory.Exists(media.Path)) throw new FileNotFoundException(null, media.Path);
        _mounted[media.Slot] = media with { IsInserted = true };
    }
    /// <summary>Retire le montage du lecteur sans supprimer ni modifier le fichier source.</summary>
    /// <param name="slot">Emplacement du média à consulter ou à commander, selon son identité de protocole.</param>
    public void EjectMedia(EmulationMediaSlot slot) { EnsureReady(); _mounted.Remove(slot); }
    /// <summary>Expose l’image de disquette actuellement montée ; les autres lecteurs gardent leurs propres slots.</summary>
    /// <returns>Instantané du jeu de disques courant, de sa sélection et de l&apos;état d&apos;éjection.</returns>
    public DiskStatus GetDiskStatus()
    {
        EnsureReady();
        var images = _mounted.Values.Where(media => media.Category == MediaCategory.Floppy).OrderBy(media => media.Slot.ProtocolValue).Select((media, index) => new DiskImageStatus(index, media.Path, media.Label)).ToArray();
        return new(images.Length, images.Length == 0 ? -1 : 0, images.Length == 0, images);
    }
    /// <summary>Valide la seule image du lecteur fictif ; aucune sélection inexistante n’est acceptée.</summary>
    /// <param name="index">Index de la sélection ou de la sous-commande dans son protocole, conservé sans conversion en numéro affiché.</param>
    public void SelectDisk(int index) { if (index < 0 || index >= GetDiskStatus().ImageCount) throw new ArgumentOutOfRangeException(nameof(index)); }
    /// <summary>Le simulateur n’altère aucun média utilisateur ; il ne possède donc aucun changement à écrire.</summary>
    /// <param name="slot">Emplacement du média à consulter ou à commander, selon son identité de protocole.</param>
    /// <returns>False pour ce simulateur, qui ne modifie pas les fichiers de médias de l&apos;utilisateur.</returns>
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) { EnsureReady(); return false; }
    /// <summary>Vérifie le lecteur demandé ; aucun changement de média n’est produit par ce simulateur.</summary>
    /// <param name="slot">Emplacement du média à consulter ou à commander, selon son identité de protocole.</param>
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        EnsureReady();
        if (!_mounted.ContainsKey(slot)) throw new InvalidOperationException(nameof(slot));
    }
}
