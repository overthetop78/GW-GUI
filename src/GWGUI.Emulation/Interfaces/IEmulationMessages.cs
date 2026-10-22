namespace GWGUI.Emulation.Interfaces;

/// <summary>Contextualisation commune des échecs de machine pour l’application hôte.</summary>
/// <remarks>Les annulations et erreurs déjà contextualisées conservent leur identité ; le service ne possède aucune session.</remarks>
public interface IEmulationMessages
{
    /// <summary>Construit l’erreur de cœur absent avec l’identité publique de l’émulateur concerné.</summary>
    EmulationMessageException EmulatorNotInstalled(string emulatorId);
    /// <summary>Construit une erreur destinée au dialogue de l’hôte avec sa catégorie, son code et sa cause facultative.</summary>
    EmulationMessageException CreateError(EmulationMessageCategory category, EmulationMessageCode code, Exception? innerException);

    /// <summary>Associe un diagnostic public à une cause qui n’en possède pas encore.</summary>
    /// <param name="error">Cause conservée comme exception interne lorsque la contextualisation est nécessaire.</param>
    /// <param name="fallbackCategory">Domaine fonctionnel employé pour une cause non contextualisée.</param>
    /// <param name="fallbackCode">Code public permettant à l’hôte de choisir sa traduction.</param>
    /// <param name="context">Identités et valeurs décrites par les interfaces de contexte communes.</param>
    /// <returns>Erreur existante ou erreur contextualisée par le service commun.</returns>
    Exception Translate(Exception error, EmulationMessageCategory fallbackCategory, EmulationMessageCode fallbackCode, IEmulationMessageContext? context = null);
    /// <summary>Conserve un message déjà localisé par le module dans l’enveloppe publique.</summary>
    /// <param name="error">Cause dont le texte a déjà été traduit.</param>
    /// <param name="category">Domaine fonctionnel de cette cause.</param>
    /// <param name="context">Contexte public de la machine ou du cœur.</param>
    /// <returns>Erreur prête à être présentée sans retraduire son texte.</returns>
    Exception TranslateLocalized(Exception error, EmulationMessageCategory category, IEmulationMessageContext? context = null);
    /// <summary>Construit une demande de média avec les catégories attendues par la machine.</summary>
    /// <param name="machineId">Identité stable du profil concerné.</param>
    /// <param name="requiredMedia">Types d’emplacements pouvant satisfaire cette demande.</param>
    /// <param name="innerException">Cause facultative de la demande de média.</param>
    /// <returns>Exception publique dont le contexte permet à l’hôte de proposer les bons lecteurs.</returns>
    Exception RequiredMediaMissing(string machineId, IReadOnlyList<EmulationMediaCategory> requiredMedia, Exception? innerException);
}
