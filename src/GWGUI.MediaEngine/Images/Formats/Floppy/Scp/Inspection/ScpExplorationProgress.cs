namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;

/// <summary>Étape réelle publiée pendant l'analyse complète d'une capture SCP.</summary>
public enum ScpExplorationProgressKind
{
    FormatProbeStarted,
    FormatProbeCompleted,
    CandidateStarted,
    RevolutionDecoded,
    TrackDecoded,
    CandidateCompleted
}

/// <summary>Décrit une unité d'analyse SCP terminée et le total correspondant.</summary>
public sealed record ScpExplorationProgress(
    ScpExplorationProgressKind Kind,
    string Detail,
    int Completed,
    int Total,
    bool? Recognized = null);
