using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Dimensions de la dernière image utilisée pour décrire le statut vidéo de la machine.</summary>
/// <param name="Width">Largeur de l’image en pixels.</param>
/// <param name="Height">Hauteur de l’image en pixels.</param>
/// <param name="Pitch">Nombre d’octets entre les débuts de deux lignes successives, incluant leur éventuel remplissage.</param>
/// <param name="AspectRatio">Rapport d’affichage largeur sur hauteur annoncé par le cœur.</param>
public sealed record RuntimeGeometry(int Width, int Height, int Pitch, float AspectRatio);

/// <summary>Instantané des informations d’exécution exposées par la machine et son cœur.</summary>
/// <param name="Model">Modèle matériel effectif de la session.</param>
/// <param name="Region">Standard vidéo annoncé, ou null lorsqu’il n’est pas disponible.</param>
/// <param name="FramesPerSecond">Cadence du cœur en images par seconde.</param>
/// <param name="SampleRate">Fréquence audio en hertz.</param>
/// <param name="Geometry">Dimensions de la dernière image disponible, ou null avant une image exploitable.</param>
/// <param name="CoreName">Nom annoncé par le cœur courant.</param>
/// <param name="MediaActivity">Activité des lecteurs logiques de la machine.</param>
/// <param name="LedStates">États des témoins d’activité indexés tels qu’ils sont exposés par le cœur.</param>
/// <param name="BufferedAudioFrames">Nombre de frames audio en attente, indépendant du nombre de canaux par frame.</param>
/// <param name="AudioOverrunCount">Compteur de débordement audio signalé par le cœur.</param>
/// <param name="AudioUnderrunCount">Compteur de manque de données audio signalé par le cœur.</param>
/// <param name="HostProcessState">État d’hébergement, distinct de l’état actif ou en pause de la machine.</param>
/// <param name="HostProcessId">Identifiant du processus séparé lorsqu’il est exposé, sinon null.</param>
/// <remarks>Ce contrat transporte des valeurs de statut et ne possède pas le processus décrit.
/// Ses dictionnaires doivent rester stables pendant leur consultation.</remarks>
public sealed record RuntimeStatus(MachineModel Model, EmulationVideoStandard? Region, double FramesPerSecond, int SampleRate, RuntimeGeometry? Geometry,
    string CoreName, IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity, IReadOnlyDictionary<int, bool> LedStates, int BufferedAudioFrames,
    long AudioOverrunCount, long AudioUnderrunCount, HostProcessState HostProcessState, int? HostProcessId);

