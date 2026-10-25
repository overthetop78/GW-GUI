namespace GWGUI.Emulation.Interfaces;

/// <summary>Messages et politique de capture fournis par le module à son contrôle de disques.</summary>
/// <remarks>La politique reste empruntée. Ses messages sont lus seulement lorsque le traitement existant
/// produit l’erreur correspondante, afin de conserver le contexte et la langue au moment de l’échec.</remarks>
public interface IEmulationDiskControlPolicy
{
    /// <summary>Message si les callbacks de contrôle ne sont pas disponibles.</summary>
    string UnavailableMessage { get; }
    /// <summary>Message si une table native ne fournit pas tous les callbacks obligatoires.</summary>
    string IncompleteMessage { get; }
    /// <summary>Message si le cœur refuse l’éjection.</summary>
    string EjectFailedMessage { get; }
    /// <summary>Message si le cœur refuse la sélection d’image pendant une insertion.</summary>
    string SelectFailedMessage { get; }
    /// <summary>Message si le cœur refuse l’insertion.</summary>
    string InsertFailedMessage { get; }
    /// <summary>Message si le cœur refuse la création d’un emplacement.</summary>
    string CreateSlotFailedMessage { get; }
    /// <summary>Message si le cœur refuse le remplacement du contenu d’un emplacement.</summary>
    string ReplaceFailedMessage { get; }
    /// <summary>Message si le média demandé ne passe pas la validation du contrôle.</summary>
    string MediaMissingMessage { get; }
    /// <summary>Message de sélection explicite, avec le même repli sur le message de sélection habituel.</summary>
    string RequestedSelectFailedMessage => SelectFailedMessage;
    /// <summary>Message de réinsertion après une sélection explicite, avec le même repli sur le message habituel.</summary>
    string RequestedInsertFailedMessage => InsertFailedMessage;
    /// <summary>True pour détacher les callbacks lorsqu’une table de base entièrement vide est reçue ; false pour la validation habituelle.</summary>
    bool ResetOnEmptyCapture => false;
}
