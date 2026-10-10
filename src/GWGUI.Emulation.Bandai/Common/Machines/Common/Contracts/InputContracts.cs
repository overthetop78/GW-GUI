namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Contracts;

public sealed record InputConfiguration(IReadOnlyList<ControllerBinding>? Controllers = null);
