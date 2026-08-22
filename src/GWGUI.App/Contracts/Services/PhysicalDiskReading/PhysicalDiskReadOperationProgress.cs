using GWGUI.App.Enums.Services.PhysicalDiskReading;
using GWGUI.MediaEngine.Exploration.Contracts;

namespace GWGUI.App.Contracts.Services.PhysicalDiskReading;

/// <summary>État commun publié pendant l'acquisition, le décodage, l'enregistrement et l'exploration.</summary>
public sealed record PhysicalDiskReadOperationProgress : IEtatLectureDisquette
{
    public PhysicalDiskReadOperationProgress(
        PhysicalDiskReadStage stage,
        int completedTracks,
        int totalTracks,
        int? cylinder = null,
        int? head = null,
        int attempt = 1,
        IReadOnlyList<PhysicalDiskTrackAddress>? tracks = null,
        IPiste? acquiredTrack = null,
        string? messageCode = null,
        IReadOnlyDictionary<string, string>? messageParameters = null,
        string? externalMessage = null)
    {
        Stage = stage;
        CompletedTracks = completedTracks;
        TotalTracks = totalTracks;
        Cylinder = cylinder;
        Head = head;
        Attempt = attempt;
        Tracks = tracks;
        PisteAcquise = acquiredTrack;
        CodeMessage = messageCode;
        ParametresMessage = messageParameters ?? new Dictionary<string, string>();
        MessageExterne = externalMessage;
        EtatsPistes = CreateTrackStates(tracks, completedTracks, cylinder, head, attempt);
    }

    public PhysicalDiskReadStage Stage { get; }
    public int CompletedTracks { get; }
    public int TotalTracks { get; }
    public int? Cylinder { get; }
    public int? Head { get; }
    public int Attempt { get; }
    public IReadOnlyList<PhysicalDiskTrackAddress>? Tracks { get; }
    public string Etape => Stage.ToString();
    public int NombrePistesTerminees => CompletedTracks;
    public int NombrePistesTotal => TotalTracks;
    public int? Cylindre => Cylinder;
    public int? Face => Head;
    public int Tentative => Attempt;
    public IPiste? PisteAcquise { get; }
    public IReadOnlyList<IEtatPisteLecture> EtatsPistes { get; }
    public string? CodeMessage { get; }
    public IReadOnlyDictionary<string, string> ParametresMessage { get; }
    public string? MessageExterne { get; }

    private static IReadOnlyList<IEtatPisteLecture> CreateTrackStates(
        IReadOnlyList<PhysicalDiskTrackAddress>? tracks,
        int completedTracks,
        int? cylinder,
        int? head,
        int attempt)
    {
        if (tracks is null)
        {
            return [];
        }

        return tracks.Select((track, index) =>
        {
            var state = index < completedTracks ? "completed" : "pending";
            var attempts = index < completedTracks ? 1 : 0;
            if (track.Cylinder == cylinder && track.Head == head && index >= completedTracks)
            {
                state = attempt > 1 ? "retry" : "reading";
                attempts = attempt;
            }

            return (IEtatPisteLecture)new TrackReadState(track.Cylinder, track.Head, state, attempts);
        }).ToArray();
    }

    private sealed record TrackReadState(
        int Cylindre,
        int Face,
        string Etat,
        int Tentatives) : IEtatPisteLecture;
}
