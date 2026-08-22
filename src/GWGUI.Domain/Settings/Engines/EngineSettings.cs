namespace GWGUI.Domain.Settings.Engines;

public sealed class EngineSettings
{
    public OperationEngine PhysicalRead { get; set; } = OperationEngine.GreaseweazleHostTools;
    public OperationEngine PhysicalWrite { get; set; } = OperationEngine.GreaseweazleHostTools;
    public OperationEngine Conversion { get; set; } = OperationEngine.Internal;
    public OperationEngine ExplorerRead { get; set; } = OperationEngine.Internal;
}
