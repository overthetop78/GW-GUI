namespace GWGUI.Emulation.Snk.Common.Machines.Common.Contracts;

public sealed record InputConfiguration(IReadOnlyList<ControllerBinding>? Controllers = null);
