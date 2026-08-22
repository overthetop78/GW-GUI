namespace GWGUI.App.Contracts.ViewModels.Visualization;

public sealed record ScpDecodeInfo(
    string Decoder,
    double Confidence,
    double CellTicks,
    int StructureCount,
    int Revolution);
