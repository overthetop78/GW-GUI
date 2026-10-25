namespace GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;

/// <summary>Valeurs audio résolues pour la sortie de la session et les réglages de la configuration.</summary>
/// <param name="OutputDeviceId">Identifiant emprunté de sortie physique, ou null pour la sortie par défaut de l'hôte.</param>
/// <param name="LatencyMilliseconds">Latence demandée, en millisecondes, après application des règles du cœur.</param>
/// <param name="StereoSeparation">Séparation des canaux en pourcentage, conservée pour les adaptations qui exposent ce réglage.</param>
/// <remarks>Cette description ne crée ni ne possède de périphérique. Le propriétaire de la session crée et
/// libère sa sortie ; les résolutions conservent les valeurs enregistrées et les contraintes natives du cœur.
/// La séparation stéréo reste une donnée de réglage ; ce record ne transforme aucun échantillon.</remarks>
public sealed record AudioConfiguration(string? OutputDeviceId = null, int LatencyMilliseconds = AudioConstants.DefaultLatencyMilliseconds, int StereoSeparation = AudioConstants.FullStereoSeparation);
