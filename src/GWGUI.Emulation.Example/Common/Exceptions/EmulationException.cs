namespace GWGUI.Emulation.Example.Common.Exceptions;

/// <summary>Exception d’émulation portant les catégories et codes du module dans le contrat générique du SDK.</summary>
/// <remarks>Le contrat de base conserve Category, Code, Context et IsLocalized, ainsi que la cause initiale.
/// Le récepteur reconnaît le problème par ses codes et utilise le contexte pour ses données particulières,
/// indépendamment de la langue du message. Cette spécialisation conserve ces fonctions sans recopier leur stockage.</remarks>
public sealed class EmulationException : GWGUI.Emulation.Exceptions.EmulationException<ErrorCategory, ErrorCode>
{
    /// <summary>Construit un diagnostic typé en conservant le message et la cause de l’échec.</summary>
    /// <param name="category">Domaine fonctionnel concerné : cœur, firmware, contenu, option, hôte ou état.</param>
    /// <param name="code">Motif technique stable utilisé pour reconnaître le problème.</param>
    /// <param name="message">Message ou clé de ressource interprété selon isLocalized.</param>
    /// <param name="context">Dictionnaire de diagnostic conservé par référence ; null produit un contexte vide.</param>
    /// <param name="innerException">Cause initiale facultative, conservée pour sa trace et ses détails.</param>
    /// <param name="isLocalized">True si le message est déjà traduit ; false si sa résolution reste à effectuer.</param>
    /// <remarks>Le contexte doit rester stable après construction : l’exception ne copie pas le dictionnaire fourni.</remarks>
    public EmulationException(ErrorCategory category, ErrorCode code, string message, IReadOnlyDictionary<string, string>? context = null, Exception? innerException = null, bool isLocalized = false)
        : base(category, code, message, context, innerException, isLocalized)
    {
    }
}
