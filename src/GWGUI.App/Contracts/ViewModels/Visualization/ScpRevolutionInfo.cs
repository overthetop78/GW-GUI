namespace GWGUI.App.Contracts.ViewModels.Visualization;

public sealed record ScpRevolutionInfo(
    int Number,
    int Transitions,
    double DurationMilliseconds,
    double Rpm);
