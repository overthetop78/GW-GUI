namespace GWGUI.App.Contracts.ViewModels.Visualization;

public sealed record ScpInspectorModel(
    int Head,
    int Cylinder,
    int ScpEntry,
    IReadOnlyList<ScpRevolutionInfo> Revolutions,
    ScpDecodeInfo? Decode,
    IReadOnlyList<ScpInspectorEntry> Structures,
    IReadOnlyList<string> Sectors)
{
    public int RevolutionCount => Revolutions.Count;
    public int SectorCount => Sectors.Count;
    public int TotalTransitions => Revolutions.Sum(item => item.Transitions);
    public double AverageRpm => Revolutions.Count == 0 ? 0 : Revolutions.Average(item => item.Rpm);
    public double AverageDurationMilliseconds => Revolutions.Count == 0
        ? 0
        : Revolutions.Average(item => item.DurationMilliseconds);
}
