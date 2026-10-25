using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Example.Common.Enums;

namespace GWGUI.Emulation.Example.Common.Contracts;

/// <summary>Géométrie de la dernière image effectivement produite par le cœur.</summary>
/// <param name="Width">Largeur de l’image en pixels.</param>
/// <param name="Height">Hauteur de l’image en pixels.</param>
/// <param name="Pitch">Distance en octets entre deux lignes de pixels.</param>
/// <param name="AspectRatio">Rapport largeur sur hauteur prévu pour l’affichage.</param>
public sealed record RuntimeGeometry(int Width, int Height, int Pitch, float AspectRatio);

/// <summary>Instantané des mesures et activités du cœur, indépendant de sa configuration enregistrée.</summary>
/// <param name="Model">Identité du modèle en cours d’exécution.</param>
/// <param name="Region">Standard vidéo annoncé par le cœur, ou null lorsqu’il est inconnu.</param>
/// <param name="FramesPerSecond">Cadence vidéo en images par seconde.</param>
/// <param name="SampleRate">Fréquence des échantillons audio en hertz.</param>
/// <param name="Geometry">Dernière géométrie vidéo connue, ou null avant la première image.</param>
/// <param name="CoreName">Nom technique annoncé par le cœur.</param>
/// <param name="MediaActivity">Activité des lecteurs après interprétation des indicateurs natifs.</param>
/// <param name="LedStates">Valeurs brutes des indicateurs natifs, indexées par leur identifiant.</param>
/// <param name="BufferedAudioFrames">Nombre de frames audio en attente de lecture.</param>
/// <param name="AudioOverrunCount">Nombre de dépassements de capacité du tampon audio.</param>
/// <param name="AudioUnderrunCount">Nombre de manques de données dans ce tampon.</param>
/// <param name="HostProcessState">État du processus qui héberge le cœur.</param>
/// <param name="HostProcessId">Identifiant du processus lorsqu’il existe.</param>
/// <remarks>Les dictionnaires sont des instantanés stables fournis par leur propriétaire ; ce record
/// ne possède ni processus, ni image native, ni sortie audio.</remarks>
public sealed record RuntimeStatus(MachineModel Model, EmulationVideoStandard? Region, double FramesPerSecond, int SampleRate, RuntimeGeometry? Geometry, string CoreName,
    IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity, IReadOnlyDictionary<int, bool> LedStates, int BufferedAudioFrames, long AudioOverrunCount,
    long AudioUnderrunCount, HostProcessState HostProcessState, int? HostProcessId);
