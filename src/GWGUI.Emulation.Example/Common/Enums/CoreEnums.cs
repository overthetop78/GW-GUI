namespace GWGUI.Emulation.Example.Common.Enums;

/// <summary>Commandes binaires échangées entre le client Common et le processus qui héberge le cœur.</summary>
/// <remarks>Les codes sur un octet sont définis dans CoreHostConstants et doivent être identiques des deux côtés.
/// Chaque commande détermine les arguments à lire et le résultat à écrire après son en-tête.</remarks>
internal enum HostCommand : byte
{
    /// <summary>Initialiser le cœur avec sa configuration et les chemins de sa session.</summary>
    Initialize = CoreHostConstants.InitializeCommand,
    /// <summary>Calculer une frame et récupérer les sorties vidéo, audio et de statut.</summary>
    RunFrame = CoreHostConstants.RunFrameCommand,
    /// <summary>Réinitialiser le matériel émulé en conservant les montages.</summary>
    HardReset = CoreHostConstants.HardResetCommand,
    /// <summary>Arrêter le contenu sans demander la destruction immédiate de l’hôte.</summary>
    Stop = CoreHostConstants.StopCommand,
    /// <summary>Monter le média décrit dans la requête.</summary>
    InsertMedia = CoreHostConstants.InsertMediaCommand,
    /// <summary>Retirer le média du lecteur indiqué.</summary>
    EjectMedia = CoreHostConstants.EjectMediaCommand,
    /// <summary>Récupérer les octets de l’état courant du cœur.</summary>
    SaveState = CoreHostConstants.SaveStateCommand,
    /// <summary>Restaurer des octets d’état dont Common a préalablement validé l’enveloppe.</summary>
    LoadState = CoreHostConstants.LoadStateCommand,
    /// <summary>Appliquer une option identifiée par sa clé et sa valeur.</summary>
    SetOption = CoreHostConstants.SetOptionCommand,
    /// <summary>Choisir une image parmi les disques exposés par le cœur.</summary>
    SelectDisk = CoreHostConstants.SelectDiskCommand,
    /// <summary>Libérer le cœur et terminer le processus qui le possède.</summary>
    Dispose = CoreHostConstants.DisposeCommand,
    /// <summary>Enregistrer explicitement les modifications de médias prises en charge.</summary>
    SaveMediaChanges = CoreHostConstants.SaveMediaChangesCommand,
    /// <summary>Lire les images disponibles, leur sélection et l’état d’éjection du lecteur.</summary>
    GetDiskStatus = CoreHostConstants.GetDiskStatusCommand,
    /// <summary>Demander si des changements de médias restent à enregistrer.</summary>
    HasUnsavedMediaChanges = CoreHostConstants.HasUnsavedMediaChangesCommand,
    /// <summary>Affecter le périphérique émulé à un port de contrôleur.</summary>
    SetControllerPortDevice = CoreHostConstants.SetControllerPortDeviceCommand,
    /// <summary>Demander la réinitialisation logicielle mise en œuvre par le cœur.</summary>
    SoftReset = CoreHostConstants.SoftResetCommand
}

/// <summary>État d’hébergement du cœur, distinct du cycle de vie de la machine émulée.</summary>
/// <remarks>Un processus peut rester Running lorsque la machine est en pause.
/// InProcess signifie que le cœur utilise le processus appelant et ne possède pas de processus enfant.</remarks>
public enum HostProcessState
{
    /// <summary>Le cœur s’exécute dans le processus courant.</summary>
    InProcess,
    /// <summary>Le processus séparé n’a pas encore démarré.</summary>
    NotStarted,
    /// <summary>Le processus d’hôte est actif.</summary>
    Running,
    /// <summary>La fin du processus d’hôte a été observée.</summary>
    Exited,
    /// <summary>Le démarrage, l’exécution ou la communication de l’hôte a échoué.</summary>
    Faulted
}

/// <summary>Statut précédant les données d’une réponse du protocole d’hôte.</summary>
internal enum HostResponseStatus : byte
{
    /// <summary>L’opération a réussi ; lire les données attendues pour la commande.</summary>
    Success = CoreHostConstants.SuccessResponse,
    /// <summary>L’opération a échoué ; lire le diagnostic sérialisé au lieu du résultat normal.</summary>
    Failure = CoreHostConstants.FailureResponse
}
