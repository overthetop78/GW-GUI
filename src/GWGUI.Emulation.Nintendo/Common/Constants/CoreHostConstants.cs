namespace GWGUI.Emulation.Nintendo.Common.Constants;

/// <summary>Identifiants, codes et délais du client Common et de son hôte de cœur.</summary>
internal static class CoreHostConstants
{
    /// <summary>Version du format binaire des échanges, contrôlée lors de la lecture de leur en-tête.</summary>
    internal const int ProtocolVersion = 5;
    /// <summary>Statut précédant les données d’une réponse réussie.</summary>
    internal const byte SuccessResponse = 1;
    /// <summary>Statut précédant le diagnostic d’une réponse en échec.</summary>
    internal const byte FailureResponse = 2;
    /// <summary>Commande lisant les images disponibles, la sélection et l’état d’éjection du lecteur.</summary>
    internal const byte GetDiskStatusCommand = 13;
    /// <summary>Commande de réinitialisation logicielle du cœur, distincte de la remise à zéro matérielle.</summary>
    internal const byte SoftResetCommand = 16;
    /// <summary>Commande affectant un périphérique émulé à un port de contrôleur.</summary>
    internal const byte SetControllerPortDeviceCommand = 15;
    /// <summary>Identifiant de plateforme utilisé par les annotations des services réservés à Windows.</summary>
    internal const string Windows = "windows";
    /// <summary>Nom de serveur désignant la machine locale dans l’API des canaux nommés.</summary>
    internal const string LocalPipeServerName = ".";
    /// <summary>Durée maximale en millisecondes pour établir la connexion au processus d’hôte.</summary>
    internal const int ConnectionTimeoutMilliseconds = 15_000;
    /// <summary>Commande enregistrant explicitement les modifications de médias prises en charge.</summary>
    internal const byte SaveMediaChangesCommand = 12;
    /// <summary>Commande demandant si des modifications de médias restent à sauvegarder.</summary>
    internal const byte HasUnsavedMediaChangesCommand = 14;
    /// <summary>Durée en millisecondes accordée à une fermeture normale avant arrêt forcé du processus.</summary>
    internal const int GracefulExitTimeoutMilliseconds = 5_000;
    /// <summary>Argument qui sélectionne le mode d’hébergement du cœur lors du lancement de l’exécutable.</summary>
    internal const string CommandLineArgument = "--nintendo-core-host";
    /// <summary>Préfixe du canal nommé de commande, suivi d’un identifiant de session unique.</summary>
    internal const string PipePrefix = "gwgui-nintendo-";
    /// <summary>Préfixe du bloc de mémoire partagée utilisé pour publier les images de la session.</summary>
    internal const string VideoMapPrefix = "gwgui-nintendo-video-";
    /// <summary>Capacité en octets des tampons de transport du canal nommé.</summary>
    internal const int PipeBufferSize = 8 * 1024 * 1024;
    /// <summary>Nom invariant de l’hôte utilisé pour identifier ses diagnostics.</summary>
    internal const string HostName = "Nintendo";
    /// <summary>Durée maximale en secondes pour recevoir une réponse complète.</summary>
    internal const int ResponseTimeoutSeconds = 30;
    /// <summary>Commande libérant le cœur puis terminant son hôte.</summary>
    internal const byte DisposeCommand = 11;
    /// <summary>Commande retirant le média du lecteur indiqué.</summary>
    internal const byte EjectMediaCommand = 6;
    /// <summary>Commande de réinitialisation matérielle de la machine émulée.</summary>
    internal const byte HardResetCommand = 3;
    /// <summary>Commande initialisant le cœur avec sa configuration et ses chemins.</summary>
    internal const byte InitializeCommand = 1;
    /// <summary>Commande montant un média dans le lecteur indiqué.</summary>
    internal const byte InsertMediaCommand = 5;
    /// <summary>Commande restaurant les octets d’un état préalablement validé par Common.</summary>
    internal const byte LoadStateCommand = 8;
    /// <summary>Commande calculant une frame et récupérant les sorties associées.</summary>
    internal const byte RunFrameCommand = 2;
    /// <summary>Commande demandant les octets de l’état courant du cœur.</summary>
    internal const byte SaveStateCommand = 7;
    /// <summary>Commande choisissant une image dans la liste de disques du cœur.</summary>
    internal const byte SelectDiskCommand = 10;
    /// <summary>Commande appliquant une option identifiée par sa clé et sa valeur.</summary>
    internal const byte SetOptionCommand = 9;
    /// <summary>Commande arrêtant le contenu sans détruire immédiatement l’hôte.</summary>
    internal const byte StopCommand = 4;
}
