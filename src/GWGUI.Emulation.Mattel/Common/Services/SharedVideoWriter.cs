using System.IO;
using System.IO.MemoryMappedFiles;
using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Mattel.Common.Constants;
using GWGUI.Emulation.Mattel.Common.Functions;

namespace GWGUI.Emulation.Mattel.Common.Services;

/// <summary>Configure le rédacteur vidéo du module et le relie à l’implémentation commune par son interface.</summary>
/// <param name="namePrefix">Préfixe unique de la session, partagé avec le client qui ouvre les segments.</param>
/// <remarks>Le service SDK conserve l’allocation et la libération des segments. Le module fournit ses règles
/// de capacité et de nommage. CoreHost possède ce relais et le libère à la fin de sa session, même après erreur.
/// Le codec reçoit l’interface ; aucune classe interne du SDK n’est héritée ou instanciée directement.</remarks>
internal sealed class SharedVideoWriter(string namePrefix) : IEmulationSharedVideoWriter
{
    /// <summary>Rédacteur possédé qui gère les segments et les vues successives de la session.</summary>
    private readonly IEmulationSharedVideoWriter _writer = CoreHostFunctions.CreateSharedVideoWriter(namePrefix, CoreHostFunctions.CalculateVideoSlotCapacity,
        CoreHostConstants.VideoMapGenerationSeparator, CoreHostConstants.UniqueNameFormat);

    /// <summary>Vue courante empruntée par le codec pendant l’écriture, ou null sans segment alloué.</summary>
    /// <remarks>Le consommateur ne ferme pas cette vue. Elle peut être remplacée par EnsureCapacity et ne
    /// doit être conservée ni après ce remplacement ni après Dispose.</remarks>
    public MemoryMappedViewAccessor? View => _writer.View;

    /// <summary>Nom du segment courant à transmettre au processus lecteur, ou null sans segment actif.</summary>
    public string? Name => _writer.Name;

    /// <summary>Capacité d’un emplacement vidéo en octets, distincte du nombre d’emplacements du segment.</summary>
    public int SlotCapacity => _writer.SlotCapacity;

    /// <summary>Garantit une capacité suffisante pour les pixels de la prochaine image.</summary>
    /// <param name="frameLength">Longueur des pixels en octets, calculée par le codec à partir de la frame.</param>
    /// <remarks>Le service conserve le segment adapté ou remplace sa génération en libérant l’ancienne.
    /// Les échecs d’allocation ferment les ressources déjà acquises selon les règles de son implémentation.</remarks>
    public void EnsureCapacity(int frameLength) => _writer.EnsureCapacity(frameLength);

    /// <summary>Détache et libère la vue ainsi que le segment possédés par le rédacteur commun.</summary>
    /// <remarks>CoreHost appelle cette opération dans sa portée de libération, après la dernière écriture.
    /// Les flux de commande reçus par les codecs restent la propriété de leur propre session.</remarks>
    public void Dispose() => _writer.Dispose();
}
